namespace backend.Contracts.Search;

public class SearchRequest
{
    public string Text { get; set; } = string.Empty;
    public string? Graph { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
