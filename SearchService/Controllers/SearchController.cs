using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchService.Models;

namespace SearchService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly Services.SearchService.SearchService.ISearchService _searchService;

        public SearchController(Services.SearchService.SearchService.ISearchService searchService)
        {
            _searchService = searchService;
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllDocuments()
        {
            var results = await _searchService.GetAllDocumentsAsync();
            return Ok(results);
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] string? type)
        {
            var results = await _searchService.SearchAsync(query, type);
            return Ok(results);
        }

        [HttpPost("index")]
        public async Task<IActionResult> IndexDocument([FromBody] SearchIndexDocument doc)
        {
            await _searchService.IndexDocumentAsync(doc);
            return Ok("Document indexed successfully");
        }
    }
}

