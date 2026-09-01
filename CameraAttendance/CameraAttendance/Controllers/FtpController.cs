using CameraAttendance.Services;
using Microsoft.AspNetCore.Mvc;

namespace CameraAttendance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FtpController : ControllerBase
    {
        //private readonly FtpImageService _ftpImageService;

        //public FtpController(FtpImageService ftpImageService)
        //{
        //    _ftpImageService = ftpImageService;
        //}

        //[HttpGet("process-images")]
        //public IActionResult ProcessImages()
        //{
        //    var result = _ftpImageService.ProcessImages();

        //    return Ok(new
        //    {
        //        success = true,
        //        message = result
        //    });
        //}

        private readonly IWebHostEnvironment _environment;

        public FtpController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("save-image")]
        public IActionResult SaveImage(string fileName)
        {
            string sourceFolder = @"D:\ftpserver\Picture\Face Recognition";

            string destinationFolder = Path.Combine(
                _environment.WebRootPath,
                "FaceRecognition"
            );

            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            string sourcePath = Path.Combine(sourceFolder, fileName);
            string destinationPath = Path.Combine(destinationFolder, fileName);

            if (!System.IO.File.Exists(sourcePath))
            {
                return NotFound("Image not found in FTP folder");
            }

            System.IO.File.Copy(
                sourcePath,
                destinationPath,
                true
            );

            return Ok(new
            {
                message = "Image saved successfully",
                fileName = fileName,
                path = $"/FaceRecognition/{fileName}"
            });
        }
    }
}