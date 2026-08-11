using MerxoSell.API.DTOs.Manufacturer;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Services;

public class ManufacturerService : IManufacturerService
{
    private readonly IManufacturerRepository _repo;

    public ManufacturerService(IManufacturerRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<ManufacturerDto>> GetAllAsync()
    {
        var manufacturers = await _repo.GetAllAsync();
        return manufacturers.Select(Map).ToList();
    }

    public async Task<ManufacturerDto?> GetByIdAsync(int id)
    {
        var m = await _repo.GetByIdAsync(id);
        return m is null ? null : Map(m);
    }

    public async Task<ManufacturerDto> CreateAsync(CreateManufacturerDto dto)
    {
        var manufacturer = new Manufacturer
        {
            Name         = dto.Name,
            ContactEmail = dto.ContactEmail,
            Phone        = dto.Phone,
            Address      = dto.Address,
            Country      = dto.Country,
            Website      = dto.Website,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        };
        var created = await _repo.CreateAsync(manufacturer);
        return Map(created);
    }

    public async Task<ManufacturerDto?> UpdateAsync(int id, UpdateManufacturerDto dto)
    {
        var updated = await _repo.UpdateAsync(new Manufacturer
        {
            ManufacturerId = id,
            Name           = dto.Name,
            ContactEmail   = dto.ContactEmail,
            Phone          = dto.Phone,
            Address        = dto.Address,
            Country        = dto.Country,
            Website        = dto.Website,
            IsActive       = dto.IsActive
        });
        return updated is null ? null : Map(updated);
    }

    public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

    private static ManufacturerDto Map(Manufacturer m) => new()
    {
        ManufacturerId = m.ManufacturerId,
        Name           = m.Name,
        ContactEmail   = m.ContactEmail,
        Phone          = m.Phone,
        Address        = m.Address,
        Country        = m.Country,
        Website        = m.Website,
        IsActive       = m.IsActive,
        CreatedAt      = m.CreatedAt,
        ProductCount   = m.Products.Count
    };
}
