namespace MerxoSell.API.Models;

public class Category
{
    public int      CategoryId       { get; set; }
    public int?     ParentCategoryId { get; set; }
    public string   Name             { get; set; } = string.Empty;
    public string   Slug             { get; set; } = string.Empty;
    public string?  ImageUrl         { get; set; }
    public int      SortOrder        { get; set; }
    public bool     IsActive         { get; set; } = true;

    /// <summary>
    /// Fashion-style products that warrant a colour/size variant matrix.
    /// Sellers only see the variant builder when listing under one of these
    /// categories; everywhere else, products use the flat stock model.
    /// </summary>
    public bool     IsFashion        { get; set; }

    public Category?             ParentCategory { get; set; }
    public ICollection<Category> SubCategories  { get; set; } = [];
    public ICollection<Product>  Products       { get; set; } = [];
}
