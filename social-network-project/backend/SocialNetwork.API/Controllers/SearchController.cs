using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SocialNetwork.API.Services;

namespace SocialNetwork.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        private int GetCurrentUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub") ?? "0");

        /// <summary>Search users by username, full name, or bio using Trie O(L)</summary>
        [HttpGet("users")]
        public async Task<IActionResult> SearchUsers(
            [FromQuery] string q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { message = "Query parameter 'q' is required." });

            int currentUserId = GetCurrentUserId();
            var results = await _searchService.SearchUsersAsync(q, currentUserId, page, pageSize);
            return Ok(new { results, query = q, count = results.Count });
        }

        /// <summary>Autocomplete username suggestions using Trie O(L)</summary>
        [HttpGet("autocomplete")]
        public async Task<IActionResult> AutoComplete([FromQuery] string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return Ok(new { suggestions = new List<string>() });

            var suggestions = await _searchService.AutoCompleteAsync(prefix);
            return Ok(new { suggestions });
        }
    }
}
