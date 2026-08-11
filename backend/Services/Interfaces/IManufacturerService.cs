using MerxoSell.API.DTOs.Manufacturer;

namespace MerxoSell.API.Services.Interfaces;

public interface IManufacturerService
{
    Task<IReadOnlyList<ManufacturerDto>> GetAllAsync();
    Task<ManufacturerDto?> GetByIdAsync(int id);
    Task<ManufacturerDto> CreateAsync(CreateManufacturerDto dto);
    Task<ManufacturerDto?> UpdateAsync(int id, UpdateManufacturerDto dto);
    Task<bool> DeleteAsync(int id);
}
