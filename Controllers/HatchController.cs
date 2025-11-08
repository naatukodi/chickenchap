// Controllers/HatchController.cs
using ChickenChap.Api.Domain;
using ChickenChap.Api.Dtos;
using ChickenChap.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ChickenChap.Api.Controllers;

[ApiController]
[Route("api/hatch")]
public class HatchController(ICosmosRepository<HatchBatch> repo) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<HatchDto>> Create(CreateHatchDto dto)
    {
        var e = new HatchBatch
        {
            pk = dto.FarmId,
            FarmId = dto.FarmId,
            BatchId = dto.BatchId,
            SetDate = DateOnly.Parse(dto.SetDate),
            Breed = dto.Breed,
            IncubatorType = dto.IncubatorType,
            EggsSet = dto.EggsSet,
            Infertile = dto.Infertile,
            EarlyMortality = dto.EarlyMortality,
            LateMortality = dto.LateMortality,
            Hatched = dto.Hatched,
            Notes = dto.Notes
        };
        var created = await repo.CreateAsync(e, e.pk);
        return CreatedAtAction(nameof(GetById), new { id = created.id, farmId = created.pk }, HatchDto.From(created));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HatchDto>> GetById(string id, [FromQuery] string farmId)
    {
        var found = await repo.GetAsync(id, farmId);
        return found is null ? NotFound() : Ok(HatchDto.From(found));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HatchDto>>> Query([FromQuery] string farmId, [FromQuery] string? from = null, [FromQuery] string? to = null, [FromQuery] string? batchId = null)
    {
        var where = "c.pk = @farmId AND c.docType = 'hatch'";
        if (!string.IsNullOrWhiteSpace(from)) where += " AND c.SetDate >= @from";
        if (!string.IsNullOrWhiteSpace(to)) where += " AND c.SetDate <= @to";
        if (!string.IsNullOrWhiteSpace(batchId)) where += " AND c.BatchId = @bid";

        var sql = $"SELECT * FROM c WHERE {where} ORDER BY c.SetDate DESC";
        var list = new List<HatchDto>();
        await foreach (var h in repo.QueryAsync(sql, ("@farmId", farmId), ("@from", from ?? ""), ("@to", to ?? ""), ("@bid", batchId ?? "")))
            list.Add(HatchDto.From(h));
        return Ok(list);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<HatchDto>> Update(string id, [FromQuery] string farmId, CreateHatchDto dto)
    {
        var existing = await repo.GetAsync(id, farmId);
        if (existing is null) return NotFound();

        existing.UpdatedUtc = DateTime.UtcNow;
        existing.BatchId = dto.BatchId;
        existing.SetDate = DateOnly.Parse(dto.SetDate);
        existing.Breed = dto.Breed;
        existing.IncubatorType = dto.IncubatorType;
        existing.EggsSet = dto.EggsSet;
        existing.Infertile = dto.Infertile;
        existing.EarlyMortality = dto.EarlyMortality;
        existing.LateMortality = dto.LateMortality;
        existing.Hatched = dto.Hatched;
        existing.Notes = dto.Notes;

        var saved = await repo.UpsertAsync(existing, farmId);
        return Ok(HatchDto.From(saved));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, [FromQuery] string farmId)
    {
        await repo.DeleteAsync(id, farmId);
        return NoContent();
    }
}
