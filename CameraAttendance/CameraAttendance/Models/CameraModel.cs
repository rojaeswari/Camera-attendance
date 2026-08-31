using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CameraAttendance.Models
{
    [Table("Cameras")]
    public class CameraModel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CameraName { get; set; } = string.Empty;

        [Required]
        public string IPAddress { get; set; } = string.Empty;

        public string? Location { get; set; }

        [Required]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}