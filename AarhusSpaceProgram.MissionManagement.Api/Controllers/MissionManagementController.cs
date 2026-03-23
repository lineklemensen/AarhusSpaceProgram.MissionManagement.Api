using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;
using AarhusSpaceProgram.MissionManagement.Api.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static AarhusSpaceProgram.MissionManagement.Api.DTO.MissionManagementDTOs;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MissionManagementController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<MissionManagementController> _logger;

        public MissionManagementController(
            MissionManagementDbContext context,
            ILogger<MissionManagementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Astronauts
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
                            action: "GetById",
                            controller: "Missions",
                            values: new { id = missionId },
                            protocol: Request.Scheme)!,
                        "mission",
                        "GET"),
                }
            });
        }

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
                            action: "GetById",
                            controller: "Missions",
                            values: new { id = missionId },
                            protocol: Request.Scheme)!,
                        "mission",
                        "GET"),
                }
            });
        }

        // Manager
        [HttpPut("manager", Name = "AssignManager")]
        [ResponseCache(NoStore = true)]
        public async Ta

        // Scientists
        [HttpPost("{missionId:int}/scientists", Name = "AssignScientistsToMission")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<object>>> AssignScientists(int missionId, AssignScientistsToMissionDTO dto)
        {
            var missionExists = await _context.Missions
                .Where(m => m.Id == missionId)
                .AnyAsync();

            if (!missionExists)
                return NotFound($"Mission with id {missionId} not found.");

            var requestedScientistIds = dto.Scientists
                .Select(s => s.ScientistId)
                .Distinct()
                .ToList();

            if (requestedScientistIds.Count == 0)
                return BadRequest("No scientist IDs provided.");

            var scientists = await _context.Scientists
                .Where(s => requestedScientistIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();

            var missingScientistIds = requestedScientistIds
                .Except(scientists)
                .ToList();

            if (missingScientistIds.Count > 0)
                return BadRequest($"The following scientist IDs do not exist: {string.Join(", ", missingScientistIds)}");

            var alreadyAssignedScientistIds = await _context.MissionScientistAssignments
                .Where(ms => ms.MissionId == missionId && requestedScientistIds.Contains(ms.ScientistId))
                .Select(ms => ms.ScientistId)
                .ToListAsync();

            var scientistIdsToAssign = requestedScientistIds
                .Except(alreadyAssignedScientistIds)
                .ToList();

            foreach (var scientistId in scientistIdsToAssign)
            {
                _context.MissionScientistAssignments.Add(new MissionScientistAssignment
                {
                    MissionId = missionId,
                    ScientistId = scientistId
                });
            }

            await _context.SaveChangesAsync();

            var response = new
            {
                MissionId = missionId,
                AssignedScientistIds = scientistIdsToAssign,
                AlreadyAssignedScientistIds = alreadyAssignedScientistIds
            };

            return Ok(new RestDTO<object>
            {
                Data = response,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: "GetById",
                            controller: "Missions",
                            values: new { id = missionId },
                            protocol: Request.Scheme)!,
                        "mission",
                        "GET"),
                }
            });
        }

        [HttpPost("{missionId:int}/scietists/remove", Name = "RemoveScientistsFromMission")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<object>>> RemoveScientists(int missionId, RemoveScientistsFromMissionDTO dto)
        {
            var missionExists = await _context.Missions
                .Where(m => m.Id == missionId)
                .AnyAsync();

            if (!missionExists)
                return NotFound($"Mission with id {missionId} not found.");

            var requestedScientistIds = dto.Scientists
                .Select(s => s.ScientistId)
                .Distinct()
                .ToList();

            if (requestedScientistIds.Count == 0)
                return BadRequest("No scientist IDs provided.");

            var relationsToRemove = await _context.MissionScientistAssignments
                .Where(ms => ms.MissionId == missionId && requestedScientistIds.Contains(ms.ScientistId))
                .ToListAsync();
            if (relationsToRemove.Count == 0)
                return BadRequest("None of the provided scientist IDs are assigned to this mission.");

            _context.MissionScientistAssignments.RemoveRange(relationsToRemove);
            await _context.SaveChangesAsync();

            var removedScientistIds = relationsToRemove
                .Select(r => r.ScientistId)
                .ToList();

            return Ok(new RestDTO<object>
            {
                Data = new
                {
                    MissionId = missionId,
                    RemovedScientistIds = removedScientistIds
                },
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: "GetById",
                            controller: "Missions",
                            values: new { id = missionId },
                            protocol: Request.Scheme)!,
                        "mission",
                        "GET"),
                }
            });
        }
    }
}
