using CameraAttendance.Models;
using Microsoft.EntityFrameworkCore;

namespace CameraAttendance.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserModel> Users { get; set; }

        public DbSet<AttendanceModel> Attendance { get; set; }

        public DbSet<CameraModel> Cameras { get; set; }
        public DbSet<StrangerAttendanceModel> StrangerAttendance { get; set; }

    }
}