using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MissionsController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<MissionsController> _logger;

        public MissionsController(
            MissionManagementDbContext context, 
            ILogger<MissionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet(Name = "GetMissions")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<MissionListDTO[]>> Get()
        {
            var query = _context.Missions
                .AsNoTracking()
                .Select(m => new MissionListDTO
                {
                    Id = m.Id,
                    Name = m.Name,
                    Status = m.Status.ToString()
                });

            return new RestDTO<MissionListDTO[]>
            {
                Data = await query.ToArrayAsync(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(null, "Missions", null, Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }
    }
}
