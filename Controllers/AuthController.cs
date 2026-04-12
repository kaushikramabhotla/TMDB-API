using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TMDB_API.Models;
using TMDB_API.Repository;

namespace TMDB_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private TmdbContext _context;

        public AuthController(TmdbContext context)
        {
            _context = context;
        }
        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] string idToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

            var googleId = payload.Subject;
            var email = payload.Email;
            var name = payload.Name;


            // Check User
            var user = await _context
                .Users
                .FirstOrDefaultAsync(x => x.Provider == idToken);

            if(user == null)
            {
                user = new User
                {
                    Name = name,
                    Email = email,
                    ProviderUserId = googleId,
                    Provider = "Google"
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            //Generate JWT Token
            var token = GenerateJwtToken(user);

            return Ok(token);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SECRET_KEY_EXTREME_SIZE_HIGH_SECURITY_KEYS"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken
                (
                    issuer : "TMDB-API",
                    audience: "TMDB-API",
                    claims : claims,
                    expires : DateTime.UtcNow.AddDays(1),
                    signingCredentials : creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
