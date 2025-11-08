// Controllers/ExpensesController.cs
using ChickenChap.Api.Domain;
using ChickenChap.Api.Dtos;
using ChickenChap.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ChickenChap.Api.Controllers;

[ApiController]
[Route("api/expenses")]
public class ExpensesController(ICosmosRepository<Expense> repo) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(CreateExpenseDto dto)
    {
        var e = new Expense
        {
            pk = dto.FarmId,
            FarmId = dto.FarmId,
            Date = DateOnly.Parse(dto.Date),
            Category = dto.Category,
            Amount = dto.Amount,
            Vendor = dto.Vendor,
            PaymentMode = dto.PaymentMode,
            ReceiptLink = dto.ReceiptLink,
            Notes = dto.Notes
        };
        var created = await repo.CreateAsync(e, e.pk);
        return CreatedAtAction(nameof(GetById), new { id = created.id, farmId = created.pk }, ExpenseDto.From(created));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseDto>> GetById(string id, [FromQuery] string farmId)
    {
        var found = await repo.GetAsync(id, farmId);
        return found is null ? NotFound() : Ok(ExpenseDto.From(found));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> Query([FromQuery] string farmId, [FromQuery] string? from = null, [FromQuery] string? to = null, [FromQuery] string? category = null)
    {
        var where = "c.pk = @farmId AND c.docType = 'expense'";
        if (!string.IsNullOrWhiteSpace(from)) where += " AND c.Date >= @from";
        if (!string.IsNullOrWhiteSpace(to)) where += " AND c.Date <= @to";
        if (!string.IsNullOrWhiteSpace(category)) where += " AND c.Category = @cat";

        var sql = $"SELECT * FROM c WHERE {where} ORDER BY c.Date DESC";
        var list = new List<ExpenseDto>();
        await foreach (var e in repo.QueryAsync(sql, ("@farmId", farmId), ("@from", from ?? ""), ("@to", to ?? ""), ("@cat", category ?? "")))
            list.Add(ExpenseDto.From(e));
        return Ok(list);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExpenseDto>> Update(string id, [FromQuery] string farmId, CreateExpenseDto dto)
    {
        var existing = await repo.GetAsync(id, farmId);
        if (existing is null) return NotFound();

        existing.UpdatedUtc = DateTime.UtcNow;
        existing.Date = DateOnly.Parse(dto.Date);
        existing.Category = dto.Category;
        existing.Amount = dto.Amount;
        existing.Vendor = dto.Vendor;
        existing.PaymentMode = dto.PaymentMode;
        existing.ReceiptLink = dto.ReceiptLink;
        existing.Notes = dto.Notes;

        var saved = await repo.UpsertAsync(existing, farmId);
        return Ok(ExpenseDto.From(saved));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, [FromQuery] string farmId)
    {
        await repo.DeleteAsync(id, farmId);
        return NoContent();
    }
}
