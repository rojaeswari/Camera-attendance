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
            try
            {
                var attendance = await _context.Attendance
                    .OrderByDescending(a => a.AttendanceTime)
                    .Select(a => new
                    {
                        a.AttendanceId,
                        a.UserId,
                        a.UserName,
                        a.AttendanceTime,
                        a.ImagePath
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = attendance
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


        // POST: api/Attendance
        [HttpPost]
        public async Task<IActionResult> MarkAttendance(
            [FromBody] AttendanceRequest request)
        {
            try
            {
                // Check User
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

                // Current date
                var today = DateTime.Now.Date;
                var tomorrow = today.AddDays(1);

                // Check already marked today
                var alreadyMarked = await _context.Attendance
                    .AnyAsync(a =>
                        a.UserId == request.UserId &&
                        a.AttendanceTime >= today &&
                        a.AttendanceTime < tomorrow);

                if (alreadyMarked)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Attendance already marked for today"
                    });
                }

                // Create Attendance
                var attendance = new AttendanceModel
                {
                    UserId = user.Id,
                    UserName = user.Name,
                    AttendanceTime = DateTime.Now,
                    ImagePath = request.ImagePath
                };

                _context.Attendance.Add(attendance);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Attendance marked successfully",
                    data = new
                    {
                        attendance.AttendanceId,
                        attendance.UserId,
                        attendance.UserName,
                        attendance.AttendanceTime,
                        attendance.ImagePath
                    }
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


    // Request Model
    public class AttendanceRequest
    {
        public int UserId { get; set; }

        public string? ImagePath { get; set; }
    }
}