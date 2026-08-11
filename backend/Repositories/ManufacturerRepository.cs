using Microsoft.EntityFrameworkCore;
using MerxoSell.API.Data;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;

namespace MerxoSell.API.Repositories;

public class ManufacturerRepository : IManufacturerRepository
{
    private readonly AppDbContext _db;

    public ManufacturerRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Manufacturer>> GetAllAsync()
        => await _db.Manufacturers
                    .AsNoTracking()
                    .Include(m => m.Products)
                    .OrderBy(m => m.Name)
                    .ToListAsync();

    public async Task<Manufacturer?> GetByIdAsync(int id)
        => await _db.Manufacturers
                    .AsNoTracking()
                    .Include(m => m.Products)
                    .FirstOrDefaultAsync(m => m.ManufacturerId == id);

    public async Task<Manufacturer> CreateAsync(Manufacturer manufacturer)
    {
        _db.Manufacturers.Add(manufacturer);
        await _db.SaveChangesAsync();
        return manufacturer;
    }

    public async Task<Manufacturer?> UpdateAsync(Manufacturer manufacturer)
    {
        var existing = await _db.Manufacturers.FindAsync(manufacturer.ManufacturerId);
        if (existing is null) return null;

        existing.Name         = manufacturer.Name;
        existing.ContactEmail = manufacturer.ContactEmail;
        existing.Phone        = manufacturer.Phone;
        existing.Address      = manufacturer.Address;
        existing.Country      = manufacturer.Country;
        existing.Website      = manufacturer.Website;
        existing.IsActive     = manufacturer.IsActive;
        existing.UpdatedAt    = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var manufacturer = await _db.Manufacturers.FindAsync(id);
        if (manufacturer is null) return false;

        _db.Manufacturers.Remove(manufacturer);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
        => await _db.Manufacturers.AnyAsync(m => m.ManufacturerId == id);
}
