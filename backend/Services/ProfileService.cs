using MerxoSell.API.DTOs.Profile;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Interfaces;
using MerxoSell.API.Data;
using Microsoft.EntityFrameworkCore;

namespace MerxoSell.API.Services;

public class ProfileService : IProfileService
{
    private readonly IUserRepository _userRepo;
    private readonly IAddressRepository _addressRepo;
    private readonly ISellerRepository _sellerRepo;
    private readonly AppDbContext _db;

    public ProfileService(IUserRepository userRepo, IAddressRepository addressRepo, ISellerRepository sellerRepo, AppDbContext db)
    {
        _userRepo = userRepo;
        _addressRepo = addressRepo;
        _sellerRepo = sellerRepo;
        _db = db;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        var addresses = await _addressRepo.GetByUserIdAsync(userId);
        var payments = await _db.UserPaymentMethods
                                .Where(pm => pm.UserId == userId && !pm.IsDeleted)
                                .ToListAsync();

        return new UserProfileDto(
            user.UserId,
            user.FullName,
            user.Email,
            user.Phone,
            user.PreferredCurrency,
            user.Country ?? "Canada",
            user.CreatedAt,
            addresses.Select(MapAddressDto),
            payments.Select(MapPaymentDto)
        );
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        user.FullName = dto.FullName;
        user.Phone = dto.Phone;
        user.PreferredCurrency = dto.PreferredCurrency;
        user.Country = dto.Country;
        
        await _userRepo.UpdateAsync(user);

        return await GetProfileAsync(userId);
    }

    public async Task<AddressDto> AddAddressAsync(int userId, CreateAddressDto dto)
    {
        ValidateZip(dto.Country, dto.PostalCode);

        bool isDefault = dto.IsDefault;
        if (isDefault)
        {
            await _addressRepo.SetAllNonDefaultAsync(userId);
        }
        else
        {
            var existing = await _addressRepo.GetByUserIdAsync(userId);
            if (!existing.Any()) isDefault = true;
        }

        var address = new Address
        {
            UserId = userId,
            FullName = dto.FullName,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            Phone = dto.Phone ?? string.Empty,
            Label = dto.Label,
            IsDefault = isDefault
        };

        var created = await _addressRepo.CreateAddressAsync(address);
        return MapAddressDto(created);
    }

    public async Task<AddressDto> UpdateAddressAsync(int userId, int addressId, UpdateAddressDto dto)
    {
        ValidateZip(dto.Country, dto.PostalCode);

        var address = await _addressRepo.GetByIdAsync(addressId, userId);
        if (address == null) throw new KeyNotFoundException("Address not found.");

        if (dto.IsDefault && !address.IsDefault)
        {
            await _addressRepo.SetAllNonDefaultAsync(userId);
        }

        address.FullName = dto.FullName;
        address.AddressLine1 = dto.AddressLine1;
        address.AddressLine2 = dto.AddressLine2;
        address.City = dto.City;
        address.State = dto.State;
        address.PostalCode = dto.PostalCode;
        address.Country = dto.Country;
        address.Phone = dto.Phone ?? string.Empty;
        address.Label = dto.Label;
        address.IsDefault = dto.IsDefault;

        var updated = await _addressRepo.UpdateAddressAsync(address);
        return MapAddressDto(updated);
    }

    public async Task<IEnumerable<AddressDto>> GetAddressesAsync(int userId)
    {
        var addresses = await _addressRepo.GetByUserIdAsync(userId);
        return addresses.Select(MapAddressDto);
    }

    private static readonly Dictionary<string, System.Text.RegularExpressions.Regex> _zipPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["US"]      = new(@"^\d{5}(-\d{4})?$"),
        ["CA"]      = new(@"^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$"),
        ["CANADA"]  = new(@"^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$"),
        ["UK"]      = new(@"^[A-Z]{1,2}\d[A-Z\d]? ?\d[A-Z]{2}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase),
        ["GB"]      = new(@"^[A-Z]{1,2}\d[A-Z\d]? ?\d[A-Z]{2}$", System.Text.RegularExpressions.RegexOptions.IgnoreCase),
        ["IN"]      = new(@"^\d{6}$"),
        ["INDIA"]   = new(@"^\d{6}$"),
        ["AU"]      = new(@"^\d{4}$"),
        ["DE"]      = new(@"^\d{5}$"),
        ["FR"]      = new(@"^\d{5}$"),
    };

    private static void ValidateZip(string country, string zip)
    {
        if (string.IsNullOrWhiteSpace(country) || string.IsNullOrWhiteSpace(zip)) return;
        var key = country.Trim();
        if (_zipPatterns.TryGetValue(key, out var pattern) && !pattern.IsMatch(zip.Trim()))
        {
            throw new ArgumentException($"Invalid postal code format for {country}.");
        }
    }

