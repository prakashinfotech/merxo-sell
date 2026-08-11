using System.ComponentModel.DataAnnotations;

namespace MerxoSell.API.DTOs.Manufacturer;

public class CreateManufacturerDto
{
    [Required, MaxLength(200)]
    public string  Name         { get; set; } = string.Empty;

    [MaxLength(200), EmailAddress]
    public string? ContactEmail { get; set; }

    [MaxLength(50)]
    public string? Phone        { get; set; }

    [MaxLength(500)]
    public string? Address      { get; set; }

    [MaxLength(100)]
    public string? Country      { get; set; }

    [MaxLength(300)]
    public string? Website      { get; set; }
}
