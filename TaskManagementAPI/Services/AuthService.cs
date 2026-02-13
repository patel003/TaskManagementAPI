using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        public AuthService(ApplicationDbContext Context)
        {
            _context = Context;
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
    }
}
