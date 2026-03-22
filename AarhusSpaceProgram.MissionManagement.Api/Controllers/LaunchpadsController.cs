using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;
using AarhusSpaceProgram.MissionManagement.Api.Models.Enums;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LaunchpadsController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<LaunchpadsController> _logger;

        public LaunchpadsController(
            MissionManagementDbContext context,
            ILogger<LaunchpadsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet(Name = "GetLaunchpads")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<LaunchpadListItemDTO[]>> Get()
        {
            var query = _context.Launchpads
                .AsNoTracking()
                .Select(l => new LaunchpadListItemDTO
                {
                    Id = l.Id,
                    PadCode = l.PadCode,
                    Location = l.Location,
                    Status = l.Status.ToString(),
                    MaxSupportedWeightKg = l.MaxSupportedWeightKg
                });
            return new RestDTO<LaunchpadListItemDTO[]>
            {
                Data = await query.ToArrayAsync(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            null,
                            "Launchpads",
                            null,
                            Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }

        [HttpGet("{id:int}", Name = "GetLaunchpadById")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult<RestDTO<LaunchpadListItemDTO>>> GetById(int id)
        {
            var launchpad = await _context.Launchpads
                .AsNoTracking()
                .Where(l => l.Id == id)
                .Select(l => new LaunchpadListItemDTO
                {
                    Id = l.Id,
                    PadCode = l.PadCode,
                    Location = l.Location,
                    Status = l.Status.ToString(),
                    MaxSupportedWeightKg = l.MaxSupportedWeightKg
                })
                .FirstOrDefaultAsync();

            if (launchpad == null)
                return NotFound();

            return Ok(new RestDTO<LaunchpadListItemDTO>
            {
                Data = launchpad,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Launchpads",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        [HttpPatch("{id:int}", Name = "UpdateLaunchpad")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult> Patch(int id, UpdateLaunchpadDTO dto)
        {
            var launchpad = await _context.Launchpads
                .Where(lp => lp.Id == id)
                .FirstOrDefaultAsync();

            if (launchpad == null)
                return NotFound();

            if (dto.PadCode != null)
                launchpad.PadCode = dto.PadCode;

            if (dto.Location != null)
                launchpad.Location = dto.Location;

            if (dto.Status != null)
            {
                if (!Enum.TryParse<LaunchpadStatus>(dto.Status, ignoreCase: true, out var status))
                    return BadRequest($"Invalid status value: {dto.Status}");
                launchpad.Status = status;
            }

            if (dto.MaxSupportedWeightKg != null)
            {
                if (dto.MaxSupportedWeightKg < 0)
                    return BadRequest("MaxSupportedWeightKg must be non-negative.");
                launchpad.MaxSupportedWeightKg = dto.MaxSupportedWeightKg.Value;
            }

            await _context.SaveChangesAsync();

            var result = new LaunchpadListItemDTO
            {
                Id = launchpad.Id,
                PadCode = launchpad.PadCode,
                Location = launchpad.Location,
                Status = launchpad.Status.ToString(),
                MaxSupportedWeightKg = launchpad.MaxSupportedWeightKg
            };

            return Ok(new RestDTO<LaunchpadListItemDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Launchpads",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        [HttpDelete("{id:int}", Name = "DeleteLaunchpad")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult> Delete(int id)
        {
            var launchpad = await _context.Launchpads
                .Where(lp => lp.Id == id)
                .FirstOrDefaultAsync();

            if (launchpad == null)
                return NotFound();

            _context.Launchpads.Remove(launchpad);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
