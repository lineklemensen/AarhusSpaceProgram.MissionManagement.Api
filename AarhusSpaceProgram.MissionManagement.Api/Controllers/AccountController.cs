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
                        dynamic staff = null;

                        switch (input.Role)
                        {
                            case "Astronaut":
                                staff = await _context.Astronauts
                                    .FirstOrDefaultAsync(a => a.Id == input.StaffId);
                                newUser.Astronaut = staff;
                                break;

                            case "Scientist":
                                staff = await _context.Scientists
                                    .FirstOrDefaultAsync(s => s.Id == input.StaffId);
                                newUser.Scientist = staff;
                                break;

                            case "Manager":
                                staff = await _context.Managers
                                    .FirstOrDefaultAsync(m => m.Id == input.StaffId);
                                newUser.Manager = staff;
                                break;

                            default:
                                throw new Exception("Invalid role specified");
                        }

                        if (staff == null)
                            throw new Exception(
                                $"No {input.Role} found with ID {input.StaffId}.");

                        staff.UserId = newUser.Id;
                        staff.User = newUser;

                        await _userManager.AddToRoleAsync(newUser, input.Role);

                        await _context.SaveChangesAsync();

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
        public async Task<ActionResult> Login(LoginDTO input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByNameAsync(input.UserName);

                    if (user == null || !await _userManager.CheckPasswordAsync(
                            user, input.Password))
                        throw new Exception("Invalid login attempt");
                    else
                    {
                        var signigCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(
                                System.Text.Encoding.UTF8.GetBytes(
                                    _configuration["Jwt:SigningKey"])),
                            SecurityAlgorithms.HmacSha256);

                        var claims = new List<Claim>();
                        claims.Add(new Claim(
                            ClaimTypes.Name, user.UserName));

                        var jwtObject = new JwtSecurityToken(
                            issuer: _configuration["Jwt:Issuer"],
                            audience: _configuration["Jwt:Audience"],
                            claims: claims,
                            expires: DateTime.Now.AddSeconds(300),
                            signingCredentials: signigCredentials);

                        var jwtString = new JwtSecurityTokenHandler()
                            .WriteToken(jwtObject);

                        return StatusCode(
                            StatusCodes.Status200OK, jwtString);
                    }
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
                exceptionDetails.Status = StatusCodes.Status401Unauthorized;
                exceptionDetails.Type = "https://tools.ietf.org/html/rfc7235#section-6.6.1";

                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    exceptionDetails);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok($"User with ID {id} has been deleted.");
        }
    }
}
