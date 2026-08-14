using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Payments;

public record CreateRazorpayOrderRequestDto(
    [Required] decimal Amount,
    [Required] string Currency,
    string? Receipt
);

public record CreateRazorpayOrderResponseDto(
    string RazorpayOrderId,
    string KeyId,
    long AmountInSubunits,
    string Currency,
    string AccountEmail
);

public record VerifyRazorpayPaymentDto(
    [Required] string RazorpayOrderId,
    [Required] string RazorpayPaymentId,
    [Required] string RazorpaySignature
);

public record RazorpayConfigDto(
    string KeyId,
    string AccountEmail
);
