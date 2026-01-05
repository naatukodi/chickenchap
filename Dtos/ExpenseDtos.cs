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
    string? Notes,
    List<IFormFile>? Images = null
);

public record UpdateExpenseDto(
    string Date,
    string Category,
    decimal Amount,
    string? Vendor,
    string? PaymentMode,
    string? ReceiptLink,
    string? Notes,
    List<string>? ImagesToKeep = null,    // ✅ OLD images to keep (by full URL)
    List<IFormFile>? ImagesToAdd = null   // ✅ NEW images to upload
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
    List<string> ImageUrls,
    DateTime CreatedUtc,
    DateTime? UpdatedUtc
)
{
    public static ExpenseDto From(Expense e) => new(
        e.id, 
        e.FarmId, 
        e.Date.ToString("yyyy-MM-dd"), 
        e.Category, 
        e.Amount, 
        e.Vendor,
        e.PaymentMode, 
        e.ReceiptLink, 
        e.Notes, 
        e.ImageUrls ?? new(), 
        e.CreatedUtc, 
        e.UpdatedUtc
    );
}
