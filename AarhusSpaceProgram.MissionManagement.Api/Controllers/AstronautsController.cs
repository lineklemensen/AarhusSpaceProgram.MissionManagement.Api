using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AstronautsController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<AstronautsController> _logger;

        public AstronautsController(
            MissionManagementDbContext context, 
            ILogger<AstronautsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet(Name = "GetAstronauts")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<AstronautListItemDTO[]>> Get()
        {
            var query = _context.Astronauts
                .AsNoTracking()
                .Select(a => new AstronautListItemDTO
                {
                    Id = a.Id,
                    Name = a.Name,
                    Rank = a.Rank.ToString(),
                    Paygrade = a.Paygrade,
                    HoursInSimulation = a.HoursInSimulation,
                    HoursInSpace = a.HoursInSpace
                });

            return new RestDTO<AstronautListItemDTO[]>
            {
                Data = await query.ToArrayAsync(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            null,
                            "Astronauts",
                            null,
                            Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }
    }
}
