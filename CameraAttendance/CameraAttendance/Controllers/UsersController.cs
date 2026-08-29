using CameraAttendance.Data;
using CameraAttendance.DTOs;
using CameraAttendance.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CameraAttendance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
         public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers() 
        {
            var users = await _context.Users
                .Select(u => new 
                { u.Id,
                  u.Name,
                  u.Email,
                  u.RoleId,
                  u.IsActive,
                  u.CreatedAt,
                  u.UpdatedAt 
                }).
                ToListAsync();
            return Ok(new 
            { success = true,
              data = users });
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(RegisterDTO model)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Email already exists"
                    });
                }

                var user = new UserModel
                {
                    Name = model.Name,
                    Email = model.Email,

                    // Temporary:
                    
                    PasswordHash = model.Password,

                    RoleId = model.RoleId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "User added successfully",
                    data = user.Id
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

    }
}
