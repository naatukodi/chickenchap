// Controllers/ReportsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChickenChap.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(Container container) : ControllerBase
{
    public record DayTotal(string Date, decimal Total);
    public record BuyerTotal(string Buyer, decimal Revenue);

    static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [HttpGet("daily-eggs")]
    public async Task<ActionResult<IEnumerable<DayTotal>>> DailyEggs([FromQuery] string farmId, [FromQuery] string? from = null, [FromQuery] string? to = null)
    {
        var where = "c.pk = @farmId AND c.docType = 'egg'";
        if (!string.IsNullOrWhiteSpace(from)) where += " AND c.Date >= @from";
        if (!string.IsNullOrWhiteSpace(to)) where += " AND c.Date <= @to";

        // GROUP BY supported in Cosmos SQL
        var sql = $@"
SELECT c.Date AS Date, SUM(c.EggsCollected) AS Total
FROM c
WHERE {where}
GROUP BY c.Date
ORDER BY c.Date ASC";

        var qd = new QueryDefinition(sql).WithParameter("@farmId", farmId)
                                         .WithParameter("@from", from ?? "")
                                         .WithParameter("@to", to ?? "");

        var results = new List<DayTotal>();
        using var it = container.GetItemQueryIterator<dynamic>(qd, requestOptions: new QueryRequestOptions { PartitionKey = new(farmId) });
        while (it.HasMoreResults)
        {
            foreach (var r in await it.ReadNextAsync())
            {
                results.Add(new DayTotal((string)r.Date, (decimal)r.Total));
            }
        }
        return Ok(results);
    }

    [HttpGet("revenue-daily")]
    public async Task<ActionResult<IEnumerable<DayTotal>>> RevenueDaily([FromQuery] string farmId, [FromQuery] string? from = null, [FromQuery] string? to = null)
    {
        var where = "c.pk = @farmId AND c.docType = 'sale'";
        if (!string.IsNullOrWhiteSpace(from)) where += " AND c.Date >= @from";
        if (!string.IsNullOrWhiteSpace(to)) where += " AND c.Date <= @to";

        var sql = $@"
SELECT c.Date AS Date, SUM(c.UnitPrice * c.Quantity) AS Total
FROM c
WHERE {where}
GROUP BY c.Date
ORDER BY c.Date ASC";

        var qd = new QueryDefinition(sql).WithParameter("@farmId", farmId)
                                         .WithParameter("@from", from ?? "")
                                         .WithParameter("@to", to ?? "");

        var results = new List<DayTotal>();
        using var it = container.GetItemQueryIterator<dynamic>(qd, requestOptions: new QueryRequestOptions { PartitionKey = new(farmId) });
        while (it.HasMoreResults)
        {
            foreach (var r in await it.ReadNextAsync())
                results.Add(new DayTotal((string)r.Date, (decimal)r.Total));
        }
        return Ok(results);
    }

    [HttpGet("costs-daily")]
    public async Task<ActionResult<IEnumerable<DayTotal>>> CostsDaily([FromQuery] string farmId, [FromQuery] string? from = null, [FromQuery] string? to = null)
    {
        // Feed cost (Quantity * CostPerKg) + Expense amount
        var feedWhere = "c.pk = @farmId AND c.docType = 'feed'";
        var expWhere = "c.pk = @farmId AND c.docType = 'expense'";
        if (!string.IsNullOrWhiteSpace(from)) { feedWhere += " AND c.Date >= @from"; expWhere += " AND c.Date >= @from"; }
        if (!string.IsNullOrWhiteSpace(to)) { feedWhere += " AND c.Date <= @to"; expWhere += " AND c.Date <= @to"; }

        var sql = $@"
SELECT t.Date, SUM(t.Total) AS Total FROM (
  SELECT c.Date AS Date, SUM(c.QuantityKg * c.CostPerKg) AS Total FROM c
  WHERE {feedWhere}
  GROUP BY c.Date
  UNION ALL
  SELECT c.Date AS Date, SUM(c.Amount) AS Total FROM c
  WHERE {expWhere}
  GROUP BY c.Date
) t
GROUP BY t.Date
ORDER BY t.Date ASC";

        var qd = new QueryDefinition(sql).WithParameter("@farmId", farmId)
                                         .WithParameter("@from", from ?? "")
                                         .WithParameter("@to", to ?? "");
        var list = new List<DayTotal>();
        using var it = container.GetItemQueryIterator<dynamic>(qd, requestOptions: new QueryRequestOptions { PartitionKey = new(farmId) });
        while (it.HasMoreResults)
        {
            foreach (var r in await it.ReadNextAsync())
                list.Add(new DayTotal((string)r.Date, (decimal)r.Total));
        }
        return Ok(list);
    }

    [HttpGet("top-buyers")]
    public async Task<ActionResult<IEnumerable<BuyerTotal>>> TopBuyers([FromQuery] string farmId, [FromQuery] int top = 5, [FromQuery] string? from = null, [FromQuery] string? to = null)
    {
        var where = "c.pk = @farmId AND c.docType = 'sale'";
        if (!string.IsNullOrWhiteSpace(from)) where += " AND c.Date >= @from";
        if (!string.IsNullOrWhiteSpace(to)) where += " AND c.Date <= @to";

        var sql = $@"
SELECT c.Buyer AS Buyer, SUM(c.UnitPrice * c.Quantity) AS Revenue
FROM c
WHERE {where}
GROUP BY c.Buyer
ORDER BY Revenue DESC";

        var qd = new QueryDefinition(sql).WithParameter("@farmId", farmId)
                                         .WithParameter("@from", from ?? "")
                                         .WithParameter("@to", to ?? "");

        var list = new List<BuyerTotal>();
        using var it = container.GetItemQueryIterator<dynamic>(qd, requestOptions: new QueryRequestOptions { PartitionKey = new(farmId) });
        while (it.HasMoreResults)
        {
            foreach (var r in await it.ReadNextAsync())
            {
                var buyer = (string?)r.Buyer ?? "(Unknown)";
                var rev = (decimal)r.Revenue;
                list.Add(new BuyerTotal(buyer, rev));
            }
        }
        return Ok(list.Take(Math.Max(1, top)));
    }
}
