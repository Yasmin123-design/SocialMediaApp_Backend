using SearchService.Dtos;
using SearchService.Models;

namespace SearchService.Services.SearchService.SearchService
{
    public interface ISearchService
    {
        Task DeleteDocumentAsync(string id);
        Task<IEnumerable<SearchResultDto>> SearchAsync(string query, string? type);
        Task IndexDocumentAsync(SearchIndexDocument doc);
        Task<IEnumerable<SearchIndexDocument>> GetAllDocumentsAsync();

    }
}
