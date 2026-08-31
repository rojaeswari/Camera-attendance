using CameraAttendance.Services;
using Microsoft.AspNetCore.Mvc;

namespace CameraAttendance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FtpController : ControllerBase
    {
        private readonly FtpImageService _ftpImageService;

        public FtpController(FtpImageService ftpImageService)
        {
            _ftpImageService = ftpImageService;
        }

        [HttpGet("process-images")]
        public IActionResult ProcessImages()
        {
            var result = _ftpImageService.ProcessImages();

            return Ok(new
            {
                success = true,
                message = result
            });
        }
    }
}