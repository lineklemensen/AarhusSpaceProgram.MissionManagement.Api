using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;
using AarhusSpaceProgram.MissionManagement.Api.Models.Enums;
using AarhusSpaceProgram.MissionManagement.Api.Logging;
using AarhusSpaceProgram.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MissionsController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<MissionsController> _logger;

        public MissionsController(
            MissionManagementDbContext context, 
            ILogger<MissionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // CREATE
        /// <summary>
        /// Create new mission
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Manager")]
        [HttpPost(Name = "CreateMission")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<MissionListItemDTO>>> Post(CreateMissionDTO dto)
        {
            MissionStatus status;

            if (string.IsNullOrWhiteSpace(dto.Status))
            {
                status = MissionStatus.Created;
            }
            else if (!Enum.TryParse(dto.Status, out status))
            {
                return BadRequest($"Invalid status value: {dto.Status}");
            }

            if (!Enum.TryParse<MissionType>(dto.Type, out var type))
                return BadRequest($"Invalid type value: {dto.Type}");

            var manager = await _context.Managers.FirstOrDefaultAsync(m => m.Id == dto.ManagerId);
            if (manager == null)
                return BadRequest($"Manager with ID {dto.ManagerId} does not exist");

            var mission = new Mission
            {
                Name = dto.Name,
                ManagerId = dto.ManagerId,
                LaunchDate = dto.LaunchDate,
                DurationHours = dto.DurationHours,
                Status = status,
                Type = type
            };

            _context.Missions.Add(mission);
            await _context.SaveChangesAsync();

            var result = new MissionListItemDTO
            {
                Id = mission.Id,
                Name = mission.Name,
                Manager = manager.Name,
                LaunchDate = mission.LaunchDate,
                DurationHours = mission.DurationHours,
                Status = mission.Status.ToString(),
                Type = mission.Type.ToString()
            };

            return Created(
                Url.Action(
                    action: nameof(GetById),
                    controller: "Missions",
                    values: new { id = mission.Id },
                    protocol: Request.Scheme)!,

                new RestDTO<MissionListItemDTO>
                {
                    Data = result,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(
                            Url.Action(
                                action: nameof(GetById),
                                controller: "Missions",
                                values: new { id = mission.Id },
                                protocol: Request.Scheme)!,
                            "self",
                            "GET"),

                        new LinkDTO(
                            Url.Action(
                                action: nameof(Get),
                                controller: "Missions",
                                values: null,
                                protocol: Request.Scheme)!,
                            "collection",
                            "GET")
                    }
                });
        }

        // READ
        /// <summary>
        /// List all missions (simple)
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet(Name = "GetMissions")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<MissionSimpleListItemDTO[]>> Get([FromQuery] string? status = null)
        {
            var mission = _context.Missions
                .AsNoTracking()
                .Select(m => new MissionSimpleListItemDTO
                {
                    Id = m.Id,
                    Name = m.Name,
                    Status = m.Status.ToString()
                });

            return new RestDTO<MissionSimpleListItemDTO[]>
            {
                Data = await mission.ToArrayAsync(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(Get), 
                            controller: "Missions", 
                            values: null, 
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }

        /// <summary>
        /// Get mission by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("{id:int}", Name = "GetMissionById")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult<RestDTO<MissionListItemDTO>>> GetById(int id)
        {
            var mission = await _context.Missions
                .AsNoTracking()
                .Where(m => m.Id == id)
                .Select(m => new MissionListItemDTO
                {
                    Id = m.Id,
                    Name = m.Name,
                    Manager = m.Manager != null ? m.Manager.Name : "Unassigned",
                    LaunchDate = m.LaunchDate,
                    DurationHours = m.DurationHours,
                    Status = m.Status.ToString(),
                    Type = m.Type.ToString()
                })
                .FirstOrDefaultAsync();

            if (mission == null)
                return NotFound();

            return Ok(new RestDTO<MissionListItemDTO>
            {
                Data = mission,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Missions",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        // UPDATE
        /// <summary>
        /// Update mission
        /// </summary>
        /// <remarks>
        /// A mission's status can only be updated according to the following rules:
        /// Constraint 1: A mission cannot move directly from Created to Active
        /// Constraint 2: A mission cannot move from Completed back to Active
        /// Constraint 3: Only Active missions can become Completed, Failed, or Aborted
        /// Constraint 4: A mission cannot become Active without at least 1 assigned astronaut
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Authorize(Roles = "Manager")]
        [HttpPatch("{id:int}", Name = "UpdateMission")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<MissionListItemDTO>>> Patch(int id, UpdateMissionDTO dto)
        {
            var mission = await _context.Missions
                .Include(m => m.Manager)
                .Include(m => m.AstronautAssignments)
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();

            if (mission == null)
                return NotFound();

            if (dto.Name != null)
                mission.Name = dto.Name;

            if (dto.LaunchDate != null)
            {
                if (mission.LaunchpadId.HasValue)
                {
                    var overlappingMission = await _context.Missions
                        .Where(m => m.Id != id 
                                 && m.LaunchpadId == mission.LaunchpadId 
                                 && m.LaunchDate == dto.LaunchDate)
                        .AnyAsync();

                    if (overlappingMission)
                        return BadRequest("The assigned launchpad is already booked for another mission on the requested launch date.");
                }

                mission.LaunchDate = dto.LaunchDate;
            }

            if (dto.DurationHours != null)
                mission.DurationHours = dto.DurationHours;

            if (dto.Status != null)
            {
                if (!Enum.TryParse<MissionStatus>(dto.Status, out var status))
                    return BadRequest($"Invalid status value: {dto.Status}");

                // Constraint 1: Cannot move directly from Created to Active
                if (mission.Status == MissionStatus.Created && status == MissionStatus.Active)
                    return BadRequest("A mission cannot move directly from Created to Active.");

                // Constraint 2: Cannot move from Completed back to Active
                if (mission.Status == MissionStatus.Completed && status == MissionStatus.Active)
                    return BadRequest("A mission cannot move from Completed back to Active.");

                // Constraint 3: Only Active missions can become Completed, Failed, or Aborted
                if ((status == MissionStatus.Completed || status == MissionStatus.Failed || status == MissionStatus.Aborted) 
                    && mission.Status != MissionStatus.Active)
                {
                    return BadRequest($"A mission can only become {status} if its current status is Active.");
                }

                //Constraint 4: Cannot become Active without at least 1 assigned astronaut
                if (status == MissionStatus.Active && mission.AstronautAssignments.Count == 0)
                    return BadRequest("At least 1 astronaut must be assigned before a mission can become Active.");

                // Constraint 5: To become Planned, a mission must have a Target, Launchpad, Rocket, and LaunchDate.
                if (status == MissionStatus.Planned)
                {
                    var missingRequirements = new List<string>();
                    
                    if (mission.CelestialBodyId == null) missingRequirements.Add("Target Celestial Body");
                    if (mission.LaunchpadId == null) missingRequirements.Add("Launchpad");
                    if (mission.RocketId == null) missingRequirements.Add("Rocket");
                    if (mission.LaunchDate == null && dto.LaunchDate == null) missingRequirements.Add("Launch Date");

                    if (missingRequirements.Any())
                    {
                        return BadRequest($"A mission cannot become Planned. Missing requirements: {string.Join(", ", missingRequirements)}");
                    }
                }

                mission.Status = status;
            }

            if (dto.Type != null)
            {
                if (!Enum.TryParse<MissionType>(dto.Type, out var type))
                    return BadRequest($"Invalid type value: {dto.Type}");
                mission.Type = type;
            }

            await _context.SaveChangesAsync();

            var result = new MissionListItemDTO
            {
                Id = mission.Id,
                Name = mission.Name,
                Manager = mission.Manager != null ? mission.Manager.Name : "Unassigned",
                LaunchDate = mission.LaunchDate,
                DurationHours = mission.DurationHours,
                Status = mission.Status.ToString(),
                Type = mission.Type.ToString()
            };

            return Ok(new RestDTO<MissionListItemDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Missions",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        // DELETE
        /// <summary>
        /// Delete mission
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Manager")]
        [HttpDelete("{id:int}", Name = "DeleteMission")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> Delete(int id)
        {
            var mission = await _context.Missions
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();

            if (mission == null)
                return NotFound();

            _context.Missions.Remove(mission);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
