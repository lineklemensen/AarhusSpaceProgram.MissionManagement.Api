using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;


namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ScientistsController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<ScientistsController> _logger;

        public ScientistsController(
            MissionManagementDbContext context,
            ILogger<ScientistsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // CREATE
        /// <summary>
        /// Create scientist
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost(Name = "CreateScientist")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<ScientistListItemDTO>>> Post(CreateScientistDTO dto)
        {
            var scientist = new Scientist
            {
                Name = dto.Name,
                Title = dto.Title,
                Specialty = dto.Specialty,
                HireDate = dto.HireDate
            };

            _context.Scientists.Add(scientist);
            await _context.SaveChangesAsync();

            var result = new ScientistListItemDTO
            {
                Id = scientist.Id,
                Name = scientist.Name,
                Title = scientist.Title,
                Specialty = scientist.Specialty,
                HireDate = scientist.HireDate
            };

            return Created(
                Url.Action(
                    action: nameof(GetById),
                    controller: "Scientists",
                    values: new { id = scientist.Id },
                    protocol: Request.Scheme)!,

                new RestDTO<ScientistListItemDTO>
                {
                    Data = result,
                    Links = new List<LinkDTO>
                    {
                        new LinkDTO(
                            Url.Action(
                                action: nameof(GetById),
                                controller: "Scientists",
                                values: new { id = scientist.Id },
                                protocol: Request.Scheme)!,
                            "self",
                            "GET"),

                        new LinkDTO(
                            Url.Action(
                                action: nameof(Get),
                                controller: "Scientists",
                                values: null,
                                protocol: Request.Scheme)!,
                            "collection",
                            "GET"),
                    }
                });
        }

        // READ
        /// <summary>
        /// List all scientists
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "GetScientists")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<ScientistListItemDTO[]>> Get()
        {
            var scientist = _context.Scientists
                .AsNoTracking()
                .Select(s => new ScientistListItemDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Title = s.Title,
                    Specialty = s.Specialty,
                    HireDate = s.HireDate
                });

            return new RestDTO<ScientistListItemDTO[]>
            {
                Data = await scientist.ToArrayAsync(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(Get),
                            controller: "Scientists",
                            values: null,
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }

        /// <summary>
        /// Get scientist by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetScientistById")]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult<RestDTO<ScientistListItemDTO>>> GetById(string id)
        {
            var scientist = await _context.Scientists
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new ScientistListItemDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Title = s.Title,
                    Specialty = s.Specialty,
                    HireDate = s.HireDate
                })
                .FirstOrDefaultAsync();

            if (scientist == null)
                return NotFound();

            return Ok(new RestDTO<ScientistListItemDTO>
            {
                Data = scientist,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Scientists",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        // UPDATE
        /// <summary>
        /// Update scientist info
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPatch("{id}", Name = "UpdateScientist")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult> Patch(string id, UpdateScientistDTO dto)
        {
            var scientist = await _context.Scientists
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync();

            if (scientist == null)
                return NotFound();

            if (dto.Name != null)
                scientist.Name = dto.Name;

            if (dto.Title != null)
                scientist.Title = dto.Title;

            if (dto.Specialty != null)
                scientist.Specialty = dto.Specialty;

            await _context.SaveChangesAsync();

            var result = new ScientistListItemDTO
            {
                Id = scientist.Id,
                Name = scientist.Name,
                Title = scientist.Title,
                Specialty = scientist.Specialty,
                HireDate = scientist.HireDate
            };

            return Ok(new RestDTO<ScientistListItemDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Scientists",
                            values: new { id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        // DELETE
        /// <summary>
        /// Delete scientist
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}", Name = "DeleteScientist")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult> Delete(string id)
        {
            var scientist = await _context.Scientists
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync();

            if (scientist == null)
                return NotFound();

            _context.Scientists.Remove(scientist);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
