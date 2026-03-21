using AarhusSpaceProgram.MissionManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MissionsController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                message = "MissionsController is running",
                timestampUtc = DateTime.UtcNow
            });
        }

        // TEST endpoint: kaster en exception for at trigge Developer Exception Page
        [HttpGet("boom")]
        public IActionResult Boom()
        {
            throw new InvalidOperationException("DEV TEST: MissionsController /missions/boom threw an exception.");
        }
    }
}
