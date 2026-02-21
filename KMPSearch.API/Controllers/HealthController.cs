using Microsoft.AspNetCore.Mvc;

namespace KMPSearch.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "healthy",
            service = "KMP Search API",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
