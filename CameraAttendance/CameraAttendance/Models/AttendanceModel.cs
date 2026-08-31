namespace CameraAttendance.Models
{
    public class AttendanceModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime AttendanceDate { get; set; }

        public TimeSpan AttendanceTime { get; set; }

        public string? CameraName { get; set; }

        public string Status { get; set; } = "Present";

        public DateTime CreatedAt { get; set; }

        // Navigation property
        public UserModel? User { get; set; }
    }
}