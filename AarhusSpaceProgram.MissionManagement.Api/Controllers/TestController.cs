using AarhusSpaceProgram.MissionManagement.Api.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        [HttpPost(Name = "Test")]
        public IActionResult Test([FromBody] CreateScientistDTO dto)
        {
            if (dto == null)
                return BadRequest("DTO is null");
            return Ok(dto);
        }
    }
}
