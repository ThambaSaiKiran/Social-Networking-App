using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SocialNetwork.API.Data;
using SocialNetwork.API.DTOs.Auth;
using SocialNetwork.API.Models;

namespace SocialNetwork.API.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            bool exists = await _db.Users.AnyAsync(u =>
                u.Username == dto.Username || u.Email == dto.Email);
            if (exists) return null;

            var user = new User
            {
                Username = dto.Username.ToLower(),
                Email = dto.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Bio = dto.Bio,
                Location = dto.Location,
                ProfilePictureUrl = $"https://api.dicebear.com/7.x/avataaars/svg?seed={dto.Username}"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Assign interests
            foreach (var interestName in dto.Interests)
            {
                var interest = await _db.Interests.FirstOrDefaultAsync(i => i.Name == interestName);
                if (interest != null)
                {
                    _db.UserInterests.Add(new UserInterest { UserId = user.Id, InterestId = interest.Id });
                }
            }

            // Assign skills
            foreach (var skillName in dto.Skills)
            {
                var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Name == skillName);
                if (skill != null)
                {
                    _db.UserSkills.Add(new UserSkill { UserId = user.Id, SkillId = skill.Id, Level = "Intermediate" });
                }
            }

            await _db.SaveChangesAsync();

            return GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u =>
                u.Username == dto.UsernameOrEmail.ToLower() ||
                u.Email == dto.UsernameOrEmail.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return null;

            return GenerateAuthResponse(user);
        }

        private AuthResponseDto GenerateAuthResponse(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:ExpiryInDays"] ?? "7"));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("fullName", user.FullName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds);

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                ExpiresAt = expiry
            };
        }
    }
}
