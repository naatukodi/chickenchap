// Dtos/SaleDtos.cs
using ChickenChap.Api.Domain;

namespace ChickenChap.Api.Dtos;

public record CreateSaleDto(
    string FarmId,
    string Date,
    SaleCategory Category,
    string? ItemOrBreed,
    string? Buyer,
    int Quantity,
    decimal UnitPrice,
    string? PaymentMode,
    int? ChickAgeDays,
    string? Notes
);

public record SaleDto(
    string Id,
    string FarmId,
    string Date,
    SaleCategory Category,
    string? ItemOrBreed,
    string? Buyer,
    int Quantity,
    decimal UnitPrice,
    decimal Revenue,
    string? PaymentMode,
    int? ChickAgeDays,
    string? Notes,
    DateTime CreatedUtc,
    DateTime? UpdatedUtc
)
{
    public static SaleDto From(Sale s) => new(
        s.id, s.FarmId, s.Date.ToString("yyyy-MM-dd"), s.Category, s.ItemOrBreed, s.Buyer,
        s.Quantity, s.UnitPrice, s.UnitPrice * s.Quantity, s.PaymentMode, s.ChickAgeDays,
        s.Notes, s.CreatedUtc, s.UpdatedUtc
    );
}
