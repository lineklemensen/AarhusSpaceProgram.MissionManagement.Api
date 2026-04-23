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
    public class ScientistsController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly StaffService _staffService;

        private readonly ILogger<ScientistsController> _logger;

        public ScientistsController(
            MissionManagementDbContext context,
            StaffService staffService,
            ILogger<ScientistsController> logger)
        {
            _context = context;
            _staffService = staffService;
            _logger = logger;
        }

        // CREATE
        /// <summary>
        /// Create scientist
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost(Name = "CreateScientist")]
        [Authorize(Roles = "Manager")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<ScientistListItemDTO>>> Post([FromBody] CreateScientistDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    _logger.LogError("CreateScientistDTO is null. Check request body and Content-Type header.");
                    return BadRequest("Scientist data is required.");
                }

                // Crease user
                var userResult = await _staffService.CreateUser(
                    new CreateUserDTO
                    {
                        Name = dto.Name,
                        Role = "Scientist"
                    }
                );

                // Create scientist
                var scientist = new Scientist
                {
                    Name = dto.Name,
                    Title = dto.Title,
                    Specialty = dto.Specialty,
                    HireDate = dto.HireDate,
                    User = await _context.Users.FindAsync(userResult.Id)
                };

                // Add scientist to the database
                _context.Scientists.Add(scientist);
                await _context.SaveChangesAsync();

                // Log user credentials to testUsers.txt for testing purposes
                var repoRoot = @"C:\Users\linen\AUBEng_offline\sw4\bad\AarhusSpaceProgram.MissionManagement.Api";
                var testUsersFilePath = Path.Combine(repoRoot, "testUsers.txt");
                await System.IO.File.AppendAllTextAsync(
                    testUsersFilePath,
                    $"{userResult.Id},{userResult.TemporaryPassword}{Environment.NewLine}");

                var result = new ScientistListItemDTO
                {
                    Id = scientist.Id,
                    Name = scientist.Name,
                    Title = scientist.Title,
                    Specialty = scientist.Specialty,
                    HireDate = scientist.HireDate
                };

                return Created(
                    Url.Action(
                        action: nameof(GetById),
                        controller: "Scientists",
                        values: new { id = scientist.Id },
                        protocol: Request.Scheme)!,

                    new RestDTO<ScientistListItemDTO>
                    {
                        Data = result,
                        Links = new List<LinkDTO>
                        {
                            new LinkDTO(
                                Url.Action(
                                    action: nameof(GetById),
                                    controller: "Scientists",
                                    values: new { id = scientist.Id },
                                    protocol: Request.Scheme)!,
                                "self",
                                "GET"),

                            new LinkDTO(
                                Url.Action(
                                    action: nameof(Get),
                                    controller: "Scientists",
                                    values: null,
                                    protocol: Request.Scheme)!,
                                "collection",
                                "GET"),
                        }
                    });
            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occurred while creating the scientist: {e.Message}");
            }
        }

        // READ
        /// <summary>
        /// List all scientists
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "GetScientists")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<ScientistListItemDTO[]>> Get()
        {
            var scientist = _context.Scientists
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
                Data = await scientist.ToArrayAsync(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(Get),
                            controller: "Scientists",
                            values: null,
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }

        /// <summary>
        /// Get scientist by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetScientistById")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult<RestDTO<ScientistListItemDTO>>> GetById(string id)
        {
            var scientist = await _context.Scientists
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new ScientistListItemDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Title = s.Title,
                    Specialty = s.Specialty,
                    HireDate = s.HireDate
                })
                .FirstOrDefaultAsync();

            if (scientist == null)
                return NotFound();

            return Ok(new RestDTO<ScientistListItemDTO>
            {
                Data = scientist,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Scientists",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        // UPDATE
        /// <summary>
        /// Update scientist info
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPatch("{id}", Name = "UpdateScientist")]
        [Authorize(Roles = "Manager")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult> Patch(string id, UpdateScientistDTO dto)
        {
            var scientist = await _context.Scientists
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync();

            if (scientist == null)
                return NotFound();

            if (dto.Name != null)
                scientist.Name = dto.Name;

            if (dto.Title != null)
                scientist.Title = dto.Title;

            if (dto.Specialty != null)
                scientist.Specialty = dto.Specialty;

            await _context.SaveChangesAsync();

            var result = new ScientistListItemDTO
            {
                Id = scientist.Id,
                Name = scientist.Name,
                Title = scientist.Title,
                Specialty = scientist.Specialty,
                HireDate = scientist.HireDate
            };

            return Ok(new RestDTO<ScientistListItemDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Scientists",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        // DELETE
        /// <summary>
        /// Delete scientist
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}", Name = "DeleteScientist")]
        [Authorize(Roles = "Manager")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult> Delete(string id)
        {
            var scientist = await _context.Scientists
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync();

            if (scientist == null)
                return NotFound();

            _context.Scientists.Remove(scientist);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
