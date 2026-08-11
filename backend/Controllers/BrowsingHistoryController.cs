using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MerxoSell.API.DTOs.BrowsingHistory;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

[Route("api/browsing-history")]
public class BrowsingHistoryController : BaseApiController
{
    private readonly IBrowsingHistoryService _historyService;

    public BrowsingHistoryController(IBrowsingHistoryService historyService)
        => _historyService = historyService;

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Add([FromBody] AddBrowsingHistoryDto dto)
    {
        try
        {
            await _historyService.RecordViewAsync(GetUserIdNullable(), dto.ProductId);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    }

    [HttpGet]
    [Authorize(Roles = "Buyer")]
    public async Task<ActionResult<IReadOnlyList<BrowsingHistoryDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
        => Ok(await _historyService.GetForUserAsync(GetUserId(), page, pageSize));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> Delete(int id)
        => await _historyService.DeleteAsync(GetUserId(), id) ? NoContent() : NotFound();

    [HttpDelete]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> Clear()
    {
        await _historyService.ClearAsync(GetUserId());
        return NoContent();
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User identity not found."));

    private int? GetUserIdNullable()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(claim) ? null : int.Parse(claim);
    }
}
