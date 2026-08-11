using Microsoft.AspNetCore.Mvc;
using MerxoSell.API.DTOs.Common;
using MerxoSell.API.DTOs.Products;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Controllers;

/// <summary>Public product catalogue — no authentication required.</summary>
[Route("api/products")]
public class ProductsController : BaseApiController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// GET /api/products — filtered, sorted, paginated product list.
    /// All prices are in CAD. The frontend applies currency conversion via AppCurrencyPipe.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListDto>>> GetPagedAsync(
        [FromQuery] ProductFilterDto filter)
    {
        var result = await _productService.GetPagedAsync(filter);
        return Ok(result);
    }

    /// <summary>GET /api/products/{id} — full product detail including images, variants, and rating summary.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> GetDetailAsync(int id)
    {
        var product = await _productService.GetDetailAsync(id);
        return product is null ? NotFound(new { error = $"Product {id} not found." }) : Ok(product);
    }

    /// <summary>
    /// GET /api/products/search — global search with fuzzy match on Name + Description,
    /// price range, category, seller, in-stock toggle, sort, and paging.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<ProductListDto>>> SearchAsync(
        [FromQuery] ProductSearchFilterDto filter)
    {
        var result = await _productService.SearchAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/products/suggest?q= — top 8 autosuggest entries (5 products + 3 categories).
    /// Returns an empty list for queries shorter than 2 characters.
    /// </summary>
    [HttpGet("suggest")]
    public async Task<ActionResult<IReadOnlyList<SuggestItemDto>>> SuggestAsync([FromQuery] string q)
    {
        var items = await _productService.SuggestAsync(q ?? string.Empty);
        return Ok(items);
    }
}
