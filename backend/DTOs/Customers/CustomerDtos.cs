using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Customers;

/// <summary>Lightweight admin view of a Buyer/customer account.</summary>
public record CustomerDto(
    int      UserId,
    string   FullName,
    string   Email,
    string?  Phone,
    string   PreferredCurrency,
    bool     IsActive,
    int      OrderCount,
    decimal  TotalSpentCAD,
    int      AddressCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>Detailed customer profile + order summary for the admin view modal.</summary>
public record CustomerDetailDto(
    int      UserId,
    string   FullName,
    string   Email,
    string?  Phone,
    string   PreferredCurrency,
    bool     IsActive,
    int      OrderCount,
    decimal  TotalSpentCAD,
    int      AddressCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string?  LastOrderStatus,
    DateTime? LastOrderAt,
    IEnumerable<CustomerAddressDto> Addresses
);

public record CustomerAddressDto(
    int     AddressId,
    string  FullName,
    string  AddressLine1,
    string? AddressLine2,
    string  City,
    string? State,
    string  PostalCode,
    string  Country,
    string  Phone,
    bool    IsDefault
);

/// <summary>Admin-editable customer fields. Email and Role are intentionally read-only.</summary>
public record UpdateCustomerDto(
    [Required, MaxLength(150)] string  FullName,
    [Phone, MaxLength(20)]     string? Phone,
    [Required, MaxLength(10)]  string  PreferredCurrency,
    [Required]                 bool    IsActive
);

public record SetCustomerStatusDto([Required] bool IsActive);
