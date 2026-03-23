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

        //Overview
        [HttpGet("overview", Name = "GetMissionOverview")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult<RestDTO<List<MissionOverviewDTO>>>> GetOverview()
        {
            var overview = await _context.Missions
                .AsNoTracking()
                .OrderBy(m => m.Status)
                .Select(m => new MissionOverviewDTO
                {
                    Id = m.Id,
                    Name = m.Name,
                    ManagerName = m.Manager != null ? $"{m.Manager}" : "Unassigned",
                    Status = m.Status.ToString(),
                    LaunchDate = m.LaunchDate,
                    RocketModel = m.Rocket != null ? m.Rocket.Name : "Unassigned",
                    LaunchpadLocation = m.Launchpad != null ? m.Launchpad.Location : "Unassigned",
                    TargetCelestialBody = m.TargetBody != null ? m.TargetBody.Name : "Unassigned"
                })
                .ToListAsync();

            return Ok(new RestDTO<List<MissionOverviewDTO>>
            {
                Data = overview,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetOverview),
                            controller: "Missions",
                            values: null,
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        // Details
        [HttpGet("{id:int}/details", Name = "GetMissionDetails")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult<RestDTO<MissionDetailsDTO>>> GetDetails(int id)
        {
            var mission = await _context.Missions
                .AsNoTracking()
                .Where(m => m.Id == id)
                .Select(m => new MissionDetailsDTO
                {
                    Name = m.Name,
                    Status = m.Status.ToString(),
                    LaunchDate = m.LaunchDate,
                    ManagerName = m.Manager != null ? $"{m.Manager}" : "Unassigned",
                    RocketModel = m.Rocket != null ? m.Rocket.Name : "Unassigned",
                    LaunchpadLocation = m.Launchpad != null ? m.Launchpad.Location : "Unassigned",
                    TargetCelestialBody = m.TargetBody != null ? m.TargetBody.Name : "Unassigned",
                    Astronauts = m.AstronautAssignments.Select(a => new AstronautListItemDTO
                    {
                        Id = a.Astronaut.Id,
                        Name = a.Astronaut.Name,
                        Rank = a.Astronaut.Rank.ToString(),
                        Paygrade = a.Astronaut.Paygrade,
                        HoursInSimulation = a.Astronaut.HoursInSimulation,
                        HoursInSpace = a.Astronaut.HoursInSpace
                    }).ToList(),
                    Scientists = m.ScientistAssignments.Select(s => new ScientistListItemDTO
                    {
                        Id = s.Scientist.Id,
                        Name = s.Scientist.Name,
                        Title = s.Scientist.Title,
                        Specialty = s.Scientist.Specialty,
                        HireDate = s.Scientist.HireDate
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (mission == null)
                return NotFound($"Mission with id {id} not found.");

            return Ok(new RestDTO<MissionDetailsDTO>
            {
                Data = mission,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetDetails),
                            controller: "Missions",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetOverview),
                            controller: "Missions",
                            values: null,
                            protocol: Request.Scheme)!,
                        "collection",
                        "GET")
                }
            });
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

        // Celestial Body
        [HttpPut("target", Name = "AssignTarget")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<CelestialBodyAssignmentDTO>>> AssignTarget(CelestialBodyAssignmentDTO dto)
        {
            var mission = await _context.Missions
                .Where(m => m.Id == dto.MissionId)
                .FirstOrDefaultAsync();

            if (mission == null)
                return NotFound($"Mission with id {dto.MissionId} not found.");

            var target = await _context.CelestialBodies
                .Where(m => m.Id == dto.CelestialBodyId)
                .FirstOrDefaultAsync();

            if (target == null)
                return NotFound($"Celestial body with id {dto.CelestialBodyId} not found.");

            mission.CelestialBodyId = dto.CelestialBodyId;
            await _context.SaveChangesAsync();

            var result = new CelestialBodyAssignmentDTO
            {
                MissionId = mission.Id,
                CelestialBodyId = target.Id
            };

            return Ok(new RestDTO<CelestialBodyAssignmentDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: "GetById",
                            controller: "Missions",
                            values: new { id = mission.Id },
                            protocol: Request.Scheme)!,
                        "mission",
                        "GET"),
                }
            });
        }

        [HttpDelete("target", Name = "RemoveTarget")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<CelestialBodyAssignmentDTO>>> RemoveTarget(int missionId)
        {
            var mission = await _context.Missions
                .Where(m => m.Id == missionId)
                .FirstOrDefaultAsync();

            if (mission == null)
                return NotFound($"Mission with id {missionId} not found.");

            mission.CelestialBodyId = null;

            await _context.SaveChangesAsync();

            var result = new CelestialBodyAssignmentDTO
            {
                MissionId = mission.Id,
                CelestialBodyId = null
            };

            return Ok(new RestDTO<CelestialBodyAssignmentDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: "GetById",
                            controller: "Missions",
                            values: new { id = mission.Id },
                            protocol: Request.Scheme)!,
                        "mission",
                        "GET"),
                }
            });
        }

        // Launchpad
        [HttpPut("launchpad", Name = "AssignLaunchpad")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<LaunchpadAssignmentDTO>>> AssignLaunchpad(LaunchpadAssignmentDTO dto)
        {
            var mission = await _context.Missions
                .Where(m => m.Id == dto.MissionId)
                .FirstOrDefaultAsync();

            if (mission == null)
                return NotFound($"Mission with id {dto.MissionId} not found.");

            var launchpad = await _context.Launchpads
                .Where(m => m.Id == dto.LaunchpadId)
                .FirstOrDefaultAsync();

            if (launchpad == null)
                return NotFound($"Launchpad with id {dto.LaunchpadId} not found.");

            mission.LaunchpadId = dto.LaunchpadId;
            await _context.SaveChangesAsync();

            var result = new LaunchpadAssignmentDTO
            {
                MissionId = mission.Id,
                LaunchpadId = launchpad.Id
            };

            return Ok(new RestDTO<LaunchpadAssignmentDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: "GetById",
                            controller: "Missions",
                            values: new { id = mission.Id },
                            protocol: Request.Scheme)!,
                        "mission",
                        "GET"),
                }
            });
        }

        [HttpDelete("launchpad", Name = "RemoveLaunchpad")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<LaunchpadAssignmentDTO>>> RemoveLaunchpad(int missionId)
        {
            var mission = await _context.Missions
                .Where(m => m.Id == missionId)
                .FirstOrDefaultAsync();

            if (mission == null)
                return NotFound($"Mission with id {missionId} not found.");

            mission.LaunchpadId = null;

            await _context.SaveChangesAsync();

            var result = new LaunchpadAssignmentDTO
            {
                MissionId = mission.Id,
                LaunchpadId = null
            };

            return Ok(new RestDTO<LaunchpadAssignmentDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: "GetById",
                            controller: "Missions",
                            values: new { id = mission.Id },
                            protocol: Request.Scheme)!,
                        "mission",
                        "GET"),
                }
            });
        }

        // Manager
        [HttpPut("manager", Name = "AssignManager")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<ManagerAssignmentDTO>>> AssignManager(ManagerAssignmentDTO dto)
        {
            var mission = await _context.Missions
                .Where(m => m.Id == dto.MissionId)
                .FirstOrDefaultAsync();

            if (mission == null)
                return NotFound($"Mission with id {dto.MissionId} not found.");

            var manager = await _context.Managers
                .Where(m => m.Id == dto.ManagerId)
                .FirstOrDefaultAsync();

            if (manager == null)
                return NotFound($"Manager with id {dto.ManagerId} not found.");

            mission.ManagerId = dto.ManagerId;
            await _context.SaveChangesAsync();

            var result = new ManagerAssignmentDTO
            {
                MissionId = mission.Id,
                ManagerId = manager.Id
            };

            return Ok(new RestDTO<ManagerAssignmentDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: "GetById",
                            controller: "Missions",
                            values: new { id = mission.Id },
                            protocol: Request.Scheme)!,
                        "mission",
                        "GET"),
                }
            });
        }

        [HttpDelete("manager", Name = "RemoveManager")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<ManagerAssignmentDTO>>> RemoveManager(int missionId)
        {
            var mission = await _context.Missions
                .Where(m => m.Id == missionId)
                .FirstOrDefaultAsync();

            if (mission == null)
                return NotFound($"Mission with id {missionId} not found.");

            mission.ManagerId = null;

            await _context.SaveChangesAsync();

            var result = new ManagerAssignmentDTO
            {
                MissionId = mission.Id,
                ManagerId = null
            };

            return Ok(new RestDTO<ManagerAssignmentDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: "GetById",
                            controller: "Missions",
                            values: new { id = mission.Id },
                            protocol: Request.Scheme)!,
                        "mission",
                        "GET"),
                }
            });
        }

        // Rocket
        [HttpPut("rocket", Name = "AssignRocket")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<RocketAssignmentDTO>>> AssignRocket(RocketAssignmentDTO dto)
        {
            var mission = await _context.Missions
                .Where(m => m.Id == dto.MissionId)
                .FirstOrDefaultAsync();

            if (mission == null)
                return NotFound($"Mission with id {dto.MissionId} not found.");

            var rocket = await _context.Rockets
                .Where(m => m.Id == dto.RocketId)
                .FirstOrDefaultAsync();

            if (rocket == null)
                return NotFound($"Rocket with id {dto.RocketId} not found.");

            mission.RocketId = dto.RocketId;
            await _context.SaveChangesAsync();

            var result = new RocketAssignmentDTO
            {
                MissionId = mission.Id,
                RocketId = rocket.Id
            };

            return Ok(new RestDTO<RocketAssignmentDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: "GetById",
                            controller: "Missions",
                            values: new { id = mission.Id },
                            protocol: Request.Scheme)!,
                        "mission",
                        "GET"),
                }
            });
        }

        [HttpDelete("rocket", Name = "RemoveRocket")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<RocketAssignmentDTO>>> RemoveRocket(int missionId)
        {
            var mission = await _context.Missions
                .Where(m => m.Id == missionId)
                .FirstOrDefaultAsync();

            if (mission == null)
                return NotFound($"Mission with id {missionId} not found.");

            mission.RocketId = null;

            await _context.SaveChangesAsync();

            var result = new RocketAssignmentDTO
            {
                MissionId = mission.Id,
                RocketId = null
            };

            return Ok(new RestDTO<RocketAssignmentDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: "GetById",
                            controller: "Missions",
                            values: new { id = mission.Id },
                            protocol: Request.Scheme)!,
                        "mission",
                        "GET"),
                }
            });
        }

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
