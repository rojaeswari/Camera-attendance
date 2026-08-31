using CameraAttendance.Data;
using CameraAttendance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CameraAttendance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CamerasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CamerasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Cameras
        [HttpGet]
        public async Task<IActionResult> GetCameras()
        {
            var cameras = await _context.Cameras
                .OrderByDescending(c => c.Id)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                data = cameras
            });
        }


        // POST: api/Cameras
        [HttpPost]
        public async Task<IActionResult> AddCamera(
            [FromBody] CameraRequest request)
        {
            try
            {
                var existingCamera = await _context.Cameras
                    .FirstOrDefaultAsync(c =>
                        c.IPAddress == request.IPAddress);

                if (existingCamera != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Camera IP address already exists"
                    });
                }

                var camera = new CameraModel
                {
                    CameraName = request.CameraName,
                    IPAddress = request.IPAddress,
                    Location = request.Location,
                    Status = string.IsNullOrWhiteSpace(request.Status)
                        ? "Active"
                        : request.Status,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Cameras.Add(camera);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Camera added successfully",
                    data = camera.Id
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


        // PUT: api/Cameras/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCamera(
            int id,
            [FromBody] CameraRequest request)
        {
            try
            {
                var camera = await _context.Cameras
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (camera == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Camera not found"
                    });
                }

                var ipExists = await _context.Cameras
                    .AnyAsync(c =>
                        c.IPAddress == request.IPAddress &&
                        c.Id != id);

                if (ipExists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Camera IP address already exists"
                    });
                }

                camera.CameraName = request.CameraName;
                camera.IPAddress = request.IPAddress;
                camera.Location = request.Location;
                camera.Status = request.Status;
                camera.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Camera updated successfully"
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


        // DELETE: api/Cameras/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCamera(int id)
        {
            try
            {
                var camera = await _context.Cameras
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (camera == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Camera not found"
                    });
                }

                _context.Cameras.Remove(camera);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Camera deleted successfully"
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


    public class CameraRequest
    {
        public string CameraName { get; set; } = string.Empty;

        public string IPAddress { get; set; } = string.Empty;

        public string? Location { get; set; }

        public string Status { get; set; } = "Active";
    }
}