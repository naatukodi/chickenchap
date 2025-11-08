// Controllers/SalesController.cs
using ChickenChap.Api.Domain;
using ChickenChap.Api.Dtos;
using ChickenChap.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace ChickenChap.Api.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController(ICosmosRepository<Sale> repo) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SaleDto>> Create(CreateSaleDto dto)
    {
        var entity = new Sale
        {
            pk = dto.FarmId,
            FarmId = dto.FarmId,
            Category = dto.Category,
            ItemOrBreed = dto.ItemOrBreed,
            Buyer = dto.Buyer,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            Date = DateOnly.Parse(dto.Date),
            PaymentMode = dto.PaymentMode,
            ChickAgeDays = dto.ChickAgeDays,
            Notes = dto.Notes
        };
        var created = await repo.CreateAsync(entity, entity.pk);
        return CreatedAtAction(nameof(GetById), new { id = created.id, farmId = created.pk }, SaleDto.From(created));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SaleDto>> GetById(string id, [FromQuery] string farmId)
    {
        var found = await repo.GetAsync(id, farmId);
        return found is null ? NotFound() : Ok(SaleDto.From(found));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaleDto>>> Query([FromQuery] string farmId, [FromQuery] string? from = null, [FromQuery] string? to = null)
    {
        var where = "c.pk = @farmId AND c.docType = 'sale'";
        if (!string.IsNullOrWhiteSpace(from)) where += " AND c.Date >= @from";
        if (!string.IsNullOrWhiteSpace(to)) where += " AND c.Date <= @to";

        var sql = $"SELECT * FROM c WHERE {where} ORDER BY c.Date DESC";
        var list = new List<SaleDto>();
        await foreach (var s in repo.QueryAsync(sql, ("@farmId", farmId), ("@from", from ?? ""), ("@to", to ?? "")))
            list.Add(SaleDto.From(s));
        return Ok(list);
    }
}
