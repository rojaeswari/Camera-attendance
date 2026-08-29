using Microsoft.AspNetCore.Http;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using CameraAttendance.DTOs;
using CameraAttendance.Interface;
using CameraAttendance.Models;
using Microsoft.AspNetCore.Mvc;

namespace CameraAttendance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuth Auth;

        public AuthController(IAuth auth)
        {
            Auth = auth;
        }

        [HttpPost("register")]
        public async Task<ResponceDTO> RegisterUser(UserModel Model)
        {
            return await Auth.RegisterUser(Model);
        }

        [HttpPost("login")]
        public async Task<ResponceDTO> LoginUser(string email, string password)
        {
            return await Auth.LoginUser(email, password);
        }
    }
}