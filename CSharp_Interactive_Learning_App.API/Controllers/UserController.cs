using CSharp_Interactive_Learning_App.API.DbContexts;
using CSharp_Interactive_Learning_App.API.DTOs.UserDTOs;
using CSharp_Interactive_Learning_App.API.Models;
using CSharp_Interactive_Learning_App.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CSharp_Interactive_Learning_App.API.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController(AppDbContext dbContext, AuthService authService) : ControllerBase
    {
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyUser([FromBody] string token)
        {
            var handler = new JwtSecurityTokenHandler();

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "CSharp_Interactive_Learning_App.API",

                ValidateAudience = true,
                ValidAudience = "CSharp_Interactive_Learning_App",

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("=3cm9d+5r+4=cd4+dc4=dde=dcc23gjjtygv"))
            };

            var principal = handler.ValidateToken(token, validationParams, out var validatedToken);

            int userId = Convert.ToInt32(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            User? user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return BadRequest("The user was not found.");
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return BadRequest("The user was not found.");

            var salt = user.Salt;

            if (!authService.VerifyPassword(request.Password, user.Password, salt))
                return Unauthorized("Wrong password.");

            var token = authService.GenerateJwtToken(user.Id, user.Email, "student");
            
            return Ok(new UserLoginResponse { Token = token, User = user });
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] UserSignupRequest request)
        {
            if (dbContext.Users.Any(u => u.Email == request.Email || u.Username == request.Username))
                return BadRequest("This Email address or username is already used.");

            if (request.Username.Length < 8)
                return BadRequest("The username should be eight characters or more.");

            var salt = authService.GenerateSalt();
            var hashPassword = authService.HashPassword(request.Password, salt);

            var user = new User { Email = request.Email, Password = hashPassword, Salt = salt, Username = request.Username, FullName = request.FullName };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var userToken = dbContext.Users.FirstOrDefault(u => u.Email == request.Email);
            var token = authService.GenerateJwtToken(userToken.Id, request.Email, "student");
            return Ok(new UserSignupResponse { User = user, Token = token });
        }
    }
}
