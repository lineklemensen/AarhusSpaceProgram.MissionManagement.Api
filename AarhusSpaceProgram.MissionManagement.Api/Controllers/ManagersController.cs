using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ManagersController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<ManagersController> _logger;

        public ManagersController(
            MissionManagementDbContext context,
            ILogger<ManagersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet(Name = "GetManagers")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<ManagerListItemDTO[]>> Get()
        {
            var query = _context.Managers
                .AsNoTracking()
                .Select(m => new ManagerListItemDTO
                {
                    Id = m.Id,
                    Name = m.Name
                });

            return new RestDTO<ManagerListItemDTO[]>
            {
                Data = await query.ToArrayAsync(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            null,
                            "Managers",
                            null,
                            Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }
    }
}
