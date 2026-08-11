using FluentAssertions;
using Moq;
using MerxoSell.API.DTOs.Common;
using MerxoSell.API.DTOs.Products;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services;

namespace MerxoSell.Tests.Services;

public class ProductServiceTests : TestBase
{
    private readonly Mock<IProductRepository> _repo = new();
    private ProductService BuildSut() => new(_repo.Object);

    // ── GetPaged ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPaged_PageBelowOne_NormalisesToPage1()
    {
        var filter = new ProductFilterDto { Page = 0, PageSize = 10 };

        _repo.Setup(r => r.GetPagedAsync(It.IsAny<ProductFilterDto>()))
             .ReturnsAsync((new List<ProductListDto>(), 0));

        await BuildSut().GetPagedAsync(filter);

        _repo.Verify(r => r.GetPagedAsync(It.Is<ProductFilterDto>(f => f.Page == 1)), Times.Once);
    }

    [Fact]
    public async Task GetPaged_PageSizeAbove100_ClampedTo100()
    {
        var filter = new ProductFilterDto { Page = 1, PageSize = 500 };

        _repo.Setup(r => r.GetPagedAsync(It.IsAny<ProductFilterDto>()))
             .ReturnsAsync((new List<ProductListDto>(), 0));

        await BuildSut().GetPagedAsync(filter);

        _repo.Verify(r => r.GetPagedAsync(It.Is<ProductFilterDto>(f => f.PageSize == 100)), Times.Once);
    }

    [Fact]
    public async Task GetPaged_ReturnsCorrectPaginationMeta()
    {
        var items  = Enumerable.Range(1, 5).Select(_ => new ProductListDto()).ToList();
        var filter = new ProductFilterDto { Page = 2, PageSize = 5 };

        _repo.Setup(r => r.GetPagedAsync(It.IsAny<ProductFilterDto>()))
             .ReturnsAsync((items, 12));

        var result = await BuildSut().GetPagedAsync(filter);

        result.Pagination.TotalCount.Should().Be(12);
        result.Pagination.TotalPages.Should().Be(3);
        result.Pagination.Page.Should().Be(2);
    }

    [Fact]
    public async Task GetPaged_PassesFilterThroughToRepository()
    {
        var filter = new ProductFilterDto { Page = 1, PageSize = 20, CategoryId = 7 };

        _repo.Setup(r => r.GetPagedAsync(It.IsAny<ProductFilterDto>()))
             .ReturnsAsync((new List<ProductListDto>(), 0));

        await BuildSut().GetPagedAsync(filter);

        _repo.Verify(r => r.GetPagedAsync(It.Is<ProductFilterDto>(f => f.CategoryId == 7)), Times.Once);
    }

    // ── GetDetail ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetDetail_ProductNotFound_ReturnsNull()
    {
        _repo.Setup(r => r.GetDetailAsync(It.IsAny<int>()))
             .ReturnsAsync((MerxoSell.API.Models.Product?)null);

        var result = await BuildSut().GetDetailAsync(999);

        result.Should().BeNull();
    }

    // ── Search ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Search_PageSizeAbove100_ClampedTo100()
    {
        var filter = new ProductSearchFilterDto { Page = 1, PageSize = 200, Q = "shirt" };

        _repo.Setup(r => r.SearchAsync(It.IsAny<ProductSearchFilterDto>()))
             .ReturnsAsync((new List<ProductListDto>(), 0));

        await BuildSut().SearchAsync(filter);

        _repo.Verify(r => r.SearchAsync(It.Is<ProductSearchFilterDto>(f => f.PageSize == 100)), Times.Once);
    }

    // ── Suggest ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Suggest_QueryLessThan2Chars_ReturnsEmptyWithoutCallingRepo()
    {
        var result = await BuildSut().SuggestAsync("a");

        result.Should().BeEmpty();
        _repo.Verify(r => r.SuggestAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Suggest_NullQuery_ReturnsEmptyWithoutCallingRepo()
    {
        var result = await BuildSut().SuggestAsync(null!);

        result.Should().BeEmpty();
        _repo.Verify(r => r.SuggestAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Suggest_ValidQuery_DelegatesToRepository()
    {
        var expected = new List<SuggestItemDto> { new SuggestItemDto("product", 1, "T-Shirt", null, "tshirt") };

        _repo.Setup(r => r.SuggestAsync("shirt")).ReturnsAsync(expected);

        var result = await BuildSut().SuggestAsync("shirt");

        result.Should().BeEquivalentTo(expected);
    }
}
