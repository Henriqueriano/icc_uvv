using backend.Contracts.Search;

namespace backend.Services.Search;

public interface ISearchService
{
    Task<string> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default);
    Task<string> SuggestionsAsync(string text, CancellationToken cancellationToken = default);
}
