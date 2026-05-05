namespace Zapas.Api.Extensions;

public static class PaginationExtension
{
    public static IReadOnlyList<T> GetPage<T>(
        this IReadOnlyList<T> items,
        int page,
        int pageSize)
    {
        ValidatePage(page, pageSize);

        return items
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public static IQueryable<T> GetPage<T>(
        this IQueryable<T> query,
        int page,
        int pageSize)
    {
        ValidatePage(page, pageSize);

        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
    }

    private static void ValidatePage(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than 0.");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");
        }
    }
}
