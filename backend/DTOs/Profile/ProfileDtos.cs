using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Profile;

public record UserProfileDto(
    int      UserId,
    string   FullName,
    string   Email,
    string?  Phone,
    string   PreferredCurrency,
    string   Country,
    DateTime CreatedAt,
    IEnumerable<AddressDto> Addresses,
    IEnumerable<PaymentMethodDto> PaymentMethods
);

public record PaymentMethodDto(
    int      Id,
    string   Type,
    string   Label,
    string?  SubLabel,
    bool     IsDefault,
    DateTime CreatedAt
);

public record CreatePaymentMethodDto(
    [Required] string Type,  // 'Card' or 'UPI'
    [Required, MaxLength(100)] string Label,
    [MaxLength(100)] string? SubLabel,
    bool IsDefault = false
);

public record UpdateProfileDto(
    [Required, MaxLength(150)] string FullName,
    [MaxLength(50)] string? Phone,
    [Required, MaxLength(10)] string PreferredCurrency,
    [Required, MaxLength(100)] string Country
);

public record AddressDto(
    int      AddressId,
    string   FullName,
    string   AddressLine1,
    string?  AddressLine2,
    string   City,
    string   State,
    string   PostalCode,
    string   Country,
    string?  Phone,
    string?  Label,
    bool     IsDefault
);

public record CreateAddressDto(
    [Required, MaxLength(150)] string FullName,
    [Required, MaxLength(300)] string AddressLine1,
    [MaxLength(300)]           string? AddressLine2,
    [Required, MaxLength(100)] string City,
    [Required, MaxLength(100)] string State,
    [Required, MaxLength(20)]  string PostalCode,
    [Required, MaxLength(100)] string Country,
    [MaxLength(50)]            string? Phone,
    [MaxLength(50)]            string? Label,
    bool IsDefault = false
);

public record UpdateAddressDto(
    [Required, MaxLength(150)] string FullName,
    [Required, MaxLength(300)] string AddressLine1,
    [MaxLength(300)]           string? AddressLine2,
    [Required, MaxLength(100)] string City,
    [Required, MaxLength(100)] string State,
    [Required, MaxLength(20)]  string PostalCode,
    [Required, MaxLength(100)] string Country,
    [MaxLength(50)]            string? Phone,
    [MaxLength(50)]            string? Label,
    bool IsDefault = false
);

public record ChangePasswordDto(
    [Required] string CurrentPassword,
    [Required, MinLength(6)] string NewPassword
);
