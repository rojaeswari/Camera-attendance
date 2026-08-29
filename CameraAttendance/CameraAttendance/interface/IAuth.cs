using CameraAttendance.DTOs;
using CameraAttendance.Models;

namespace CameraAttendance.Interface
{
    public interface IAuth
    {
        Task<ResponceDTO> LoginUser(string email, string password);
        Task<ResponceDTO>RegisterUser(UserModel Model);
        Task<ResponceDTO> AddUser(RegisterDTO Model);
    }
}
