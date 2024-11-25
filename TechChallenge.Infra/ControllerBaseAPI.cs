using Microsoft.AspNetCore.Mvc;

namespace TechChallenge.Infrastructure;

[ApiController]
[Route("api/[controller]")]
public class ControllerBaseAPI : ControllerBase
{
    protected IActionResult ReturnResponse<T>(ResponseBase<T> response)
    {
        if (!string.IsNullOrEmpty(response.Message))
            return StatusCode((int)response.StatusCode, response);

        return Ok(response);
    }
}
