using CameraAttendance.Data;
using CameraAttendance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CameraAttendance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AttendanceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Attendance
        [HttpGet]
        public async Task<IActionResult> GetAttendance()
        {
            var attendance = await _context.Attendance
                .Include(a => a.User)
                .OrderByDescending(a => a.AttendanceDate)
                .ThenByDescending(a => a.AttendanceTime)
                .Select(a => new
                {
                    a.Id,
                    a.UserId,
                    UserName = a.User!.Name,
                    a.AttendanceDate,
                    a.AttendanceTime,
                    a.CameraName,
                    a.Status,
                    a.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = attendance
            });
        }


        // POST: api/Attendance
        [HttpPost]
        public async Task<IActionResult> MarkAttendance(
            [FromBody] AttendanceRequest request)
        {
            try
            {
                // Check user
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == request.UserId);

                if (user == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "User not found"
                    });
                }

                var today = DateTime.UtcNow.Date;

                // Check already marked today
                var alreadyMarked = await _context.Attendance
                    .AnyAsync(a =>
                        a.UserId == request.UserId &&
                        a.AttendanceDate == today);

                if (alreadyMarked)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Attendance already marked for today"
                    });
                }

                var attendance = new AttendanceModel
                {
                    UserId = request.UserId,
                    AttendanceDate = today,
                    AttendanceTime = DateTime.UtcNow.TimeOfDay,
                    CameraName = request.CameraName,
                    Status = "Present",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Attendance.Add(attendance);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Attendance marked successfully",
                    data = attendance.Id
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


    public class AttendanceRequest
    {
        public int UserId { get; set; }

        public string? CameraName { get; set; }
    }
}