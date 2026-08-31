using System;
using System.IO;
using System.Linq;

namespace CameraAttendance.Services
{
    public class FtpImageService
    {
        private readonly string _imageFolder =
            @"D:\ftpserver\picture\Face Recognition";

        public string ProcessImages()
        {
            if (!Directory.Exists(_imageFolder))
            {
                return "Folder not found: " + _imageFolder;
            }

            var images = Directory.GetFiles(
                _imageFolder,
                "*.*"
            )
            .Where(file =>
                file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            .ToList();

            if (images.Count == 0)
            {
                return "No images found in folder.";
            }

            foreach (var imagePath in images)
            {
                var fileName = Path.GetFileName(imagePath);

                string cameraName;

                if (fileName.StartsWith(
                    "CH01",
                    StringComparison.OrdinalIgnoreCase))
                {
                    cameraName = "Indoor Camera";
                }
                else if (fileName.StartsWith(
                    "CH02",
                    StringComparison.OrdinalIgnoreCase))
                {
                    cameraName = "Outdoor Camera";
                }
                else
                {
                    cameraName = "Unknown Camera";
                }

                Console.WriteLine(
                    $"Image: {fileName} | Camera: {cameraName}");
            }

            return $"{images.Count} image(s) processed successfully.";
        }
    }
}