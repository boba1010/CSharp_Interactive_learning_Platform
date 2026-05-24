using CSharp_Interactive_Learning_App.API.DbContexts;
using CSharp_Interactive_Learning_App.API.DTOs.UserDTOs;
using CSharp_Interactive_Learning_App.API.Models;
using CSharp_Interactive_Learning_App.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace CSharp_Interactive_Learning_App.API.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController(AppDbContext dbContext, AuthService authService) : ControllerBase
    {
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException();
            return userId;
        }

        [Authorize]
        [HttpGet("auth/verify")]
        public async Task<IActionResult> VerifyUser()
        {
            Console.WriteLine(User.Identity?.IsAuthenticated);
            Console.WriteLine(User.Identity?.AuthenticationType);
            int userId = GetUserId();

            User? user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return BadRequest("The user was not found.");
            return Ok();
        }

        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return BadRequest("The user was not found.");

            var salt = user.Salt;

            if (!authService.VerifyPassword(request.Password, user.Password, salt))
                return Unauthorized("Wrong password.");

            var token = authService.GenerateJwtToken(user.Id, user.Email, "student");

            var newRefreshToken = new RefreshToken()
            {
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsRevoked = false,
                UserId = user.Id,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            };

            var userDTO = new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                TotalXp = user.TotalXp,
                Username = user.Username,
                CurrentLevel = user.CurrentLevel,
                UnlockedLessonIds = user.UnlockedLessonIds,
                CompletedLessonIds = user.CompletedLessonIds
            };

            return Ok(new UserLoginResponse { Token = token, User = userDTO, RefreshToken = newRefreshToken.Token });
        }

        [HttpPost("auth/signup")]
        public async Task<IActionResult> Signup([FromBody] UserSignupRequest request)
        {
            if (dbContext.Users.Any(u => u.Email == request.Email || u.Username == request.Username))
                return BadRequest("This Email address or username is already used.");

            if (request.Username.Length < 8)
                return BadRequest("The username should be eight characters or more.");

            var salt = authService.GenerateSalt();
            var hashPassword = authService.HashPassword(request.Password, salt);

            var user = new User { Email = request.Email, Password = hashPassword, Salt = salt, Username = request.Username, FullName = request.FullName };

            var newRefreshToken = new RefreshToken()
            {
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsRevoked = false,
                UserId = user.Id,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            };

            dbContext.RefreshTokens.Add(newRefreshToken);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var userToken = dbContext.Users.FirstOrDefault(u => u.Email == request.Email);
            var token = authService.GenerateJwtToken(userToken.Id, request.Email, "student");

            var userDTO = new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                TotalXp = user.TotalXp,
                Username = user.Username,
                CurrentLevel = user.CurrentLevel,
                UnlockedLessonIds = user.UnlockedLessonIds,
                CompletedLessonIds = user.CompletedLessonIds
            };

            return Ok(new UserSignupResponse { User = userDTO, Token = token, RefreshToken = newRefreshToken.Token });
        }

        [HttpPost("auth/refresh")]
        public async Task<IActionResult> RefreshSession([FromBody] RefreshTokenRequest request)
        {
            var refreshToken = dbContext.RefreshTokens.FirstOrDefault(x => x.Token == x.Token);
            if (refreshToken == null)
                return Unauthorized();

            if (refreshToken.ExpiresAt <= DateTime.UtcNow
                || refreshToken.IsRevoked)
                return Unauthorized();

            var user = dbContext.Users.FirstOrDefault(u => u.Id == refreshToken.UserId);
            if (user == null)
                return Unauthorized();

            var accessToken = authService.GenerateJwtToken(user.Id, user.Email, "student");
            var newRefreshToken = new RefreshToken()
            {
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsRevoked = false,
                UserId = user.Id,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            };

            refreshToken.IsRevoked = true;
            dbContext.RefreshTokens.Update(refreshToken);
            dbContext.RefreshTokens.Add(newRefreshToken);
            await dbContext.SaveChangesAsync();

            return Ok(new RefreshTokenResponse()
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken.Token
            });
        }
    }
}
