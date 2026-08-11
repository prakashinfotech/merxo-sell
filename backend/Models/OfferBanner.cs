namespace MerxoSell.API.Models;

/// <summary>
/// Promotional banner displayed in one of the homepage zones:
/// Hero (full-width), MidLeft, MidRight, or Strip (thin full-width).
/// </summary>
public class OfferBanner
{
    public int      BannerId        { get; set; }

    /// <summary>Homepage zone — Hero | MidLeft | MidRight | Strip.</summary>
    public string   Slot            { get; set; } = "Hero";

    public string   Title           { get; set; } = string.Empty;
    public string?  Subtitle        { get; set; }

    /// <summary>Short pill text overlay, e.g. "50% OFF".</summary>
    public string?  BadgeText       { get; set; }

    /// <summary>Primary banner image URL (via MediaController upload).</summary>
    public string   ImageUrl        { get; set; } = string.Empty;

    /// <summary>Small thumbnail shown in the Hero right column.</summary>
    public string?  SideImageUrl    { get; set; }

    public string?  CtaLabel        { get; set; }
    public string?  CtaUrl          { get; set; }
    public string?  SecondaryLabel  { get; set; }
    public string?  SecondaryUrl    { get; set; }

    /// <summary>Hex background colour override, e.g. "#fff8eb". Null = theme default.</summary>
    public string?  BackgroundColor { get; set; }

    /// <summary>Hex text colour override, e.g. "#1a3a5e".</summary>
    public string?  TextColor       { get; set; }

    /// <summary>Optional FK — clicking the banner navigates to this product.</summary>
    public int?     LinkedProductId  { get; set; }

    /// <summary>Optional FK — clicking the banner navigates to this category.</summary>
    public int?     LinkedCategoryId { get; set; }

    public DateTime? StartsAt        { get; set; }
    public DateTime? EndsAt          { get; set; }
    public int       SortOrder       { get; set; }
    public bool      IsActive        { get; set; } = true;
    public DateTime  CreatedAt       { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt       { get; set; }

    // Navigation
    public Product?  LinkedProduct   { get; set; }
    public Category? LinkedCategory  { get; set; }
}
