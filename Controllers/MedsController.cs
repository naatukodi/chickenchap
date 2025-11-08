// Controllers/MedsController.cs
using ChickenChap.Api.Domain;
using ChickenChap.Api.Dtos;
using ChickenChap.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ChickenChap.Api.Controllers;

[ApiController]
[Route("api/meds")]
public class MedsController(ICosmosRepository<MedRecord> repo) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MedDto>> Create(CreateMedDto dto)
    {
        var e = new MedRecord
        {
            pk = dto.FarmId,
            FarmId = dto.FarmId,
            ShedId = dto.ShedId,
            Date = DateOnly.Parse(dto.Date),
            Name = dto.Name,
            Type = string.IsNullOrWhiteSpace(dto.Type) ? "Medicine" : dto.Type,
            DosePerBird = dto.DosePerBird,
            BirdsTreated = dto.BirdsTreated,
            TotalCost = dto.TotalCost,
            Notes = dto.Notes
        };
        var created = await repo.CreateAsync(e, e.pk);
        return CreatedAtAction(nameof(GetById), new { id = created.id, farmId = created.pk }, MedDto.From(created));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MedDto>> GetById(string id, [FromQuery] string farmId)
    {
        var found = await repo.GetAsync(id, farmId);
        return found is null ? NotFound() : Ok(MedDto.From(found));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MedDto>>> Query([FromQuery] string farmId, [FromQuery] string? from = null, [FromQuery] string? to = null)
    {
        var where = "c.pk = @farmId AND c.docType = 'med'";
        if (!string.IsNullOrWhiteSpace(from)) where += " AND c.Date >= @from";
        if (!string.IsNullOrWhiteSpace(to)) where += " AND c.Date <= @to";

        var sql = $"SELECT * FROM c WHERE {where} ORDER BY c.Date DESC";
        var list = new List<MedDto>();
        await foreach (var m in repo.QueryAsync(sql, ("@farmId", farmId), ("@from", from ?? ""), ("@to", to ?? "")))
            list.Add(MedDto.From(m));
        return Ok(list);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MedDto>> Update(string id, [FromQuery] string farmId, CreateMedDto dto)
    {
        var existing = await repo.GetAsync(id, farmId);
        if (existing is null) return NotFound();

        existing.UpdatedUtc = DateTime.UtcNow;
        existing.ShedId = dto.ShedId;
        existing.Date = DateOnly.Parse(dto.Date);
        existing.Name = dto.Name;
        existing.Type = dto.Type;
        existing.DosePerBird = dto.DosePerBird;
        existing.BirdsTreated = dto.BirdsTreated;
        existing.TotalCost = dto.TotalCost;
        existing.Notes = dto.Notes;

        var saved = await repo.UpsertAsync(existing, farmId);
        return Ok(MedDto.From(saved));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, [FromQuery] string farmId)
    {
        await repo.DeleteAsync(id, farmId);
        return NoContent();
    }
}
