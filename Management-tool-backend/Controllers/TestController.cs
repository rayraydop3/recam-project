using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using RecamNewBackend.Models;

namespace RecamNewBackend.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public TestController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("create-user")]
        public async Task<IActionResult> CreateUser()
        {
            var user = new User
            {
                UserName = "testuser",
                Email = "test@test.com"
            };

            var result = await _userManager.CreateAsync(user, "Password123!");

            if (result.Succeeded)
            {
                return Ok("User created!");
            }

            return BadRequest(result.Errors);
        }
    }
}