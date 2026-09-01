using System.ComponentModel.DataAnnotations;

namespace CameraAttendance.Models
{
    public class AttendanceModel
    {
        [Key]
        public int AttendanceId { get; set; }

        
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public DateTime AttendanceTime { get; set; }

        [MaxLength(500)]
        public string? ImagePath { get; set; }
    }
}