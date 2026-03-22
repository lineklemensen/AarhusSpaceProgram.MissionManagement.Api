using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;
using AarhusSpaceProgram.MissionManagement.Api.Models.Enums;

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
        [HttpPost(Name = "CreateMission")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<MissionListItemDTO>>> Post(CreateMissionDTO dto)
        {
            if (!Enum.TryParse<MissionStatus>(dto.Status, out var status))
                return BadRequest($"Invalid status value: {dto.Status}");

            if (!Enum.TryParse<MissionType>(dto.Type, out var type))
                return BadRequest($"Invalid type value: {dto.Type}");

            var mission = new Mission
            {
                Name = dto.Name,
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
        [HttpGet(Name = "GetMissions")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<MissionSimpleListItemDTO[]>> Get()
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
                            null, 
                            "Missions", 
                            null, 
                            Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }

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
                    Status = m.Status.ToString()
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
        [HttpPatch("{id:int}", Name = "UpdateMission")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<MissionListItemDTO>>> Patch(int id, UpdateMissionDTO dto)
        {
            var mission = await _context.Missions
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();

            if (mission == null)
                return NotFound();

            if (dto.Name != null)
                mission.Name = dto.Name;

            if (dto.LaunchDate != null)
                mission.LaunchDate = dto.LaunchDate;

            if (dto.DurationHours != null)
                mission.DurationHours = dto.DurationHours;

            if (dto.Status != null)
            {
                if (!Enum.TryParse<MissionStatus>(dto.Status, out var status))
                    return BadRequest($"Invalid status value: {dto.Status}");
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

        // Assignments
        [HttpPost("{missionId:int}/astronauts", Name = "AssignAstronautsToMission")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<object>>> AssignAstronauts(int missionId, AssignAstronautsToMissionDTO dto)
        {
            var missionExists = await _context.Missions
                .Where(m => m.Id == missionId)
                .AnyAsync();

            if (!missionExists)
                return NotFound($"Mission with id {missionId} not found.");

            var requestedAstronautIds = dto.Astronauts
                .Select(a => a.AstronautId)
                .Distinct()
                .ToList();

            if (requestedAstronautIds.Count == 0)
                return BadRequest("No astronaut IDs provided.");

            var astronauts = await _context.Astronauts
                .Where(a => requestedAstronautIds.Contains(a.Id))
                .Select(a => a.Id)
                .ToListAsync();

            var missingAstronautIds = requestedAstronautIds
                .Except(astronauts)
                .ToList();

            if (missingAstronautIds.Count > 0)
                return BadRequest($"The following astronaut IDs do not exist: {string.Join(", ", missingAstronautIds)}");

            var alreadyAssignedAstronautIds = await _context.MissionAstronautAssignments
                .Where(ma => ma.MissionId == missionId && requestedAstronautIds.Contains(ma.AstronautId))
                .Select(ma => ma.AstronautId)
                .ToListAsync();

            var astronautIdsToAssign = requestedAstronautIds
                .Except(alreadyAssignedAstronautIds)
                .ToList();

            foreach (var astronautId in astronautIdsToAssign)
            {
                _context.MissionAstronautAssignments.Add(new MissionAstronautAssignment
                {
                    MissionId = missionId,
                    AstronautId = astronautId
                });
            }

            await _context.SaveChangesAsync();

            var response = new
            {
                MissionId = missionId,
                AssignedAstronautIds = astronautIdsToAssign,
                AlreadyAssignedAstronautIds = alreadyAssignedAstronautIds
            };

            return Ok(new RestDTO<object>
            {
                Data = response,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Missions",
                            values: new { id = missionId },
                            protocol: Request.Scheme)!,
                        "mission",
                        "GET"),
                }
            });
        }

        // Removals
        [HttpPost("{missionId:int}/astronauts/remove", Name = "RemoveAstronautsFromMission")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<object>>> RemoveAstronauts(int missionId, RemoveAstronautsFromMissionDTO dto)
        { 
            var missionExists = await _context.Missions
                .Where(m => m.Id == missionId)
                .AnyAsync();

            if (!missionExists)
                return NotFound($"Mission with id {missionId} not found.");

            var requestedAstronautIds = dto.Astronauts
                .Select(a => a.AstronautId)
                .Distinct()
                .ToList();

            if (requestedAstronautIds.Count == 0)
                return BadRequest("No astronaut IDs provided.");

            var relationsToRemove = await _context.MissionAstronautAssignments
                .Where(ma => ma.MissionId == missionId && requestedAstronautIds.Contains(ma.AstronautId))
                .ToListAsync();
            if (relationsToRemove.Count == 0)
                return BadRequest("None of the provided astronaut IDs are assigned to this mission.");

            _context.MissionAstronautAssignments.RemoveRange(relationsToRemove);
            await _context.SaveChangesAsync();

            var removedAstronautIds = relationsToRemove
                .Select(r => r.AstronautId)
                .ToList();

            return Ok(new RestDTO<object>
            {
                Data = new
                {
                    MissionId = missionId,
                    RemovedAstronautIds = removedAstronautIds
                },
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Missions",
                            values: new { id = missionId },
                            protocol: Request.Scheme)!,
                        "mission",
                        "GET"),
                }
            });
        }


        // DELETE
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
