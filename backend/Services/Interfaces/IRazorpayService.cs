using MerxoSell.API.DTOs.Payments;

namespace MerxoSell.API.Services.Interfaces;

public interface IRazorpayService
{
    Task<CreateRazorpayOrderResponseDto> CreateOrderAsync(CreateRazorpayOrderRequestDto dto);
    Task<bool> VerifyPaymentAsync(VerifyRazorpayPaymentDto dto);
    RazorpayConfigDto GetConfig();
}
