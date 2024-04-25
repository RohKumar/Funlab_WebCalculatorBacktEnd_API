using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using WebCalculator.Models;

namespace WebCalculatorAPI.Controllers
{

    [EnableCors("AllowFrontend")]
    [Route("api/[controller]")]
    [ApiController]
    public class SignInController : ControllerBase
    {
        [HttpPost]
        public IActionResult SignIn(UserSignInRequest request)
        {
            try
            {
                // Read JSON file containing user data
                var json = System.IO.File.ReadAllText("user_data.json");
                var userData = JObject.Parse(json);

                // Validate email and password against stored user data
                var user = userData;

                if ((string)user["Email"] == request.Email && (string)user["Password"] == request.Password)
                {
                    // Successful sign-in
                    return Ok();
                }
                else
                {
                    // Unsuccessful sign-in
                    return BadRequest("Invalid email or password");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
