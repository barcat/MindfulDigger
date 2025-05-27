namespace MindfulDigger.Model;

public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public PaginationMetadataDto Pagination { get; set; } = new();
}

public class PaginationMetadataDto
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; } // Reverted from long to int
}

