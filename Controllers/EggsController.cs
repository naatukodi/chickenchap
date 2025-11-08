// Controllers/EggsController.cs
using ChickenChap.Api.Domain;
using ChickenChap.Api.Dtos;
using ChickenChap.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ChickenChap.Api.Controllers;

[ApiController]
[Route("api/eggs")]
public class EggsController(ICosmosRepository<EggCollection> repo) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<EggDto>> Create([FromBody] CreateEggDto dto)
    {
        var date = DateOnly.Parse(dto.Date);
        var entity = new EggCollection
        {
            pk = dto.FarmId,  // partition key
            FarmId = dto.FarmId,
            ShedId = dto.ShedId,
            Date = date,
            EggsCollected = dto.EggsCollected,
            BrokenEggs = dto.BrokenEggs,
            Notes = dto.Notes,
            LocationName = dto.LocationName,
            Lat = dto.Lat,
            Lng = dto.Lng
        };
        var created = await repo.CreateAsync(entity, entity.pk);
        return CreatedAtAction(nameof(GetById), new { id = created.id, farmId = created.pk }, EggDto.From(created));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EggDto>> GetById(string id, [FromQuery] string farmId)
    {
        var found = await repo.GetAsync(id, farmId);
        return found is null ? NotFound() : Ok(EggDto.From(found));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EggDto>>> Query([FromQuery] string farmId, [FromQuery] string? from = null, [FromQuery] string? to = null)
    {
        // NOTE: single-container filter includes docType
        var where = "c.pk = @farmId AND c.docType = 'egg'";
        if (!string.IsNullOrWhiteSpace(from)) where += " AND c.Date >= @from";
        if (!string.IsNullOrWhiteSpace(to)) where += " AND c.Date <= @to";

        var sql = $"SELECT * FROM c WHERE {where} ORDER BY c.Date DESC";
        var list = new List<EggDto>();
        await foreach (var e in repo.QueryAsync(sql, ("@farmId", farmId), ("@from", from ?? ""), ("@to", to ?? "")))
            list.Add(EggDto.From(e));

        return Ok(list);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EggDto>> Update(string id, [FromQuery] string farmId, [FromBody] CreateEggDto dto)
    {
        var existing = await repo.GetAsync(id, farmId);
        if (existing is null) return NotFound();

        existing.UpdatedUtc = DateTime.UtcNow;
        existing.ShedId = dto.ShedId;
        existing.Date = DateOnly.Parse(dto.Date);
        existing.EggsCollected = dto.EggsCollected;
        existing.BrokenEggs = dto.BrokenEggs;
        existing.Notes = dto.Notes;
        existing.LocationName = dto.LocationName;
        existing.Lat = dto.Lat; existing.Lng = dto.Lng;

        var saved = await repo.UpsertAsync(existing, farmId);
        return Ok(EggDto.From(saved));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, [FromQuery] string farmId)
    {
        await repo.DeleteAsync(id, farmId);
        return NoContent();
    }
}
