using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RocketsController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<RocketsController> _logger;

        public RocketsController(
            MissionManagementDbContext context,
            ILogger<RocketsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet(Name = "GetRockets")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<RocketListItemDTO[]>> Get()
        {
            var query = _context.Rockets
                .AsNoTracking()
                .Select(r => new RocketListItemDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    PayloadCapacityKg = r.PayloadCapacityKg,
                    CrewCapacity = r.CrewCapacity,
                    NumberOfStages = r.NumberOfStages,
                    FuelCapacityKg = r.FuelCapacityKg,
                    WeightKg = r.WeightKg
                });

            return new RestDTO<RocketListItemDTO[]>
            {
                Data = await query.ToArrayAsync(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            null,
                            "Rockets",
                            null,
                            Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }

        [HttpGet("{id:int}", Name = "GetRocketById")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult<RestDTO<RocketListItemDTO>>> GetById(int id)
        {
            var rocket = await _context.Rockets
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new RocketListItemDTO
                {
                    Id = r.Id,
                    Name = r.Name,
                    PayloadCapacityKg = r.PayloadCapacityKg,
                    CrewCapacity = r.CrewCapacity,
                    NumberOfStages = r.NumberOfStages,
                    FuelCapacityKg = r.FuelCapacityKg,
                    WeightKg = r.WeightKg
                })
                .FirstOrDefaultAsync();

            if (rocket == null)
                return NotFound();

            return Ok(new RestDTO<RocketListItemDTO>
            {
                Data = rocket,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller:"Rockets",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        [HttpPatch("{id:int}", Name = "UpdateRocket")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult> Patch(int id, UpdateRocketDTO dto)
        {
            var rocket = await _context.Rockets
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();

            if (rocket == null)
                return NotFound();

            if (dto.Name != null)
                rocket.Name = dto.Name;

            if (dto.PayloadCapacityKg != null)
            {
                if (dto.PayloadCapacityKg < 0)
                    return BadRequest("Payload capacity cannot be negative.");
                rocket.PayloadCapacityKg = dto.PayloadCapacityKg.Value;
            }

            if (dto.CrewCapacity != null)
            {
                if (dto.CrewCapacity < 0)
                    return BadRequest("Crew capacity cannot be negative.");
                rocket.CrewCapacity = dto.CrewCapacity.Value;
            }

            if (dto.NumberOfStages != null)
            {
                if (dto.NumberOfStages < 0)
                    return BadRequest("Number of stages cannot be negative.");
                rocket.NumberOfStages = dto.NumberOfStages.Value;
            }

            if (dto.FuelCapacityKg != null)
            {
                if (dto.FuelCapacityKg < 0)
                    return BadRequest("Fuel capacity cannot be negative.");
                rocket.FuelCapacityKg = dto.FuelCapacityKg.Value;
            }

            if (dto.WeightKg != null)
            {
                if (dto.WeightKg < 0)
                    return BadRequest("Weight cannot be negative.");
                rocket.WeightKg = dto.WeightKg.Value;
            }

            await _context.SaveChangesAsync();

            var result = new RocketListItemDTO
            {
                Id = rocket.Id,
                Name = rocket.Name,
                PayloadCapacityKg = rocket.PayloadCapacityKg,
                CrewCapacity = rocket.CrewCapacity,
                NumberOfStages = rocket.NumberOfStages,
                FuelCapacityKg = rocket.FuelCapacityKg,
                WeightKg = rocket.WeightKg
            };

            return Ok(new RestDTO<RocketListItemDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller:"Rockets",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }
    }
}
