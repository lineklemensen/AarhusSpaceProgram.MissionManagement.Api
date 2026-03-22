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

        [HttpGet(Name = "GetMissions")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<MissionSimpleListItemDTO[]>> Get()
        {
            var query = _context.Missions
                .AsNoTracking()
                .Select(m => new MissionSimpleListItemDTO
                {
                    Id = m.Id,
                    Name = m.Name,
                    Status = m.Status.ToString()
                });

            return new RestDTO<MissionSimpleListItemDTO[]>
            {
                Data = await query.ToArrayAsync(),
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
