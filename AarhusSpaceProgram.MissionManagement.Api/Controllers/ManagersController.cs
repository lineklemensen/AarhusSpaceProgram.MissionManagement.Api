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

        // CREATE
        /// <summary>
        /// Create manager
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost(Name = "CreateManager")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<ManagerListItemDTO>>> Post(CreateManagerDTO dto)
        {
            var manager = new Manager
            {
                Name = dto.Name
            };

            _context.Managers.Add(manager);
            await _context.SaveChangesAsync();

            var result = new ManagerListItemDTO
            {
                Id = manager.Id,
                Name = manager.Name
            };

            return Created(
                Url.Action(
                    action: nameof(GetById),
                    controller: "Managers",
                    values: new { id = manager.Id },
                    protocol: Request.Scheme)!,

                new RestDTO<ManagerListItemDTO>
                {
                    Data = result,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(
                            Url.Action(
                                action: nameof(GetById),
                                controller: "Managers",
                                values: new { id = manager.Id },
                                protocol: Request.Scheme)!,
                            "self",
                            "GET"),

                        new LinkDTO(
                            Url.Action(
                                action: nameof(Get),
                                controller: "Managers",
                                values: null,
                                protocol: Request.Scheme)!,
                            "collection",
                            "GET"),
                    }
                });
        }

        // READ
        /// <summary>
        /// List all managers
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "GetManagers")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<ManagerListItemDTO[]>> Get()
        {
            var manager = _context.Managers
                .AsNoTracking()
                .Select(m => new ManagerListItemDTO
                {
                    Id = m.Id,
                    Name = m.Name
                });

            return new RestDTO<ManagerListItemDTO[]>
            {
                Data = await manager.ToArrayAsync(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(Get),
                            controller: "Managers",
                            values: null,
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }

        /// <summary>
        /// Get manager by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        // UPDATE
        /// <summary>
        /// Update manager info
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id:int}", Name = "UpdateManager")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<ManagerListItemDTO>>> Put(int id, UpdateManagerDTO dto)
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

        // DELETE
        /// <summary>
        /// Delete manager
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}", Name = "DeleteManager")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> Delete(int id)
        {
            var manager = await _context.Managers
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();

            if (manager == null)
                return NotFound();

            _context.Managers.Remove(manager);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
