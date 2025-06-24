using GenAIExpertEngineAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GenAIExpertEngineAPI.Controllers
{
    public class ExpertQueryResponse
    {
        public string Response { get; set; } = string.Empty;
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ExpertController : ControllerBase
    {
        //private readonly OrchestratorService _orchestratorService;
        private readonly RefereeService _refereeService;

        public ExpertController(RefereeService refereeService)
        {
            _refereeService = refereeService;
        }

        [HttpPost("query")]
        public async Task<IActionResult> ProcessQuery([FromBody] UserQueryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserText))
            {
                return BadRequest("User query cannot be empty.");
            }
            if (string.IsNullOrWhiteSpace(request.ConversationId))
            {
                return BadRequest("Conversation ID cannot be empty.");
            }

            // Call the RefereeService, which now returns RefereeResponseOutput
            RefereeResponseOutput responseOutput = await _refereeService.GenerateRefereeResponseAsync(request.ConversationId, request.UserText);

            // Return the RefereeResponseOutput object directly.
            // ASP.NET Core will automatically serialize this C# object into a JSON response.
            return Ok(responseOutput);
        }
    }

    // Example class for the incoming user query request body
    public class UserQueryRequest
    {
        public string? ConversationId { get; set; }
        public string? UserText { get; set; }
    }
}
