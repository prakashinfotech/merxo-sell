using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MerxoSell.API.DTOs.Manufacturer;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

/// <summary>SuperAdmin/Seller manufacturer / supplier management.</summary>
[Route("api/manufacturers")]
[Authorize(Roles = "SuperAdmin,Seller")]
public class ManufacturersController : BaseApiController
{
    private readonly IManufacturerService _service;

    public ManufacturersController(IManufacturerService service) => _service = service;

    /// <summary>GET /api/manufacturers — list all manufacturers.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    /// <summary>GET /api/manufacturers/{id}</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null
            ? NotFound(new { error = $"Manufacturer {id} not found." })
            : Ok(item);
    }

    /// <summary>POST /api/manufacturers — create a new manufacturer.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateManufacturerDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.ManufacturerId }, created);
    }

    /// <summary>PUT /api/manufacturers/{id} — update an existing manufacturer.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateManufacturerDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated is null
            ? NotFound(new { error = $"Manufacturer {id} not found." })
            : Ok(updated);
    }

    /// <summary>DELETE /api/manufacturers/{id}</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted
            ? NoContent()
            : NotFound(new { error = $"Manufacturer {id} not found." });
    }
}
