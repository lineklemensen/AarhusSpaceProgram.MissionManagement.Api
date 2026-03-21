using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;


namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ScientistsController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<ScientistsController> _logger;

        public ScientistsController(
            MissionManagementDbContext context,
            ILogger<ScientistsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet(Name = "GetScientists")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<ScientistListItemDTO[]>> Get()
        {
            var query = _context.Scientists
                .AsNoTracking()
                .Select(s => new ScientistListItemDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Title = s.Title,
                    Specialty = s.Specialty,
                    HireDate = s.HireDate
                });
            return new RestDTO<ScientistListItemDTO[]>
            {
                Data = await query.ToArrayAsync(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            null,
                            "Scientists",
                            null,
                            Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }
    }
}
