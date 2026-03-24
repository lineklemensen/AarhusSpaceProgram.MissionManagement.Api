using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;
using AarhusSpaceProgram.MissionManagement.Api.Models.Enums;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CelestialBodiesController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<CelestialBodiesController> _logger;

        public CelestialBodiesController(
            MissionManagementDbContext context,
            ILogger<CelestialBodiesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // CREATE
        [HttpPost(Name = "CreateCelestialBody")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<CelestialBodyListItemDTO>>> Post(CreateCelestialBodyDTO dto)
        {
            if (!Enum.TryParse<CelestialBodyType>(dto.BodyType, true, out var bodyType))
                return BadRequest($"Invalid BodyType: {dto.BodyType}");

            PlanetClass? planetClass = null;

            // Enforce that PlanetClass is only provided for BodyType "Planet"
            if (bodyType == CelestialBodyType.Planet)
            {
                if (string.IsNullOrWhiteSpace(dto.PlanetClass))
                    return BadRequest("PlanetClass is required for a planet.");

                if (!Enum.TryParse<PlanetClass>(dto.PlanetClass, true, out var parsedPlanetClass))
                    return BadRequest($"Invalid PlanetClass: {dto.PlanetClass}");

                planetClass = parsedPlanetClass;
            }
            else if (bodyType == CelestialBodyType.Moon)
            {
                if (!string.IsNullOrWhiteSpace(dto.PlanetClass))
                    return BadRequest("PlanetClass should not be provided for a moon.");
            }

            var body = new CelestialBody
            {
                Name = dto.Name,
                BodyType = bodyType,
                PlanetClass = planetClass,
                DistanceValueToParentAU = dto.DistanceValueToParentAU,
                ParentId = dto.ParentId
            };

            _context.CelestialBodies.Add(body);
            await _context.SaveChangesAsync();

            // Load parent name for the response if ParentId is provided
            if (body.ParentId != null)
            {
                await _context.Entry(body)
                    .Reference(b => b.Parent)
                    .LoadAsync();
            }

            var result = new CelestialBodyListItemDTO
            {
                Id = body.Id,
                Name = body.Name,
                BodyType = body.BodyType.ToString(),
                PlanetClass = body.PlanetClass != null ? body.PlanetClass.ToString() : null,
                DistanceValueToParentAU = body.DistanceValueToParentAU,
                ParentName = body.Parent != null ? body.Parent.Name : null
            };

            return Created(
                Url.Action(
                    action: nameof(GetById),
                    controller: "CelestialBodies",
                    values: new { id = body.Id },
                    protocol: Request.Scheme)!,

                new RestDTO<CelestialBodyListItemDTO>
                {
                    Data = result,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(
                            Url.Action(
                                action: nameof(GetById),
                                controller: "CelestialBodies",
                                values: new { id = body.Id },
                                protocol: Request.Scheme)!,
                            "self",
                            "GET"),

                        new LinkDTO(
                            Url.Action(
                                action: nameof(Get),
                                controller: "CelestialBodies",
                                values: null,
                                protocol: Request.Scheme)!,
                            "collection",
                            "GET"),
                    }
                });
        }

        // READ
        [HttpGet(Name = "GetCelestialBodies")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<CelestialBodyListItemDTO[]>> Get()
        {
            var body = await _context.CelestialBodies
                .AsNoTracking()
                .Select(cb => new CelestialBodyListItemDTO
                {
                    Id = cb.Id,
                    Name = cb.Name,
                    BodyType = cb.BodyType.ToString(),
                    PlanetClass = cb.PlanetClass != null ? cb.PlanetClass.ToString() : null,
                    DistanceValueToParentAU = cb.DistanceValueToParentAU,
                    ParentName = cb.Parent != null ? cb.Parent.Name : null
                })
                .ToArrayAsync();

            return new RestDTO<CelestialBodyListItemDTO[]>
            {
                Data = body,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(Get),
                            controller: "CelestialBodies",
                            values: null,
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }

        [HttpGet("{id:int}", Name = "GetCelestialBodyById")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult<RestDTO<CelestialBodyListItemDTO>>> GetById(int id)
        {
            var body = await _context.CelestialBodies
                .AsNoTracking()
                .Where(cb => cb.Id == id)
                .Select(cb => new CelestialBodyListItemDTO
                {
                    Id = cb.Id,
                    Name = cb.Name,
                    BodyType = cb.BodyType.ToString(),
                    PlanetClass = cb.PlanetClass != null ? cb.PlanetClass.ToString() : null,
                    DistanceValueToParentAU = cb.DistanceValueToParentAU,
                    ParentName = cb.Parent != null ? cb.Parent.Name : null
                })
                .FirstOrDefaultAsync();

            if (body == null)
                return NotFound();

            return Ok(new RestDTO<CelestialBodyListItemDTO>
            {
                Data = body,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "CelestialBodies",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        // UPDATE
        [HttpPatch("{id:int}", Name = "UpdateCelestialBody")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<CelestialBodyListItemDTO>>> Patch(int id, UpdateCelestialBodyDTO dto)
        {
            var body = await _context.CelestialBodies
                .Where(cb => cb.Id == id)
                .FirstOrDefaultAsync();

            if (body == null)
                return NotFound();

            if (dto.Name != null)
                body.Name = dto.Name;

            if (dto.PlanetClass != null)
            {
                if (body.BodyType == CelestialBodyType.Moon)
                    return BadRequest("A moon cannot have a PlanetClass.");

                if (!Enum.TryParse<PlanetClass>(dto.PlanetClass, true, out var planetClass))
                    return BadRequest($"Invalid PlanetClass: {dto.PlanetClass}");

                body.PlanetClass = planetClass;
            }

            if (dto.ParentId != null)
            {
                if (dto.ParentId == id)
                    return BadRequest("A celestial body cannot be its own parent.");

                var parentExists = await _context.CelestialBodies.AnyAsync(cb => cb.Id == dto.ParentId);

                if (!parentExists)
                    return BadRequest($"Parent with Id {dto.ParentId} does not exist.");

                body.ParentId = dto.ParentId;
            }

            await _context.SaveChangesAsync();

            var result = new CelestialBodyListItemDTO
            {
                Id = body.Id,
                Name = body.Name,
                BodyType = body.BodyType.ToString(),
                // Safely handle null when formatting the PlanetClass property for the response check:
                PlanetClass = body.PlanetClass != null ? body.PlanetClass.ToString() : null,
                ParentName = body.Parent != null ? body.Parent.Name : null,
            };

            return Ok(new RestDTO<CelestialBodyListItemDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "CelestialBodies",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }
    }
}
