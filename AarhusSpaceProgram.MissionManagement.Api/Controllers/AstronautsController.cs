using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;
using AarhusSpaceProgram.MissionManagement.Api.Models.Enums;
using AarhusSpaceProgram.MissionManagement.Api.Services;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AstronautsController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly StaffService _staffService;

        private readonly ILogger<AstronautsController> _logger;

        public AstronautsController(
            MissionManagementDbContext context,
            StaffService staffService,
            ILogger<AstronautsController> logger)
        {
            _context = context;
            _staffService = staffService;
            _logger = logger;
        }

        // CREATE
        /// <summary>
        /// Create a new astronaut
        /// </summary>
        /// <remarks>
        /// The Rank property must be one of the following values: Candidate, Astronaut, Pilot, Commander, MissionSpecialist, PayloadSpecialist, EVASpecialist. 
        /// HoursInSimulation and HoursInSpace cannot be negative.
        /// </remarks>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost(Name = "CreateAstronaut")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<AstronautListItemDTO>>> Post(CreateAstronautDTO dto)
        {
            try
            {
                if (!Enum.TryParse<AstronautRank>(dto.Rank, out var rank))
                    return BadRequest($"Invalid rank: {dto.Rank}");

                // Create user
                var userResult = await _staffService.CreateUser(
                    new CreateUserDTO
                    {
                        Name = dto.Name,
                        Role = "Astronaut"
                    }
                );

                // Create astronaut
                var astronaut = new Astronaut
                {
                    Name = dto.Name,
                    Rank = rank,
                    Paygrade = dto.Paygrade,
                    HoursInSimulation = dto.HoursInSimulation,
                    HoursInSpace = dto.HoursInSpace,
                    User = await _context.Users.FindAsync(userResult.Id)
                };

                // Add astronaut to the database
                _context.Astronauts.Add(astronaut);
                await _context.SaveChangesAsync();

                // Log user credentials to testUsers.txt for testing purposes
                var repoRoot = AppContext.BaseDirectory;
                var testUsersFilePath = Path.Combine(repoRoot, "testUsers.txt");
                await System.IO.File.AppendAllTextAsync(
                    Path.GetFullPath(testUsersFilePath),
                    $"{userResult.Id},{userResult.TemporaryPassword}{Environment.NewLine}");

                var result = new AstronautListItemDTO
                {
                    Id = astronaut.Id,
                    Name = astronaut.Name,
                    Rank = astronaut.Rank.ToString(),
                    Paygrade = astronaut.Paygrade,
                    HoursInSimulation = astronaut.HoursInSimulation,
                    HoursInSpace = astronaut.HoursInSpace
                };

                return Created(
                    Url.Action(
                        action: nameof(GetById),
                        controller: "Astronauts",
                        values: new { id = astronaut.Id },
                        protocol: Request.Scheme)!,

                    new RestDTO<AstronautListItemDTO>
                    {
                        Data = result,
                        Links = new List<LinkDTO>
                        {
                            new LinkDTO(
                                Url.Action(
                                    action: nameof(GetById),
                                    controller: "Astronauts",
                                    values: new { id = astronaut.Id },
                                    protocol: Request.Scheme)!,
                                "self",
                                "GET"),

                            new LinkDTO(
                                Url.Action(
                                    action: nameof(Get),
                                    controller: "Astronauts",
                                    values: null,
                                    protocol: Request.Scheme)!,
                                "collection",
                                "GET"),
                        }
                    });
            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occurred while creating the astronaut: {e.Message}");
            }
        }

        // READ
        /// <summary>
        /// List all astronauts
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "GetAstronauts")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<AstronautListItemDTO[]>> Get()
        {
            var astronaut = _context.Astronauts
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
                Data = await astronaut.ToArrayAsync(),
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

        /// <summary>
        /// Get astronaut by ID
        /// </summary>
        /// <remarks>
        /// You can search for a specific astronaut by the astronaut's unique ID. The response will include the astronaut's name, rank, paygrade, hours in simulation, and hours in space.
        /// </remarks> 
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetAstronautById")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult<RestDTO<AstronautListItemDTO>>> GetById(string id)
        {
            var astronaut = await _context.Astronauts
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AstronautListItemDTO
                {
                    Id = a.Id,
                    Name = a.Name,
                    Rank = a.Rank.ToString(),
                    Paygrade = a.Paygrade,
                    HoursInSimulation = a.HoursInSimulation,
                    HoursInSpace = a.HoursInSpace
                })
                .FirstOrDefaultAsync();

            if (astronaut == null)
                return NotFound();

            return Ok(new RestDTO<AstronautListItemDTO>
            {
                Data = astronaut,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Astronauts",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        /// <summary>
        /// Astronauts ordered by experience
        /// </summary>
        /// <remarks>
        /// Get a list of all astronauts, ordered by experience. The list is descending, so the most experienced astronauts are at the top.
        /// Experience is determined by hours in space. If two astronauts have the same hours in space, the tiebreaker is hours in simulation.
        /// </remarks>
        /// <returns></returns>
        [HttpGet("experience", Name = "GetAstronautsByExperience")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<AstronautExperienceListItemDTO>>> GetByExperience()
        {
            var astronauts = await _context.Astronauts
                .AsNoTracking()
                .OrderByDescending(a => a.HoursInSpace)
                .ThenByDescending(a => a.HoursInSimulation)
                .Select(static a => new AstronautExperienceListItemDTO
                {
                    Id = a.Id,
                    Name = a.Name,
                    HoursInSpace = a.HoursInSpace,
                    HoursInSimulation = a.HoursInSimulation,
                })
                .ToArrayAsync();

            return Ok(new RestDTO<AstronautExperienceListItemDTO[]>
            {
                Data = astronauts,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetByExperience),
                            controller: "Astronauts",
                            values: null,
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        // UPDATE
        /// <summary>
        /// Update astronaut info
        /// </summary>
        /// <remarks>
        /// Use the astronaut ID to update the astronaut's information. You can update any of the following properties: Name, Rank, Paygrade, HoursInSimulation, HoursInSpace. The Rank property must be one of the following values: Candidate, Astronaut, Pilot, Commander, MissionSpecialist, PayloadSpecialist, EVASpecialist. HoursInSimulation and HoursInSpace cannot be negative or decreased.
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPatch("{id}", Name = "UpdateAstronaut")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult> Patch(string id, UpdateAstronautDTO dto)
        {
            var astronaut = await _context.Astronauts
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();

            if (astronaut == null)
                return NotFound();

            if (dto.Name != null)
                astronaut.Name = dto.Name;

            if (dto.Rank != null)
            {
                if (!Enum.TryParse<AstronautRank>(dto.Rank, out var rank))
                    return BadRequest($"Invalid rank: {dto.Rank}");
                astronaut.Rank = rank;
            }

            if (dto.Paygrade != null)
                astronaut.Paygrade = dto.Paygrade;

            if (dto.HoursInSimulation != null)
            {
                if (dto.HoursInSimulation < 0)
                {
                    return BadRequest("Hours in simulation cannot be negative.");
                }
                else if (dto.HoursInSimulation < astronaut.HoursInSimulation)
                {
                    return BadRequest("Hours in simulation cannot be decreased.");
                }

                astronaut.HoursInSimulation = dto.HoursInSimulation.Value;
            }

            if (dto.HoursInSpace != null)
            {
                if (dto.HoursInSpace < 0)
                {
                    return BadRequest("Hours in space cannot be negative.");
                }
                else if (dto.HoursInSpace < astronaut.HoursInSpace)
                {
                    return BadRequest("Hours in space cannot be decreased.");
                }

                astronaut.HoursInSpace = dto.HoursInSpace.Value;
            }

            await _context.SaveChangesAsync();

            var result = new AstronautListItemDTO
            {
                Name = astronaut.Name,
                Rank = astronaut.Rank.ToString(),
                Paygrade = astronaut.Paygrade,
                HoursInSimulation = astronaut.HoursInSimulation,
                HoursInSpace = astronaut.HoursInSpace
            };

            return Ok(new RestDTO<AstronautListItemDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Astronauts",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        // DELETE
        /// <summary>
        /// Delete an astronaut
        /// </summary>
        /// <remarks>
        /// Use the astronaut ID to delete the astronaut from the system. This action is irreversible and will remove all records of the astronaut, including their mission assignments. Use with caution.
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}", Name = "DeleteAstronaut")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult> Delete(string id)
        {
            var astronaut = await _context.Astronauts
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();

            if (astronaut == null)
                return NotFound();

            _context.Astronauts.Remove(astronaut);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
