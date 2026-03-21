using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RocketsController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<RocketsController> _logger;

        public RocketsController(
            MissionManagementDbContext context,
            ILogger<RocketsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet(Name = "GetRockets")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<RocketListItemDTO[]>> Get()
        {
            var query = _context.Rockets
                .AsNoTracking()
                .Select(r => new RocketListItemDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    PayloadCapacityKg = r.PayloadCapacityKg,
                    CrewCapacity = r.CrewCapacity,
                    NumberOfStages = r.NumberOfStages,
                    FuelCapacityKg = r.FuelCapacityKg,
                    WeightKg = r.WeightKg
                });

            return new RestDTO<RocketListItemDTO[]>
            {
                Data = await query.ToArrayAsync(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            null,
                            "Rockets",
                            null,
                            Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }
    }
}
