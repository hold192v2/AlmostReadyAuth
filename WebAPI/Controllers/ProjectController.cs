using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("project")]
    public class ProjectController : ControllerBase
    {
        [HttpGet("user")]
        [Authorize(Roles = "USER")]
        public ActionResult User()
        {
            return Ok("Hello User");
        }

        [HttpGet("admin")]
        [Authorize(Roles = "ADMIN")]
        public ActionResult Admin()
        {
            return Ok("Hello, Admin");
        }
    }
}
