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
                var today = DateTime.UtcNow.Date;

                // Total users
                var totalUsers = await _context.Users.CountAsync();

                // Today's attendance
                var todayAttendance = await _context.Attendance
                    .CountAsync(a => a.AttendanceDate == today);

                // Present count
                var present = await _context.Attendance
                    .CountAsync(a =>
                        a.AttendanceDate == today &&
                        a.Status == "Present");

                // Active cameras
                var activeCameras = await _context.Cameras
                    .CountAsync(c => c.Status == "Active");

                // Recent attendance
                var recentAttendance = await _context.Attendance
                    .Include(a => a.User)
                    .OrderByDescending(a => a.AttendanceDate)
                    .ThenByDescending(a => a.AttendanceTime)
                    .Take(5)
                    .Select(a => new
                    {
                        a.Id,
                        UserName = a.User!.Name,
                        a.AttendanceDate,
                        a.AttendanceTime,
                        a.CameraName,
                        a.Status
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