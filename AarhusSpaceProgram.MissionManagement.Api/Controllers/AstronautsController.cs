using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AstronautsController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<AstronautsController> _logger;

        public AstronautsController(
            MissionManagementDbContext context,
            ILogger<AstronautsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet(Name = "GetAstronauts")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<AstronautListItemDTO[]>> Get()
        {
            var query = _context.Astronauts
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
                Data = await query.ToArrayAsync(),
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

        [HttpGet("{id:int}", Name = "GetAstronautById")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult<RestDTO<AstronautListItemDTO>>> GetById(int id)
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

        [HttpPatch("{id:int}", Name = "UpdateAstronaut")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult> Patch(int id, UpdateAstronautDTO dto)
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

        [HttpDelete("{id:int}", Name = "DeleteAstronaut")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult> Delete(int id)
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
