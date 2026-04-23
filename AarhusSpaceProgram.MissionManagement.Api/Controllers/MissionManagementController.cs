using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;
using AarhusSpaceProgram.MissionManagement.Api.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static AarhusSpaceProgram.MissionManagement.Api.DTO.MissionManagementDTOs;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Tags("Missions")]
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
        /// <summary>
        /// Get an overview of all space missions.
        /// </summary>
        /// <remarks>
        /// You can optionally filter the missions, e.g. by their target celestial body.
        /// </remarks>
        /// <param name="target"></param>
        /// <returns></returns>
        [HttpGet("overview", Name = "GetMissionOverview")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult<RestDTO<List<MissionOverviewDTO>>>> GetOverview([FromQuery] string? target = null)
        {
            var query = _context.Missions
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrEmpty(target))
            {
                var targetLower = target.ToLower();
                query = query.Where(m =>
                    m.TargetBody != null && m.TargetBody.Name.ToLower().Contains(targetLower));
            }

            var overview = await query
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
        /// <summary>
        /// Mission details
        /// </summary>
        /// <remarks>
        /// Use the mission ID to get detailed information about a specific mission, including its assigned astronauts and scientists, target celestial body, launch date, and more.
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}/details", Name = "GetMissionDetails")]
        [Authorize]
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
        /// <summary>
        /// Assign astronaut to a mission
        /// </summary>
        /// <param name="missionId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Tags("Astronauts", "Missions", "Assigmenments")]
        [HttpPost("{missionId:int}/astronauts", Name = "AssignAstronautsToMission")]
        [Authorize(Roles = "Manager")]
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

            // Add new assignments only if not already assigned
            foreach (var astronautId in astronautIdsToAssign)
            {
                _context.MissionAstronautAssignments.Add(new MissionAstronautAssignment
                {
                    MissionId = missionId,
                    AstronautId = astronautId
                });
            }
            await _context.SaveChangesAsync();

            // Fetch all currently assigned astronauts for this mission
            var allAssignedAstronauts = await _context.MissionAstronautAssignments
                .Where(ma => ma.MissionId == missionId)
                .Select(ma => new {
                    ma.AstronautId,
                    ma.Astronaut.Name
                })
                .ToListAsync();

            var response = new
            {
                MissionId = missionId,
                AssignedAstronauts = allAssignedAstronauts
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

        /// <summary>
        /// Remove astronaut from a mission
        /// </summary>
        /// <param name="missionId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Tags("Astronauts", "Missions", "Assigmenments")]
        [HttpPost("{missionId:int}/astronauts/remove", Name = "RemoveAstronautsFromMission")]
        [Authorize(Roles = "Manager")]
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
        /// <summary>
        /// Assign a mission target
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Tags("Celestial Bodies", "Missions", "Assigmenments")]
        [HttpPut("target", Name = "AssignTarget")]
        [Authorize(Roles = "Manager")]
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

        /// <summary>
        /// Remove mission taget
        /// </summary>
        /// <param name="missionId"></param>
        /// <returns></returns>
        [Tags("Celestial Bodies", "Missions", "Assigmenments")]
        [HttpDelete("target", Name = "RemoveTarget")]
        [Authorize(Roles = "Manager")]
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
        /// <summary>
        /// Assign launchpad to mission
        /// </summary>
        /// <remarks>
        /// Note that a launchpad can only launch one mission a day.
        /// </remarks>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Tags("Launchpads", "Missions", "Assigmenments")]
        [HttpPut("launchpad", Name = "AssignLaunchpad")]
        [Authorize(Roles = "Manager")]
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

            if (mission.LaunchDate.HasValue)
            {
                var conflictingMission = await _context.Missions
                    .Where(m => m.Id != mission.Id
                        && m.LaunchpadId == dto.LaunchpadId
                        && m.LaunchDate == mission.LaunchDate)
                    .FirstOrDefaultAsync();

                if (conflictingMission != null)
                {
                    return BadRequest($"Launchpad with id {dto.LaunchpadId} is already assigned to mission '{conflictingMission.Name}' (id: {conflictingMission.Id}) on {mission.LaunchDate.Value:d}.");
                }
            }

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

        /// <summary>
        /// Remove launchpad from mission
        /// </summary>
        /// <param name="missionId"></param>
        /// <returns></returns>
        [Tags("Launchpads", "Missions", "Assigmenments")]
        [HttpDelete("launchpad", Name = "RemoveLaunchpad")]
        [Authorize(Roles = "Manager")]
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
        /// <summary>
        /// Assign mission manager
        /// </summary>
        /// <remarks>
        /// Note that a mission can only have one manager assigned.
        /// </remarks>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Tags("Managers", "Missions", "Assigmenments")]
        [HttpPut("manager", Name = "AssignManager")]
        [Authorize(Roles = "Manager")]
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

        /// <summary>
        /// Remove manager from mission
        /// </summary>
        /// <param name="missionId"></param>
        /// <returns></returns>
        [Tags("Managers", "Missions", "Assigmenments")]
        [HttpDelete("manager", Name = "RemoveManager")]
        [Authorize(Roles = "Manager")]
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
        /// <summary>
        /// Assign rocket to mission
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Tags("Rockets", "Missions", "Assigmenments")]
        [HttpPut("rocket", Name = "AssignRocket")]
        [Authorize(Roles = "Manager")]
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

        /// <summary>
        /// Remove rocket from mission
        /// </summary>
        /// <param name="missionId"></param>
        /// <returns></returns>
        [Tags("Rockets", "Missions", "Assigmenments")]
        [HttpDelete("rocket", Name = "RemoveRocket")]
        [Authorize(Roles = "Manager")]
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
        /// <summary>
        /// Assign scientist to a mission
        /// </summary>
        /// <param name="missionId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Tags("Scientists", "Missions", "Assigmenments")]
        [HttpPost("{missionId:int}/scientists", Name = "AssignScientistsToMission")]
        [Authorize(Roles = "Manager")]
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

            // Fetch all currently assigned scientists for this mission
            var allAssignedScientists = await _context.MissionScientistAssignments
                .Where(ms => ms.MissionId == missionId)
                .Select(ms => new
                {
                    ms.ScientistId,
                    ms.Scientist.Name
                })
                .ToListAsync();

            var response = new
            {
                MissionId = missionId,
                AssignedScientists = allAssignedScientists
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

        /// <summary>
        /// Remove scientist from a mission
        /// </summary>
        /// <param name="missionId"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Tags("Scientists", "Missions", "Assigmenments")]
        [HttpPost("{missionId:int}/scietists/remove", Name = "RemoveScientistsFromMission")]
        [Authorize(Roles = "Manager")]
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
