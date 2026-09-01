using System;
using System.IO;
using System.Linq;

namespace CameraAttendance.Services
{
    public class FtpImageService
    {
        //    private readonly string _imageFolder =
        //        @"D:\ftpserver\picture\Face Recognition";

        //    public string ProcessImages()
        //    {
        //        if (!Directory.Exists(_imageFolder))
        //        {
        //            return "Face Recognition folder not found.";
        //        }

        //        // Create camera folders
        //        string indoorFolder =
        //            Path.Combine(_imageFolder, "Indoor Camera");

        //        string outdoorFolder =
        //            Path.Combine(_imageFolder, "Outdoor Camera");

        //        Directory.CreateDirectory(indoorFolder);
        //        Directory.CreateDirectory(outdoorFolder);

        //        var images = Directory.GetFiles(
        //            _imageFolder,
        //            "*.*",
        //            SearchOption.TopDirectoryOnly
        //        )
        //        .Where(file =>
        //            file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
        //            file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        //            file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        //        .ToList();

        //        int indoorCount = 0;
        //        int outdoorCount = 0;

        //        foreach (var imagePath in images)
        //        {
        //            string fileName = Path.GetFileName(imagePath);

        //            // CH01 → Indoor Camera
        //            if (fileName.StartsWith(
        //                "CH01",
        //                StringComparison.OrdinalIgnoreCase))
        //            {
        //                string destination =
        //                    Path.Combine(indoorFolder, fileName);

        //                File.Copy(
        //                    imagePath,
        //                    destination,
        //                    true
        //                );

        //                indoorCount++;
        //            }

        //            // CH02 → Outdoor Camera
        //            else if (fileName.StartsWith(
        //                "CH02",
        //                StringComparison.OrdinalIgnoreCase))
        //            {
        //                string destination =
        //                    Path.Combine(outdoorFolder, fileName);

        //                File.Copy(
        //                    imagePath,
        //                    destination,
        //                    true
        //                );

        //                outdoorCount++;
        //            }
        //        }

        //        return $"Processed successfully. Indoor: {indoorCount}, Outdoor: {outdoorCount}";
        //    }
        private readonly IWebHostEnvironment environment;

        public FtpImageService(IWebHostEnvironment envi)
        {
            environment = envi;
        }

        public string SaveFaceImage(string FileName)
        {
            string FtpFolder = @"D:\ftpserver\picture\Face Recognition";

            string ProjectFolder = Path.Combine(environment.WebRootPath, "uploads");


            if (!Directory.Exists(ProjectFolder))
            {
                Directory.CreateDirectory(ProjectFolder);
            }

            string sourceFile = Path.Combine(FtpFolder, FileName);
            string DestinationFile = Path.Combine(ProjectFolder, FileName);

            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, DestinationFile, true);
            }


            return DestinationFile;
        }

    }
}