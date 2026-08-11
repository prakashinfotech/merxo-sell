using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly AppDbContext _db;
    public AddressRepository(AppDbContext db) => _db = db;

    public async Task<Address> CreateAddressAsync(Address address)
    {
        _db.Addresses.Add(address);
        await _db.SaveChangesAsync();
        return address;
    }

    public async Task<Address?> GetByIdAsync(int addressId, int userId)
    {
        return await _db.Addresses.FirstOrDefaultAsync(a => a.AddressId == addressId && a.UserId == userId);
    }

    public async Task<IEnumerable<Address>> GetByUserIdAsync(int userId)
    {
        return await _db.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ToListAsync();
    }

    public async Task<Address> UpdateAddressAsync(Address address)
    {
        _db.Addresses.Update(address);
        await _db.SaveChangesAsync();
        return address;
    }

    public async Task DeleteAddressAsync(Address address)
    {
        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync();
    }

    public async Task SetAllNonDefaultAsync(int userId)
    {
        var defaults = await _db.Addresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
        foreach (var addr in defaults)
        {
            addr.IsDefault = false;
        }
        await _db.SaveChangesAsync();
    }
}
