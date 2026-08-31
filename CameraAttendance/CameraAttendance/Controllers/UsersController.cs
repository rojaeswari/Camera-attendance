using CameraAttendance.Data;
using CameraAttendance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CameraAttendance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UsersController(
            AppDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.RoleId,
                    u.IsActive,
                    u.FaceImagePath,
                    u.CreatedAt,
                    u.UpdatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = users
            });
        }

        // POST: api/Users
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddUser(
     [FromForm] string Name,
     [FromForm] string Email,
     [FromForm] string Password,
     [FromForm] int RoleId,
     IFormFile? FaceImage)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == Email);

                if (existingUser != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Email already exists"
                    });
                }

                string? faceImagePath = null;

                // ============================
                // FACE IMAGE UPLOAD
                // ============================

                if (FaceImage != null && FaceImage.Length > 0)
                {
                    Console.WriteLine("File Name: " + FaceImage.FileName);
                    Console.WriteLine("Content Type: " + FaceImage.ContentType);
                    Console.WriteLine("File Size: " + FaceImage.Length);

                    var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "faces"
                    );

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var extension = Path.GetExtension(FaceImage.FileName);

                    var fileName = Guid.NewGuid().ToString() + extension;

                    var filePath = Path.Combine(
                        uploadsFolder,
                        fileName
                    );

                    using (var stream = new FileStream(
                        filePath,
                        FileMode.Create))
                    {
                        await FaceImage.CopyToAsync(stream);
                    }

                    faceImagePath = "/uploads/faces/" + fileName;
                }

                // ============================
                // CREATE USER
                // ============================

                var user = new UserModel
                {
                    Name = Name,
                    Email = Email,

                    PasswordHash =
                        BCrypt.Net.BCrypt.HashPassword(Password),

                    RoleId = RoleId,

                    IsActive = true,

                    FaceImagePath = faceImagePath,

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


        // DELETE: api/Users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found"
                    });
                }

                // ============================
                // DELETE FACE IMAGE
                // ============================

                if (!string.IsNullOrEmpty(user.FaceImagePath))
                {
                    var fileName = Path.GetFileName(user.FaceImagePath);

                    var filePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "faces",
                        fileName
                    );

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // ============================
                // DELETE USER FROM DATABASE
                // ============================

                _context.Users.Remove(user);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "User deleted successfully"
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


        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateUser(
    int id,
    [FromForm] string Name,
    [FromForm] string Email,
    [FromForm] string? Password,
    [FromForm] int RoleId,
    IFormFile? FaceImage)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found"
                    });
                }

                var emailExists = await _context.Users
                    .AnyAsync(u =>
                        u.Email == Email &&
                        u.Id != id);

                if (emailExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Email already exists"
                    });
                }

                // Basic details
                user.Name = Name;
                user.Email = Email;
                user.RoleId = RoleId;

                // Password - only update if entered
                if (!string.IsNullOrWhiteSpace(Password))
                {
                    user.PasswordHash =
                        BCrypt.Net.BCrypt.HashPassword(Password);
                }

                // Face Image
                if (FaceImage != null && FaceImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "faces"
                    );

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(user.FaceImagePath))
                    {
                        var oldFileName =
                            Path.GetFileName(user.FaceImagePath);

                        var oldFilePath = Path.Combine(
                            uploadsFolder,
                            oldFileName
                        );

                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Save new image
                    var extension =
                        Path.GetExtension(FaceImage.FileName);

                    var fileName =
                        Guid.NewGuid().ToString() + extension;

                    var filePath =
                        Path.Combine(uploadsFolder, fileName);

                    using (var stream =
                        new FileStream(filePath, FileMode.Create))
                    {
                        await FaceImage.CopyToAsync(stream);
                    }

                    user.FaceImagePath =
                        "/uploads/faces/" + fileName;
                }

                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "User updated successfully"
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