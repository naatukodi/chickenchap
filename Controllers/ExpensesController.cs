using ChickenChap.Api.Domain;
using ChickenChap.Api.Dtos;
using ChickenChap.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ChickenChap.Api.Controllers;

[ApiController]
[Route("api/expenses")]
public class ExpensesController(
    ICosmosRepository<Expense> repo,
    IStorageService storageService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create([FromForm] CreateExpenseDto dto)
    {
        // ✅ Validate required fields
        if (string.IsNullOrWhiteSpace(dto.FarmId))
            return BadRequest(new { error = "FarmId is required" });

        if (string.IsNullOrWhiteSpace(dto.Date))
            return BadRequest(new { error = "Date is required (format: yyyy-MM-dd)" });

        if (string.IsNullOrWhiteSpace(dto.Category))
            return BadRequest(new { error = "Category is required" });

        if (dto.Amount <= 0)
            return BadRequest(new { error = "Amount must be greater than 0" });

        // ✅ Validate and parse date
        if (!DateOnly.TryParse(dto.Date, out var parsedDate))
            return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd (e.g., 2025-12-23)" });

        try
        {
            var e = new Expense
            {
                pk = dto.FarmId,
                FarmId = dto.FarmId,
                Date = parsedDate,
                Category = dto.Category,
                Amount = dto.Amount,
                Vendor = dto.Vendor,
                PaymentMode = dto.PaymentMode,
                ReceiptLink = dto.ReceiptLink,
                Notes = dto.Notes
            };

            // ✅ Upload images if provided
            if (dto.Images?.Count > 0)
            {
                var folderPath = $"{dto.FarmId}/{DateOnly.FromDateTime(DateTime.UtcNow):yyyy-MM-dd}";
                e.ImageUrls = await storageService.UploadFilesAsync(folderPath, dto.Images);
            }

            var created = await repo.CreateAsync(e, e.pk);
            return CreatedAtAction(
                nameof(GetById), 
                new { id = created.id, farmId = created.pk }, 
                ExpenseDto.From(created));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create expense", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseDto>> GetById(string id, [FromQuery] string farmId)
    {
        // ✅ Validate required parameters
        if (string.IsNullOrWhiteSpace(farmId))
            return BadRequest(new { error = "farmId is required as query parameter" });

        try
        {
            var found = await repo.GetAsync(id, farmId);
            if (found is null) 
                return NotFound(new { error = "Expense not found" });

            // ✅ NEW: Generate fresh SAS URLs for each image dynamically
            if (found.ImageUrls?.Count > 0)
            {
                found.ImageUrls = found.ImageUrls
                    .Select(url => storageService.GetSasUrl(url))
                    .ToList();
            }

            return Ok(ExpenseDto.From(found));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve expense", details = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> Query(
        [FromQuery] string farmId, 
        [FromQuery] string? from = null, 
        [FromQuery] string? to = null, 
        [FromQuery] string? category = null)
    {
        // ✅ Validate required parameters
        if (string.IsNullOrWhiteSpace(farmId))
            return BadRequest(new { error = "farmId is required as query parameter" });

        // ✅ Validate date ranges if provided
        if (!string.IsNullOrWhiteSpace(from) && !DateOnly.TryParse(from, out _))
            return BadRequest(new { error = "Invalid 'from' date format. Use yyyy-MM-dd" });

        if (!string.IsNullOrWhiteSpace(to) && !DateOnly.TryParse(to, out _))
            return BadRequest(new { error = "Invalid 'to' date format. Use yyyy-MM-dd" });

        try
        {
            var where = "c.pk = @farmId AND c.docType = 'expense'";
            if (!string.IsNullOrWhiteSpace(from)) where += " AND c.Date >= @from";
            if (!string.IsNullOrWhiteSpace(to)) where += " AND c.Date <= @to";
            if (!string.IsNullOrWhiteSpace(category)) where += " AND c.Category = @cat";

            var sql = $"SELECT * FROM c WHERE {where} ORDER BY c.Date DESC";
            var list = new List<ExpenseDto>();
            
            await foreach (var e in repo.QueryAsync(
                sql, 
                ("@farmId", farmId), 
                ("@from", from ?? ""), 
                ("@to", to ?? ""), 
                ("@cat", category ?? "")))
            {
                // ✅ NEW: Generate fresh SAS URLs for each image dynamically
                if (e.ImageUrls?.Count > 0)
                {
                    e.ImageUrls = e.ImageUrls
                        .Select(url => storageService.GetSasUrl(url))
                        .ToList();
                }

                list.Add(ExpenseDto.From(e));
            }
            
            return Ok(list);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to query expenses", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExpenseDto>> Update(
        string id, 
        [FromQuery] string farmId, 
        [FromForm] UpdateExpenseDto dto)
    {
        // ✅ Validate required parameters
        if (string.IsNullOrWhiteSpace(farmId))
            return BadRequest(new { error = "farmId is required as query parameter" });

        if (string.IsNullOrWhiteSpace(dto.Date))
            return BadRequest(new { error = "Date is required (format: yyyy-MM-dd)" });

        if (string.IsNullOrWhiteSpace(dto.Category))
            return BadRequest(new { error = "Category is required" });

        if (dto.Amount <= 0)
            return BadRequest(new { error = "Amount must be greater than 0" });

        // ✅ Validate and parse date
        if (!DateOnly.TryParse(dto.Date, out var parsedDate))
            return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd (e.g., 2025-12-23)" });

        try
        {
            var existing = await repo.GetAsync(id, farmId);
            if (existing is null) 
                return NotFound(new { error = "Expense not found" });

            // Update expense details
            existing.UpdatedUtc = DateTime.UtcNow;
            existing.Date = parsedDate;
            existing.Category = dto.Category;
            existing.Amount = dto.Amount;
            existing.Vendor = dto.Vendor;
            existing.PaymentMode = dto.PaymentMode;
            existing.ReceiptLink = dto.ReceiptLink;
            existing.Notes = dto.Notes;

            // ✅ GRANULAR IMAGE MANAGEMENT
            
            // 1) Determine which images to keep (use clean URLs without SAS)
            var imagesToKeep = dto.ImagesToKeep ?? new List<string>();
            
            // 2) Delete images NOT in ImagesToKeep list
            var imagesToDelete = existing.ImageUrls
                .Where(url => !imagesToKeep.Contains(url))
                .ToList();

            if (imagesToDelete.Count > 0)
            {
                await storageService.DeleteFilesAsync(imagesToDelete);
            }

            // 3) Update ImageUrls to only kept images
            existing.ImageUrls = new List<string>(imagesToKeep);

            // 4) Add new images if provided
            if (dto.ImagesToAdd?.Count > 0)
            {
                var folderPath = $"{farmId}/{DateOnly.FromDateTime(DateTime.UtcNow):yyyy-MM-dd}";
                var newImageUrls = await storageService.UploadFilesAsync(folderPath, dto.ImagesToAdd);
                existing.ImageUrls.AddRange(newImageUrls);
            }

            var saved = await repo.UpsertAsync(existing, farmId);
            
            // ✅ NEW: Generate fresh SAS URLs for response
            if (saved.ImageUrls?.Count > 0)
            {
                saved.ImageUrls = saved.ImageUrls
                    .Select(url => storageService.GetSasUrl(url))
                    .ToList();
            }

            return Ok(ExpenseDto.From(saved));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to update expense", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, [FromQuery] string farmId)
    {
        // ✅ Validate required parameters
        if (string.IsNullOrWhiteSpace(farmId))
            return BadRequest(new { error = "farmId is required as query parameter" });

        try
        {
            // ✅ Clean up images before deleting
            var existing = await repo.GetAsync(id, farmId);
            if (existing is null)
                return NotFound(new { error = "Expense not found" });

            if (existing.ImageUrls?.Count > 0)
            {
                await storageService.DeleteFilesAsync(existing.ImageUrls);
            }

            await repo.DeleteAsync(id, farmId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to delete expense", details = ex.Message });
        }
    }
}
