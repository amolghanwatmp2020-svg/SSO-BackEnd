using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [HttpGet("public")]
        public IActionResult GetPublicData()
        {
            return Ok("This is public data, accessible by anyone test CICD.");
        }

        [HttpGet("authenticated")]
        [Authorize("RequireSuperAdminRole")] // Requires any authenticated user
        public IActionResult GetAuthenticatedData()
        {
            var userName = User.Identity?.Name ?? "Unknown";
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            _logger.LogInformation($"Authenticated user {userName} (ID: {userId}) accessed authenticated data. Roles: {string.Join(", ", roles)}");
            return Ok($"Hello {userName}! This data is for any authenticated user. Your roles: {string.Join(", ", roles)}");
        }

        [HttpGet("user-data")]
        [Authorize(Policy = "RequireUserRole")] // Policy checking for 'User', 'Admin', or 'SuperAdmin' role
        public IActionResult GetUserData()
        {
            var userName = User.Identity?.Name ?? "Unknown";
            _logger.LogInformation($"User {userName} accessed user-specific data.");
            return Ok($"This is user-level data. Welcome, {userName}!");
        }

        [HttpGet("admin-data")]
        [Authorize(Policy = "RequireAdminOrSuperAdminRole")] // Policy checking for 'Admin' or 'SuperAdmin' role
        public IActionResult GetAdminData()
        {
            var userName = User.Identity?.Name ?? "Unknown";
            _logger.LogInformation($"Admin/SuperAdmin {userName} accessed admin data.");
            return Ok($"This data is only for Admins and SuperAdmins. Welcome, {userName}!");
        }

        [HttpGet("superadmin-data")]
        [Authorize(Policy = "RequireSuperAdminRole")] // Policy checking for 'SuperAdmin' role
        public IActionResult GetSuperAdminData()
        {
            var userName = User.Identity?.Name ?? "Unknown";
            _logger.LogInformation($"SuperAdmin {userName} accessed super admin data.");
            return Ok($"This data is only for SuperAdmins. Behold, the power, {userName}!");
        }
    }
}