namespace MerxoSell.API.DTOs.Common;

public class PagedResult<T>
{
    public IEnumerable<T> Data       { get; init; } = [];
    public PaginationMeta Pagination { get; init; } = new();
}

public class PaginationMeta
{
    public int Page       { get; init; }
    public int PageSize   { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}
