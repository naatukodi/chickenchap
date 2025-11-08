// Controllers/FeedController.cs
using ChickenChap.Api.Domain;
using ChickenChap.Api.Dtos;
using ChickenChap.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ChickenChap.Api.Controllers;

[ApiController]
[Route("api/feed")]
public class FeedController(ICosmosRepository<FeedUsage> repo) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<FeedDto>> Create(CreateFeedDto dto)
    {
        var e = new FeedUsage
        {
            pk = dto.FarmId,
            FarmId = dto.FarmId,
            ShedId = dto.ShedId,
            Date = DateOnly.Parse(dto.Date),
            FeedType = dto.FeedType,
            QuantityKg = dto.QuantityKg,
            CostPerKg = dto.CostPerKg,
            Notes = dto.Notes
        };
        var created = await repo.CreateAsync(e, e.pk);
        return CreatedAtAction(nameof(GetById), new { id = created.id, farmId = created.pk }, FeedDto.From(created));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FeedDto>> GetById(string id, [FromQuery] string farmId)
    {
        var found = await repo.GetAsync(id, farmId);
        return found is null ? NotFound() : Ok(FeedDto.From(found));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeedDto>>> Query([FromQuery] string farmId, [FromQuery] string? from = null, [FromQuery] string? to = null)
    {
        var where = "c.pk = @farmId AND c.docType = 'feed'";
        if (!string.IsNullOrWhiteSpace(from)) where += " AND c.Date >= @from";
        if (!string.IsNullOrWhiteSpace(to)) where += " AND c.Date <= @to";

        var sql = $"SELECT * FROM c WHERE {where} ORDER BY c.Date DESC";
        var list = new List<FeedDto>();
        await foreach (var f in repo.QueryAsync(sql, ("@farmId", farmId), ("@from", from ?? ""), ("@to", to ?? "")))
            list.Add(FeedDto.From(f));
        return Ok(list);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FeedDto>> Update(string id, [FromQuery] string farmId, CreateFeedDto dto)
    {
        var existing = await repo.GetAsync(id, farmId);
        if (existing is null) return NotFound();

        existing.UpdatedUtc = DateTime.UtcNow;
        existing.ShedId = dto.ShedId;
        existing.Date = DateOnly.Parse(dto.Date);
        existing.FeedType = dto.FeedType;
        existing.QuantityKg = dto.QuantityKg;
        existing.CostPerKg = dto.CostPerKg;
        existing.Notes = dto.Notes;

        var saved = await repo.UpsertAsync(existing, farmId);
        return Ok(FeedDto.From(saved));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, [FromQuery] string farmId)
    {
        await repo.DeleteAsync(id, farmId);
        return NoContent();
    }
}
