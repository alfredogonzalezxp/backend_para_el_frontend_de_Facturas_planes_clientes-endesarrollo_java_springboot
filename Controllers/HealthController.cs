using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/health")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Ok..Running...");
        }
    }
}
