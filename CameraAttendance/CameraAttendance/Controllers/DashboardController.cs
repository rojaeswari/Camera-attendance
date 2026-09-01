using CameraAttendance.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CameraAttendance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Dashboard
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                // Today start
                var today = DateTime.Now.Date;

                // Tomorrow start
                var tomorrow = today.AddDays(1);

                // Total Users
                var totalUsers = await _context.Users
                    .CountAsync();

                // Today's Attendance
                var todayAttendance = await _context.Attendance
                    .CountAsync(a =>
                        a.AttendanceTime >= today &&
                        a.AttendanceTime < tomorrow
                    );

                // Present Count
                var present = await _context.Attendance
                    .CountAsync(a =>
                        a.AttendanceTime >= today &&
                        a.AttendanceTime < tomorrow
                    );

                // Active Cameras
                var activeCameras = await _context.Cameras
                    .CountAsync(c => c.Status == "Active");

                // Recent Attendance
                var recentAttendance = await _context.Attendance
                    .OrderByDescending(a => a.AttendanceTime)
                    .Take(5)
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
                    data = new
                    {
                        totalUsers,
                        activeCameras,
                        todayAttendance,
                        present,
                        recentAttendance
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
}