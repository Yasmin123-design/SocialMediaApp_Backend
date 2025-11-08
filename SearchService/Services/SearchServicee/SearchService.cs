using Nest;
using SearchService.Dtos;
using SearchService.Models;
using SearchService.Services.SearchService.SearchService;

namespace SearchService.Services.SearchServicee
{
    public class SearchService : ISearchService
    {
        private readonly IElasticClient _elasticClient;

        public SearchService(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task<IEnumerable<SearchResultDto>> SearchAsync(string query, string? type)
        {
            var response = await _elasticClient.SearchAsync<SearchIndexDocument>(s => s
                .Index("search-index") 
                .Query(q => q
                    .Bool(b => b
                        .Must(
                            m => m.MultiMatch(mm => mm
                                .Fields(f => f.Field(ff => ff.Title).Field(ff => ff.Content))
                                .Query(query)
                            ),
                            m => string.IsNullOrEmpty(type)
                                ? null
                                : m.Term(t => t.Field(f => f.Type).Value(type))
                        )
                    )
                )
            );

            return response.Documents.Select(d => new SearchResultDto
            {
                Id = d.Id,
                Type = d.Type,
                Title = d.Title,
                Content = d.Content
            });
        }
        public async Task<IEnumerable<SearchIndexDocument>> GetAllDocumentsAsync()
        {
            var response = await _elasticClient.SearchAsync<SearchIndexDocument>(s => s
                .Index("search-index") 
                .Query(q => q.MatchAll()) 
                .Size(1000) 
            );

            if (!response.IsValid)
                throw new Exception($"Error retrieving documents: {response.OriginalException.Message}");

            return response.Documents;
        }
        public async Task IndexDocumentAsync(SearchIndexDocument doc)
        {
            var response = await _elasticClient.IndexAsync(doc, i => i
                .Index("search-index")
                .Id(doc.Id)
            );

            if (!response.IsValid)
            {
                throw new Exception($"Failed to index document: {response.OriginalException?.Message}");
            }
        }
        public async Task DeleteDocumentAsync(string id)
        {
            var response = await _elasticClient.DeleteAsync<SearchIndexDocument>(id, d => d
                .Index("search-index")
            );

            if (!response.IsValid)
            {
                throw new Exception($"Failed to delete document with Id {id}: {response.OriginalException?.Message}");
            }
        }


    }
}
