using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface IManufacturerRepository
{
    Task<IReadOnlyList<Manufacturer>> GetAllAsync();
    Task<Manufacturer?> GetByIdAsync(int id);
    Task<Manufacturer> CreateAsync(Manufacturer manufacturer);
    Task<Manufacturer?> UpdateAsync(Manufacturer manufacturer);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
