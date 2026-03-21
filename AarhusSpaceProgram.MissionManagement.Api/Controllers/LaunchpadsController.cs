using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LaunchpadsController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<LaunchpadsController> _logger;

        public LaunchpadsController(
            MissionManagementDbContext context,
            ILogger<LaunchpadsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet(Name = "GetLaunchpads")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<LaunchpadListItemDTO[]>> Get()
        {
            var query = _context.Launchpads
                .AsNoTracking()
                .Select(l => new LaunchpadListItemDTO
                {
                    Id = l.Id,
                    PadCode = l.PadCode,
                    Location = l.Location,
                    Status = l.Status.ToString(),
                    MaxSupportedWeightKg = l.MaxSupportedWeightKg
                });
            return new RestDTO<LaunchpadListItemDTO[]>
            {
                Data = await query.ToArrayAsync(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            null,
                            "Launchpads",
                            null,
                            Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }
    }
}
