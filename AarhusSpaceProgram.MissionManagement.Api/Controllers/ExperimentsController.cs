using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ExperimentsController : ControllerBase
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<ExperimentsController> _logger;

        public ExperimentsController(
            MissionManagementDbContext context,
            ILogger<ExperimentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // CREATE
        [HttpPost(Name = "CreateExperiment")]
        [Authorize(Roles = "Manager, Scientist")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<ExperimentListItemDTO>>> Post(CreateExperimentDTO dto)
        {
            try
            {
                // Create experiment
                var experiment = new Experiment
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    CreationDate = dto.CreationDate
                };

                _context.Experiments.Add(experiment);
                await _context.SaveChangesAsync();

                var result = new ExperimentListItemDTO
                {
                    Id = experiment.Id,
                    Name = experiment.Name,
                    Description = experiment.Description,
                    CreationDate = experiment.CreationDate
                };

                return Created(
                    Url.Action(
                        action: nameof(GetById),
                        controller: "Experiments",
                        values: new { id = experiment.Id },
                        protocol: Request.Scheme)!,

                    new RestDTO<ExperimentListItemDTO>
                    {
                        Data = result,
                        Links = new List<LinkDTO>
                        {
                            new LinkDTO(
                                Url.Action(
                                    action: nameof(GetById),
                                    controller: "Experiments",
                                    values: new { id = experiment.Id },
                                    protocol: Request.Scheme)!,
                                "self",
                                "GET"),

                            new LinkDTO(
                                Url.Action(
                                    action: nameof(Get),
                                    controller: "Experiments",
                                    values: null,
                                    protocol: Request.Scheme)!,
                                "collection",
                                "GET"),
                        }
                    });
            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occurred while creating the experiment: {e.Message}");
            }
        }

        // READ
        [HttpGet(Name = "GetExperiments")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<RestDTO<ExperimentListItemDTO[]>> Get()
        {
            var experiment = _context.Experiments
                .AsNoTracking()
                .Select(e => new ExperimentListItemDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    CreationDate = e.CreationDate
                });

            return new RestDTO<ExperimentListItemDTO[]>
            {
                Data = await experiment.ToArrayAsync(),
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(Get),
                            controller: "Experiments",
                            values: null,
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            };
        }


        [HttpGet("{id:int}", Name = "GetExperimentById")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.Any, Duration = 60)]
        public async Task<ActionResult<RestDTO<ExperimentListItemDTO>>> GetById(int id)
        {
            var experiment = await _context.Experiments
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new ExperimentListItemDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    CreationDate = e.CreationDate
                })
                .FirstOrDefaultAsync();

            if (experiment == null)
                return NotFound();

            return Ok(new RestDTO<ExperimentListItemDTO>
            {
                Data = experiment,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Experiments",
                            values: new { id = experiment.Id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET")
                }
            });
        }

        // UPDATE
        [HttpPatch("{id:int}", Name = "UpdateExperiment")]
        [Authorize(Roles = "Manager, Scientist")]
        [ResponseCache(NoStore = true)]
        public async Task<ActionResult<RestDTO<ExperimentListItemDTO>>> Patch(int id, UpdateExperimentDTO dto)
        {
            var experiment = await _context.Experiments
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();

            if (experiment == null)
                return NotFound();

            if (dto.Name != null)
                experiment.Name = dto.Name;

            if (dto.Description != null)
                experiment.Description = dto.Description;

            await _context.SaveChangesAsync();

            var result = new ExperimentListItemDTO
            {
                Id = experiment.Id,
                Name = experiment.Name,
                Description = experiment.Description,
                CreationDate = experiment.CreationDate
            };

            return Ok(new RestDTO<ExperimentListItemDTO>
            {
                Data = result,
                Links = new List<LinkDTO>
                {
                    new LinkDTO(
                        Url.Action(
                            action: nameof(GetById),
                            controller: "Experiments",
                            values: new { id = experiment.Id },
                            protocol: Request.Scheme)!,
                        "self",
                        "GET"),
                }
            });
        }

        // DELETE
        [HttpDelete("{id:int}", Name = "DeleteExperiment")]
        [Authorize(Roles = "Manager, Scientist")]
        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> Delete(int id)
        {
            var experiment = await _context.Experiments
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();

            if (experiment == null)
                return NotFound();

            _context.Experiments.Remove(experiment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
