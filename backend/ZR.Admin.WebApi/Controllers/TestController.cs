using Microsoft.AspNetCore.Mvc;

namespace ZR.Admin.WebApi.Controllers
{
    [ApiController]
    [Route("api/test")]
    [Tags("TEST - Mock APIs")]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// Mock Signup - Always returns true
        /// </summary>
        [HttpPost("signup")]
        public IActionResult Signup()
        {
            return Ok(new { success = true });
        }

        /// <summary>
        /// Mock Login - Always returns true
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login()
        {
            return Ok(new { success = true });
        }

        /// <summary>
        /// Mock Payment - Always returns true
        /// </summary>
        [HttpPost("payment")]
        public IActionResult Payment()
        {
            return Ok(new { success = true });
        }
    }
}
