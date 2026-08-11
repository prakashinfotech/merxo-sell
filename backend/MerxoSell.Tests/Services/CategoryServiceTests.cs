using FluentAssertions;
using Moq;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services;

namespace MerxoSell.Tests.Services;

public class CategoryServiceTests : TestBase
{
    private readonly Mock<ICategoryRepository> _repo = new();
    private CategoryService BuildSut() => new(_repo.Object);

    [Fact]
    public async Task GetTree_EmptyList_ReturnsEmpty()
    {
        _repo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(new List<Category>());

        var result = await BuildSut().GetTreeAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTree_FlatCategories_ReturnsAllAsRoots()
    {
        var cats = new List<Category>
        {
            MakeCategory(1, "Electronics"),
            MakeCategory(2, "Clothing"),
            MakeCategory(3, "Books"),
        };
        _repo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(cats);

        var result = (await BuildSut().GetTreeAsync()).ToList();

        result.Should().HaveCount(3);
        result.All(c => c.Children.Count == 0).Should().BeTrue();
    }

    [Fact]
    public async Task GetTree_NestedCategories_BuildsParentChildRelationship()
    {
        var cats = new List<Category>
        {
            MakeCategory(1, "Electronics"),
            MakeCategory(2, "Phones", parentId: 1),
            MakeCategory(3, "Laptops", parentId: 1),
        };
        _repo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(cats);

        var result = (await BuildSut().GetTreeAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Electronics");
        result[0].Children.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTree_ThreeLevelHierarchy_CorrectlyNested()
    {
        var cats = new List<Category>
        {
            MakeCategory(1, "Root"),
            MakeCategory(2, "Mid", parentId: 1),
            MakeCategory(3, "Leaf", parentId: 2),
        };
        _repo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(cats);

        var root = (await BuildSut().GetTreeAsync()).Single();

        root.Children.Should().HaveCount(1);
        root.Children.First().Children.Should().HaveCount(1);
        root.Children.First().Children.First().Name.Should().Be("Leaf");
    }

    [Fact]
    public async Task GetTree_MultipleRootsEachWithChildren_GroupsCorrectly()
    {
        var cats = new List<Category>
        {
            MakeCategory(1, "A"),
            MakeCategory(2, "A1", parentId: 1),
            MakeCategory(3, "B"),
            MakeCategory(4, "B1", parentId: 3),
            MakeCategory(5, "B2", parentId: 3),
        };
        _repo.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(cats);

        var result = (await BuildSut().GetTreeAsync()).ToList();

        result.Should().HaveCount(2);
        result.First(c => c.Name == "B").Children.Should().HaveCount(2);
    }
}
