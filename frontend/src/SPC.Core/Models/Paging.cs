namespace SPC.Core.Models;

public static class Paging
{
    public const int DefaultPageSize = 10;

    public static readonly int[] PageSizes = [10, 25, 50];

    public static int NormalizePageSize(int pageSize) =>
        PageSizes.Contains(pageSize) ? pageSize : DefaultPageSize;

    public static PagedResult<T> Slice<T>(IReadOnlyList<T> source, int page, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(source);

        pageSize = NormalizePageSize(pageSize);
        var totalCount = source.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);

        var items = source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
        };
    }
}
