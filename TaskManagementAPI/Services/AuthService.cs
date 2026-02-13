using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace TaskManagementAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthService(ApplicationDbContext Context, IConfiguration configuration)
        {
            _context = Context;
            _configuration = configuration;
        }
        public async Task<UserResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // check if user already exists
            var existingUser = await _context.Users
                     .FirstOrDefaultAsync(u =>
                              u.Username.ToLower().Trim() == registerDto.Username.ToLower().Trim()
                            || u.Email.ToLower().Trim() == registerDto.Email.ToLower().Trim());

            if (existingUser != null)
            {
                return null;
            }


            //Hash the Password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            // Save the user to the database
            _context.Users.Add(user); // in memory insertion
            await _context.SaveChangesAsync(); // persist to database

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                  .FirstOrDefaultAsync(u => u.Email.ToLower().Trim() == loginDto.Email.ToLower().Trim());

            // Check if user exists and password is correct
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");

            }
            //verify the password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password");

            }

            // Generate JWT Token

            var token =  GenerateJwtToken(user);
            var expiersAt = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:DurationInMinutes"]!));

            return new LoginResponseDto
            {
                Token = token,
                User = new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                },
                Expiration = expiersAt
            };

        }
        private string GenerateJwtToken(User user)
        {
            var secretKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expiryMinutes = int.Parse(_configuration["Jwt:DurationInMinutes"]);
            // Create the signing credentials
                
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            //Define the claims to be included in the token
            var claims = new[]
            {
               new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
               new Claim(JwtRegisteredClaimNames.Email, user.Email),
               new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );
            // Return the serialized token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}




