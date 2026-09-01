using CameraAttendance.Data;
using CameraAttendance.Models;
using Microsoft.EntityFrameworkCore;

namespace CameraAttendance.Services
{
    public class FtpImageWatcher : BackgroundService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FtpImageWatcher> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private FileSystemWatcher? _watcher;

        // Camera / FTP server folder
        private readonly string _ftpFolder =
            @"D:\ftpserver\Picture\Face Recognition";


        public FtpImageWatcher(
            IWebHostEnvironment environment,
            ILogger<FtpImageWatcher> logger,
            IServiceScopeFactory scopeFactory)
        {
            _environment = environment;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }


        protected override Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            // wwwroot/uploads
            string destinationFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads"
            );

            // Create uploads folder
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            // Check FTP folder
            if (!Directory.Exists(_ftpFolder))
            {
                _logger.LogError(
                    "FTP folder not found: {Folder}",
                    _ftpFolder
                );

                return Task.CompletedTask;
            }

            _watcher = new FileSystemWatcher();

            _watcher.Path = _ftpFolder;

            _watcher.Filter = "*.*";

            _watcher.NotifyFilter =
                NotifyFilters.FileName |
                NotifyFilters.LastWrite |
                NotifyFilters.Size;


            // New image detected
            _watcher.Created += async (sender, e) =>
            {
                await ProcessNewImage(
                    e.FullPath,
                    destinationFolder
                );
            };


            _watcher.EnableRaisingEvents = true;


            _logger.LogInformation(
                "FTP Image Watcher started: {Folder}",
                _ftpFolder
            );

            return Task.CompletedTask;
        }


        // =========================================================
        // PROCESS NEW IMAGE
        // =========================================================

        private async Task ProcessNewImage(
            string sourceFile,
            string destinationFolder)
        {
            try
            {
                // Wait until FTP upload is completed
                await WaitForFile(sourceFile);

                string extension =
                    Path.GetExtension(sourceFile).ToLower();

                if (extension != ".jpg" &&
                    extension != ".jpeg" &&
                    extension != ".png")
                {
                    return;
                }


                string fileName =
                    Path.GetFileName(sourceFile);


                // Copy image to wwwroot/uploads
                string destinationFile =
                    Path.Combine(
                        destinationFolder,
                        fileName
                    );


                File.Copy(
                    sourceFile,
                    destinationFile,
                    true
                );


                _logger.LogInformation(
                    "New face image received: {FileName}",
                    fileName
                );


                // Face recognition
                await RecognizeFace(destinationFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing image: {File}",
                    sourceFile
                );
            }
        }


        // =========================================================
        // FACE RECOGNITION
        // =========================================================

        private async Task RecognizeFace(string imagePath)
        {
            try
            {
                using IServiceScope scope =
                    _scopeFactory.CreateScope();

                var context =
                    scope.ServiceProvider
                        .GetRequiredService<AppDbContext>();

                // Get registered users
                var users = await context.Users
                    .Where(x => x.FaceImagePath != null)
                    .ToListAsync();

                foreach (var user in users)
                {
                    if (string.IsNullOrWhiteSpace(user.FaceImagePath))
                    {
                        continue;
                    }

                    string registeredImagePath =
                        Path.Combine(
                            _environment.WebRootPath,
                            user.FaceImagePath
                                .TrimStart('/')
                                .Replace(
                                    "/",
                                    Path.DirectorySeparatorChar.ToString()
                                )
                        );

                    if (!System.IO.File.Exists(registeredImagePath))
                    {
                        continue;
                    }

                    // Compare camera image with registered image
                    bool isMatch = CompareFaces(
                        imagePath,
                        registeredImagePath
                    );

                    if (isMatch)
                    {
                        // USER FOUND
                        await CreateUserAttendance(
                            context,
                            user,
                            imagePath
                        );

                        return;
                    }
                }

                // NO USER FOUND
                await CreateStrangerAttendance(
                    context,
                    imagePath
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Face recognition failed for {Image}",
                    imagePath
                );
            }
        }
        private async Task CreateStrangerAttendance(
    AppDbContext context,
    string imagePath)
        {
            DateTime now = DateTime.Now;

            string attendanceFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "attendance"
                );

            if (!Directory.Exists(attendanceFolder))
            {
                Directory.CreateDirectory(attendanceFolder);
            }

            string fileName =
                $"Stranger_{now:yyyyMMdd_HHmmss}.jpg";

            string attendanceImage =
                Path.Combine(
                    attendanceFolder,
                    fileName
                );

            // Save stranger image
            File.Copy(
                imagePath,
                attendanceImage,
                true
            );

            var attendance = new AttendanceModel
            {
                UserId = 0,
                UserName = "Stranger",
                AttendanceTime = now,
                ImagePath = $"/uploads/attendance/{fileName}"
            };

            context.Attendance.Add(attendance);

            await context.SaveChangesAsync();

            _logger.LogInformation(
                "Stranger detected at {Time}",
                now
            );
        }


        private async Task CreateUserAttendance(
    AppDbContext context,
    UserModel user,
    string imagePath)
        {
            DateTime now = DateTime.Now;

            // Check duplicate attendance today
            bool alreadyMarked =
                await context.Attendance.AnyAsync(x =>
                    x.UserId == user.Id &&
                    x.AttendanceTime >= now.Date &&
                    x.AttendanceTime < now.Date.AddDays(1)
                );

            if (alreadyMarked)
            {
                _logger.LogInformation(
                    "Attendance already marked for {Name}",
                    user.Name
                );

                return;
            }

            string attendanceFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "attendance"
                );

            if (!Directory.Exists(attendanceFolder))
            {
                Directory.CreateDirectory(attendanceFolder);
            }

            string safeName =
                user.Name.Replace(" ", "_");

            string extension =
                Path.GetExtension(imagePath);

            if (string.IsNullOrEmpty(extension))
            {
                extension = ".jpg";
            }

            string fileName =
                $"{safeName}_{now:yyyyMMdd_HHmmss}{extension}";

            string attendanceImage =
                Path.Combine(
                    attendanceFolder,
                    fileName
                );

            File.Copy(
                imagePath,
                attendanceImage,
                true
            );

            var attendance = new AttendanceModel
            {
                UserId = user.Id,
                UserName = user.Name,
                AttendanceTime = now,
                ImagePath = $"/uploads/attendance/{fileName}"
            };

            context.Attendance.Add(attendance);

            await context.SaveChangesAsync();

            _logger.LogInformation(
                "Attendance marked for {Name} at {Time}",
                user.Name,
                now
            );
        }
        // =========================================================
        // FACE COMPARISON
        // =========================================================

        private bool CompareFaces(
            string imagePath,
            string registeredImagePath)
        {
            // TEMPORARY TESTING
            //
            // IMPORTANT:
            // This currently returns true for testing only.
            //
            // Later actual face recognition logic
            // should be implemented here.

            return false;
        }


        // =========================================================
        // CREATE ATTENDANCE
        // =========================================================

        private async Task CreateAttendance(
            AppDbContext context,
            UserModel user,
            string imagePath)
        {
            DateTime now = DateTime.Now;


            // Check duplicate attendance today
            bool alreadyMarked =
                await context.Attendance
                    .AnyAsync(x =>
                        x.UserId == user.Id &&
                        x.AttendanceTime >= now.Date &&
                        x.AttendanceTime <
                            now.Date.AddDays(1)
                    );


            if (alreadyMarked)
            {
                _logger.LogInformation(
                    "Attendance already marked for {Name}",
                    user.Name
                );

                return;
            }


            // Attendance folder
            string attendanceFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "attendance"
                );


            if (!Directory.Exists(
                attendanceFolder))
            {
                Directory.CreateDirectory(
                    attendanceFolder);
            }


            // Safe user name
            string safeName =
                user.Name
                    .Replace(" ", "_");


            string extension =
                Path.GetExtension(imagePath);

            if (string.IsNullOrEmpty(extension))
            {
                extension = ".jpg";
            }


            string fileName =
                $"{safeName}_{now:yyyyMMdd_HHmmss}{extension}";


            string attendanceImage =
                Path.Combine(
                    attendanceFolder,
                    fileName
                );


            // Copy matched image
            File.Copy(
                imagePath,
                attendanceImage,
                true
            );


            // Create Attendance
            var attendance =
                new AttendanceModel
                {
                    UserId = user.Id,

                    UserName = user.Name,

                    AttendanceTime = now,

                    ImagePath =
                        $"/uploads/attendance/{fileName}"
                };


            context.Attendance.Add(
                attendance);


            await context.SaveChangesAsync();


            _logger.LogInformation(
                "Attendance marked for {Name} at {Time}",
                user.Name,
                now
            );
        }


        // =========================================================
        // WAIT UNTIL FILE UPLOAD COMPLETES
        // =========================================================

        private async Task WaitForFile(
            string filePath)
        {
            long previousSize = -1;


            for (int i = 0; i < 30; i++)
            {
                try
                {
                    if (!File.Exists(filePath))
                    {
                        await Task.Delay(1000);
                        continue;
                    }


                    FileInfo fileInfo =
                        new FileInfo(filePath);


                    long currentSize =
                        fileInfo.Length;


                    // Same size for two checks
                    if (currentSize > 0 &&
                        currentSize == previousSize)
                    {
                        return;
                    }


                    previousSize = currentSize;
                }
                catch (IOException)
                {
                    // FTP is still writing
                }


                await Task.Delay(1000);
            }


            throw new IOException(
                $"File is still being written: {filePath}"
            );
        }


        // =========================================================
        // DISPOSE
        // =========================================================

        public override void Dispose()
        {
            _watcher?.Dispose();

            base.Dispose();
        }
    }
}