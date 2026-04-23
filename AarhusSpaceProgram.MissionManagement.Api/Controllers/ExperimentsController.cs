using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ExperimentsController : Controller
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
        public async Task<ActionResult<RestDTO<CreateExperimentDTO>>> Post(CreateExperimentDTO dto)
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
                                    protocol: Request.Scheme)!,
                                "collection",
                                "GET")
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
        public async Task<ActionResult<RestDTO<List<ExperimentListItemDTO>>>> Get()
        {
            var experiments = await _context.Experiments
                .AsNoTracking()
                .Select(e => new ExperimentListItemDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    CreationDate = e.CreationDate
                });
        }


        [HttpGet("{id}", Name = "GetExperimentById")]
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

    }
}
