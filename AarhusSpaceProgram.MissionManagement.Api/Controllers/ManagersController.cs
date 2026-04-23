using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;
using AarhusSpaceProgram.MissionManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ManagersController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly StaffService _staffService;

        private readonly ILogger<ManagersController> _logger;

        public ManagersController(
            MissionManagementDbContext context,
            StaffService staffService,
            ILogger<ManagersController> logger)
        {
            _context = context;
            _staffService = staffService;
            _logger = logger;
        }

        // CREATE
        /// <summary>
        /// Create manager
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost(Name = "CreateManager")]
        [Authorize(Roles = "Manager")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<ManagerListItemDTO>>> Post(CreateManagerDTO dto)
        {
            try
            {
                // Create user
                var userResult = await _staffService.CreateUser(
                    new CreateUserDTO
                    {
                        Name = dto.Name,
                        Role = "Manager"
                    }
                );

                // Create manager
                var manager = new Manager
                {
                    Name = dto.Name,
                    User = await _context.Users.FindAsync(userResult.Id)
                };

                // Add manager to database
                _context.Managers.Add(manager);
                await _context.SaveChangesAsync();

                var repoRoot = @"C:\Users\linen\AUBEng_offline\sw4\bad\AarhusSpaceProgram.MissionManagement.Api";
                var testUsersFilePath = Path.Combine(repoRoot, "testUsers.txt");
                await System.IO.File.AppendAllTextAsync(
                    testUsersFilePath,
                    $"{userResult.Id},{userResult.TemporaryPassword}{Environment.NewLine}");

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
            catch (Exception e)
            {
                return StatusCode(500, $"An error occurred while creating the manager: {e.Message}");
            }
        }

        // READ
        /// <summary>
        /// List all managers
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "GetManagers")]
        [Authorize]
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
        [HttpGet("{id}", Name = "GetManagerById")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult<RestDTO<ManagerListItemDTO>>> GetById(string id)
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
        [HttpPut("{id}", Name = "UpdateManager")]
        [Authorize(Roles = "Manager")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<ManagerListItemDTO>>> Put(string id, UpdateManagerDTO dto)
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
        [HttpDelete("{id}", Name = "DeleteManager")]
        [Authorize(Roles = "Manager")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> Delete(string id)
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
