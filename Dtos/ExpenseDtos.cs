// Dtos/ExpenseDtos.cs
using ChickenChap.Api.Domain;

namespace ChickenChap.Api.Dtos;

public record CreateExpenseDto(
    string FarmId,
    string Date,
    string Category,
    decimal Amount,
    string? Vendor,
    string? PaymentMode,
    string? ReceiptLink,
    string? Notes
);

public record ExpenseDto(
    string Id,
    string FarmId,
    string Date,
    string Category,
    decimal Amount,
    string? Vendor,
    string? PaymentMode,
    string? ReceiptLink,
    string? Notes,
    DateTime CreatedUtc,
    DateTime? UpdatedUtc
)
{
    public static ExpenseDto From(Expense e) => new(
        e.id, e.FarmId, e.Date.ToString("yyyy-MM-dd"), e.Category, e.Amount, e.Vendor,
        e.PaymentMode, e.ReceiptLink, e.Notes, e.CreatedUtc, e.UpdatedUtc
    );
}