    public async Task DeleteAddressAsync(int userId, int addressId)
    {
        var address = await _addressRepo.GetByIdAsync(addressId, userId);
        if (address == null) throw new KeyNotFoundException("Address not found.");

        await _addressRepo.DeleteAddressAsync(address);

        if (address.IsDefault)
        {
            var remaining = await _addressRepo.GetByUserIdAsync(userId);
            var newDefault = remaining.FirstOrDefault();
            if (newDefault != null)
            {
                newDefault.IsDefault = true;
                await _addressRepo.UpdateAddressAsync(newDefault);
            }
        }
    }

    public async Task<AddressDto> SetDefaultAddressAsync(int userId, int addressId)
    {
        var address = await _addressRepo.GetByIdAsync(addressId, userId);
        if (address == null) throw new KeyNotFoundException("Address not found.");

        if (!address.IsDefault)
        {
            await _addressRepo.SetAllNonDefaultAsync(userId);
            address.IsDefault = true;
            var updated = await _addressRepo.UpdateAddressAsync(address);
            return MapAddressDto(updated);
        }

        return MapAddressDto(address);
    }

    public async Task<PaymentMethodDto> AddPaymentMethodAsync(int userId, CreatePaymentMethodDto dto)
    {
        if (dto.IsDefault)
        {
            var others = await _db.UserPaymentMethods.Where(pm => pm.UserId == userId).ToListAsync();
            foreach (var o in others) o.IsDefault = false;
        }
        else
        {
            var any = await _db.UserPaymentMethods.AnyAsync(pm => pm.UserId == userId && !pm.IsDeleted);
            if (!any) dto = dto with { IsDefault = true };
        }

        var pm = new UserPaymentMethod
        {
            UserId = userId,
            Type = dto.Type,
            Label = dto.Label,
            SubLabel = dto.SubLabel,
            IsDefault = dto.IsDefault,
            IsDeleted = false
        };

        _db.UserPaymentMethods.Add(pm);
        await _db.SaveChangesAsync();
        return MapPaymentDto(pm);
    }

    public async Task DeletePaymentMethodAsync(int userId, int id)
    {
        var pm = await _db.UserPaymentMethods.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (pm == null) throw new KeyNotFoundException("Payment method not found.");

        pm.IsDeleted = true;
        await _db.SaveChangesAsync();

        if (pm.IsDefault)
        {
            var next = await _db.UserPaymentMethods
                                .Where(x => x.UserId == userId && !x.IsDeleted)
                                .FirstOrDefaultAsync();
            if (next != null)
            {
                next.IsDefault = true;
                await _db.SaveChangesAsync();
            }
        }
    }

    public async Task<PaymentMethodDto> SetDefaultPaymentMethodAsync(int userId, int id)
    {
        var pm = await _db.UserPaymentMethods.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (pm == null) throw new KeyNotFoundException("Payment method not found.");

        var others = await _db.UserPaymentMethods.Where(x => x.UserId == userId).ToListAsync();
        foreach (var o in others) o.IsDefault = false;

        pm.IsDefault = true;
        await _db.SaveChangesAsync();
        return MapPaymentDto(pm);
    }

    public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Current password is incorrect.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, 11);
        await _userRepo.UpdateAsync(user);
    }

    public async Task<MerxoSell.API.DTOs.Seller.SellerDto?> GetSellerStoreAsync(int userId)
    {
        var seller = await _sellerRepo.GetByUserIdAsync(userId);
        if (seller == null) return null;

        var productCount = await _db.Products.CountAsync(p => p.SellerId == seller.SellerId && !p.IsDeleted);

        return new MerxoSell.API.DTOs.Seller.SellerDto(
            seller.SellerId, seller.UserId, seller.StoreName, seller.StoreDescription,
            seller.ContactEmail, seller.Phone, seller.IsVerified, seller.IsActive,
            seller.User.Email, seller.User.FullName, productCount, seller.CreatedAt
        );
    }

    public async Task UpdateSellerStoreAsync(int userId, MerxoSell.API.DTOs.Seller.UpdateSellerDto dto)
    {
        var seller = await _sellerRepo.GetByUserIdAsync(userId);
        if (seller == null) throw new KeyNotFoundException("Seller profile not found.");

        seller.StoreName = dto.StoreName;
        seller.StoreDescription = dto.StoreDescription;
        seller.ContactEmail = dto.ContactEmail;
        seller.Phone = dto.Phone;
        seller.UpdatedAt = DateTime.UtcNow;

        await _sellerRepo.UpdateAsync(seller);
    }

    private AddressDto MapAddressDto(Address a) => new(
        a.AddressId,
        a.FullName,
        a.AddressLine1,
        a.AddressLine2,
        a.City,
        a.State ?? string.Empty,
        a.PostalCode,
        a.Country,
        a.Phone,
        a.Label,
        a.IsDefault
    );

    private PaymentMethodDto MapPaymentDto(UserPaymentMethod pm) => new(
        pm.Id, pm.Type, pm.Label, pm.SubLabel, pm.IsDefault, pm.CreatedAt
    );
}
