namespace CameraAttendance.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; } 

        public string PasswordHash { get; set; }

        public int RoleId { get; set; }

        public bool IsActive { get; set; }

        public string? FaceImagePath { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

    }
}
