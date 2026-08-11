using MerxoSell.API.DTOs.Profile;

namespace MerxoSell.API.Services.Interfaces;

public interface IProfileService
{
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
    
    Task<AddressDto> AddAddressAsync(int userId, CreateAddressDto dto);
    Task<AddressDto> UpdateAddressAsync(int userId, int addressId, UpdateAddressDto dto);
    Task DeleteAddressAsync(int userId, int addressId);
    Task<AddressDto> SetDefaultAddressAsync(int userId, int addressId);
    Task<IEnumerable<AddressDto>> GetAddressesAsync(int userId);

    Task<PaymentMethodDto> AddPaymentMethodAsync(int userId, CreatePaymentMethodDto dto);
    Task DeletePaymentMethodAsync(int userId, int id);
    Task<PaymentMethodDto> SetDefaultPaymentMethodAsync(int userId, int id);

    Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<MerxoSell.API.DTOs.Seller.SellerDto?> GetSellerStoreAsync(int userId);
    Task UpdateSellerStoreAsync(int userId, MerxoSell.API.DTOs.Seller.UpdateSellerDto dto);
}
