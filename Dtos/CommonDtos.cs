// Dtos/CommonDtos.cs
namespace ChickenChap.Api.Dtos;

public record DateRange(string? From = null, string? To = null); // ISO yyyy-MM-dd
public record PagedResult<T>(IEnumerable<T> Items, int TotalCount);