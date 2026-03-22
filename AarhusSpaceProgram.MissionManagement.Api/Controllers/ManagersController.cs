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

        [HttpGet("{id:int}", Name = "GetManagerById")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult<RestDTO<ManagerListItemDTO>>> GetById(int id)
        {
            var manager = await _context.Managers
                .AsNoTracking()
                .Where(m => m.Id == id)
                .Select(m => new ManagerListItemDTO
                {
                    Id = m.Id,
                    Name = m.Name
                })
                .FirstOrDefaultAsync();

            if (manager == null)
                return NotFound();

            return Ok(new RestDTO<ManagerListItemDTO>
            {
                Data = manager,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Managers",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }
    
        //[HttpPost(Name = "CreateManager")]
        //public async Task<ActionResult<RestDTO<ManagerListItemDTO>>>

        [HttpPut("{id:int}", Name = "UpdateManager")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<ManagerListItemDTO>>> Put(int id, ManagerUpdateDTO dto)
        {
            var manager = await _context.Managers
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();

            if (manager == null)
                return NotFound();

            manager.Name = dto.Name;

            await _context.SaveChangesAsync();

            var result = new ManagerListItemDTO
            {
                Id = manager.Id,
                Name = manager.Name
            };

            return Ok(new RestDTO<ManagerListItemDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Managers",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET")
                }
            });
        }
    }
}
