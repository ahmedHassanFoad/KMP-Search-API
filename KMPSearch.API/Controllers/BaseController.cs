using Microsoft.AspNetCore.Mvc;

namespace KMPSearch.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
}
