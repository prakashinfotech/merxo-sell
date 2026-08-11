using MerxoSell.API.Models;

namespace MerxoSell.API.Repositories.Interfaces;

public interface IAddressRepository
{
    Task<Address> CreateAddressAsync(Address address);
    Task<Address?> GetByIdAsync(int addressId, int userId);
    Task<IEnumerable<Address>> GetByUserIdAsync(int userId);
    Task<Address> UpdateAddressAsync(Address address);
    Task DeleteAddressAsync(Address address);
    Task SetAllNonDefaultAsync(int userId);
}
