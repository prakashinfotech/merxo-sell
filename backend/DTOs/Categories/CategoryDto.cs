namespace MerxoSell.API.DTOs.Categories;

public class CategoryDto
{
    public int              CategoryId       { get; set; }
    public string           Name             { get; set; } = string.Empty;
    public string           Slug             { get; set; } = string.Empty;
    public int?             ParentCategoryId { get; set; }
    public string?          ImageUrl         { get; set; }
    /// <summary>Fashion categories drive the variant builder (colour + size matrix).</summary>
    public bool             IsFashion        { get; set; }
    public List<CategoryDto> Children        { get; set; } = [];
}
