using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;
using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AarhusSpaceProgram.MissionManagement.Api.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly MissionManagementDbContext _context;

        private readonly ILogger<AccountController> _logger;

        private readonly IConfiguration _configuration;

        private readonly UserManager<AspUser> _userManager;

        private readonly SignInManager<AspUser> _signInManager;

        public AccountController(
            MissionManagementDbContext context,
            ILogger<AccountController> logger,
            IConfiguration configuration,
            UserManager<AspUser> userManager,
            SignInManager<AspUser> signInManager)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        // [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<ActionResult> Register(RegisterDTO input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newUser = new AspUser();

                    newUser.UserName = input.UserName;
                    newUser.Email = input.Email;

                    var result = await _userManager.CreateAsync(
                        newUser, input.Password);

                    if (result.Succeeded)
                    {
                        return StatusCode(201,
                            $"User '{newUser.UserName}' has been created");
                    }
                    else
                        throw new Exception(
                            string.Format("Error: {0}", string.Join(" ",
                                result.Errors.Select(e => e.Description))));
                }
                else
                {
                    var details = new ValidationProblemDetails(ModelState);

                    details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    
                    return new BadRequestObjectResult(details);
                }
            }
            catch (Exception e)
            {
                var exceptionDetails = new ProblemDetails();

                exceptionDetails.Detail = e.Message;
                exceptionDetails.Status = StatusCodes.Status500InternalServerError;
                exceptionDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";

                return StatusCode(
                    StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        [HttpPost]
        // [ResponseCache(CacheProfileName = "NoCache")]
        public async Task<ActionResult> Login()
        {
            throw new NotImplementedException();
        }
    }
}
