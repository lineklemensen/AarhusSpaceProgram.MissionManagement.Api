using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AarhusSpaceProgram.MissionManagement.Api.DTO;
using AarhusSpaceProgram.MissionManagement.Api.Models;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
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
    }
}
