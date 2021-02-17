using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SongUploadAPI.Domain;
using SongUploadAPI.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using SongUploadAPI.DTOs;

namespace SongUploadAPI.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;

        public IdentityService(UserManager<ApplicationUser> userManager, JwtSettings jwtSettings)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings;
        }

        public async Task<Result<Token>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return new Error("User does not exist");

            var userHasValidPassword = await _userManager.CheckPasswordAsync(user, password);

            if (userHasValidPassword == false) return new Error("user and/or password is incorrect");

            return GenerateAndWriteTokenForUser(user);
        }

        public async Task<Result<Token>> RegisterAsync(string email, string password)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null) return new Error("User with this email address already exists");

            var newUser = new ApplicationUser()
            {
                Email = email,
                UserName = email,
                Songs = new List<SongDto>()
            };

            var createdUser = await _userManager.CreateAsync(newUser, password);

            if (createdUser.Succeeded == false)
                return new Error(createdUser.Errors.SelectMany(err => err.Description)
                    .ToString());

            return GenerateAndWriteTokenForUser(newUser);
        }

        private Token GenerateAndWriteTokenForUser(ApplicationUser newUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims: new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, newUser.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, newUser.Email),
                    new Claim(_jwtSettings.UserIdClaimName, newUser.Id),
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new Token(tokenHandler.WriteToken(token));
        }
    }
}
