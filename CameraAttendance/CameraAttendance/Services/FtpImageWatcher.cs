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

        private readonly FaceRecognitionService
            _faceRecognitionService;

        private FileSystemWatcher? _watcher;


        // =========================================================
        // FTP CAMERA FOLDER
        // =========================================================

        private readonly string _ftpFolder =
            @"D:\ftpserver\Picture\Face Recognition";


        // =========================================================
        // MATCH THRESHOLD
        // =========================================================

        private const double MATCH_THRESHOLD = 0.42;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public FtpImageWatcher(
            IWebHostEnvironment environment,

            ILogger<FtpImageWatcher> logger,

            IServiceScopeFactory scopeFactory,

            FaceRecognitionService faceRecognitionService)
        {
            _environment =
                environment;

            _logger =
                logger;

            _scopeFactory =
                scopeFactory;

            _faceRecognitionService =
                faceRecognitionService;
        }


        // =========================================================
        // START WATCHER
        // =========================================================

        protected override Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            try
            {
                // =================================================
                // CHECK FTP FOLDER
                // =================================================

                if (!Directory.Exists(_ftpFolder))
                {
                    _logger.LogError(
                        "FTP folder not found: {Folder}",
                        _ftpFolder
                    );

                    return Task.CompletedTask;
                }


                // =================================================
                // CREATE REQUIRED FOLDERS
                // =================================================

                string uploadsFolder =
                    Path.Combine(
                        _environment.WebRootPath,
                        "uploads"
                    );


                string attendanceFolder =
                    Path.Combine(
                        uploadsFolder,
                        "attendance"
                    );


                string facesFolder =
                    Path.Combine(
                        uploadsFolder,
                        "faces"
                    );


                string strangersFolder =
                    Path.Combine(
                        uploadsFolder,
                        "strangers"
                    );


                Directory.CreateDirectory(
                    uploadsFolder
                );

                Directory.CreateDirectory(
                    attendanceFolder
                );

                Directory.CreateDirectory(
                    facesFolder
                );

                Directory.CreateDirectory(
                    strangersFolder
                );


                // =================================================
                // FILE SYSTEM WATCHER
                // =================================================

                _watcher =
                    new FileSystemWatcher(
                        _ftpFolder
                    );


                _watcher.Filter =
                    "*.*";


                _watcher.NotifyFilter =
                    NotifyFilters.FileName |
                    NotifyFilters.LastWrite |
                    NotifyFilters.Size;


                // =================================================
                // NEW FILE
                // =================================================

                _watcher.Created += async (
                    sender,
                    e) =>
                {
                    await ProcessNewImage(
                        e.FullPath
                    );
                };


                // =================================================
                // START
                // =================================================

                _watcher.EnableRaisingEvents =
                    true;


                _logger.LogInformation(
                    "========================================"
                );

                _logger.LogInformation(
                    "FTP IMAGE WATCHER STARTED"
                );

                _logger.LogInformation(
                    "FTP Folder: {Folder}",
                    _ftpFolder
                );

                _logger.LogInformation(
                    "========================================"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to start FTP watcher"
                );
            }


            return Task.CompletedTask;
        }


        // =========================================================
        // PROCESS NEW IMAGE
        // =========================================================

        private async Task ProcessNewImage(
            string sourceFile)
        {
            try
            {
                _logger.LogInformation(
                    "========================================"
                );

                _logger.LogInformation(
                    "NEW CAMERA IMAGE"
                );

                _logger.LogInformation(
                    "Source: {File}",
                    sourceFile
                );

                _logger.LogInformation(
                    "========================================"
                );


                // =================================================
                // WAIT UNTIL FTP FINISHES UPLOAD
                // =================================================

                await WaitForFile(
                    sourceFile
                );


                // =================================================
                // CHECK EXTENSION
                // =================================================

                string extension =
                    Path.GetExtension(
                        sourceFile
                    ).ToLowerInvariant();


                if (extension != ".jpg" &&
                    extension != ".jpeg" &&
                    extension != ".png")
                {
                    _logger.LogWarning(
                        "Unsupported image: {File}",
                        sourceFile
                    );

                    return;
                }


                // =================================================
                // DIRECTLY PROCESS FTP IMAGE
                //
                // IMPORTANT:
                // We DO NOT copy raw image to wwwroot/uploads
                // =================================================

                await RecognizeFace(
                    sourceFile
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing camera image: {File}",
                    sourceFile
                );
            }
        }


        // =========================================================
        // RECOGNIZE FACE
        // =========================================================

        private async Task RecognizeFace(
            string imagePath)
        {
            try
            {
                using IServiceScope scope =
                    _scopeFactory.CreateScope();


                var context =
                    scope.ServiceProvider
                        .GetRequiredService<AppDbContext>();


                // =================================================
                // GET ACTIVE USERS WITH FACE IMAGE
                // =================================================

                var users =
                    await context.Users
                        .Where(x =>
                            x.IsActive &&
                            x.FaceImagePath != null &&
                            x.FaceImagePath != "")
                        .ToListAsync();


                // =================================================
                // NO REGISTERED USERS
                // =================================================

                if (users.Count == 0)
                {
                    _logger.LogWarning(
                        "No registered users found. Saving as Stranger."
                    );


                    await CreateStrangerAttendance(
                        context,
                        imagePath,
                        0
                    );


                    return;
                }


                // =================================================
                // FIRST CHECK:
                // CAMERA IMAGE MUST HAVE A FACE
                // =================================================

                // We use the recognition service itself below.
                // If no face is detected, all comparisons return 0.
                // =================================================


                UserModel? matchedUser = null;

                double bestSimilarity =
                    double.MinValue;

                double bestConfidence = 0;


                // =================================================
                // COMPARE WITH EVERY USER
                // =================================================

                foreach (var user in users)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(
                            user.FaceImagePath))
                        {
                            continue;
                        }


                        // =========================================
                        // CONVERT DB PATH TO PHYSICAL PATH
                        // =========================================

                        string relativePath =
                            user.FaceImagePath
                                .TrimStart('/')
                                .Replace(
                                    "/",
                                    Path.DirectorySeparatorChar
                                        .ToString()
                                );


                        string registeredImagePath =
                            Path.Combine(
                                _environment.WebRootPath,
                                relativePath
                            );


                        // =========================================
                        // CHECK REGISTERED IMAGE
                        // =========================================

                        if (!File.Exists(
                            registeredImagePath))
                        {
                            _logger.LogWarning(
                                "Registered image not found | User: {Name} | Path: {Path}",
                                user.Name,
                                registeredImagePath
                            );

                            continue;
                        }


                        // =========================================
                        // FACE RECOGNITION
                        // =========================================

                        FaceRecognitionResult result =
                            _faceRecognitionService.Recognize(
                                imagePath,
                                registeredImagePath
                            );


                        _logger.LogInformation(
                            "USER CHECK | User: {Name} | Similarity: {Similarity:F4} | Confidence: {Confidence:F4}",
                            user.Name,
                            result.Similarity,
                            result.FaceConfidence
                        );


                        // =========================================
                        // KEEP BEST MATCH
                        // =========================================

                        if (result.Similarity >
                            bestSimilarity)
                        {
                            bestSimilarity =
                                result.Similarity;

                            bestConfidence =
                                result.FaceConfidence;

                            matchedUser =
                                user;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error comparing with user: {Name}",
                            user.Name
                        );
                    }
                }


                // =================================================
                // NO FACE / NO VALID COMPARISON
                // =================================================

                if (bestSimilarity == double.MinValue)
                {
                    _logger.LogWarning(
                        "No valid face comparison was possible."
                    );

                    return;
                }


                if (bestSimilarity <= 0)
                {
                    _logger.LogWarning(
                        "No face detected in camera image. Image ignored."
                    );

                    return;
                }


                // =================================================
                // FINAL DECISION
                // =================================================

                bool isMatch =
                    matchedUser != null &&
                    bestSimilarity >= MATCH_THRESHOLD;


                _logger.LogInformation(
                    "========================================"
                );

                _logger.LogInformation(
                    "FINAL FACE RECOGNITION RESULT"
                );

                _logger.LogInformation(
                    "Best User       : {User}",
                    matchedUser?.Name ?? "None"
                );

                _logger.LogInformation(
                    "Best Similarity : {Similarity:F4}",
                    bestSimilarity
                );

                _logger.LogInformation(
                    "Confidence      : {Confidence:F4}",
                    bestConfidence
                );

                _logger.LogInformation(
                    "Threshold       : {Threshold:F2}",
                    MATCH_THRESHOLD
                );

                _logger.LogInformation(
                    "MATCH            : {Match}",
                    isMatch
                );

                _logger.LogInformation(
                    "========================================"
                );


                // =================================================
                // KNOWN USER
                // =================================================

                if (isMatch)
                {
                    await CreateUserAttendance(
                        context,
                        matchedUser!,
                        imagePath,
                        bestSimilarity
                    );

                    return;
                }


                // =================================================
                // UNKNOWN USER / STRANGER
                // =================================================

                await CreateStrangerAttendance(
                    context,
                    imagePath,
                    bestSimilarity
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Face recognition failed: {Image}",
                    imagePath
                );
            }
        }


        // =========================================================
        // USER ATTENDANCE
        // =========================================================

        private async Task CreateUserAttendance(
            AppDbContext context,

            UserModel user,

            string imagePath,

            double similarity)
        {
            DateTime now =
                DateTime.Now;


            // =================================================
            // CHECK TODAY DUPLICATE
            // =================================================

            bool alreadyMarked =
                await context.Attendance
                    .AnyAsync(x =>
                        x.UserId == user.Id &&

                        x.AttendanceTime >=
                            now.Date &&

                        x.AttendanceTime <
                            now.Date.AddDays(1)
                    );


            if (alreadyMarked)
            {
                _logger.LogInformation(
                    "Attendance already marked today for {Name}",
                    user.Name
                );

                return;
            }


            // =================================================
            // ATTENDANCE FOLDER
            // =================================================

            string attendanceFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "attendance"
                );


            Directory.CreateDirectory(
                attendanceFolder
            );


            // =================================================
            // SAFE USER NAME
            // =================================================

            string safeName =
                MakeSafeFileName(
                    user.Name
                );


            string extension =
                Path.GetExtension(
                    imagePath
                );


            if (string.IsNullOrWhiteSpace(
                extension))
            {
                extension = ".jpg";
            }


            // =================================================
            // FILE NAME
            // =================================================

            string fileName =
                $"{safeName}_{now:yyyyMMdd_HHmmssfff}{extension}";


            string destinationPath =
                Path.Combine(
                    attendanceFolder,
                    fileName
                );


            // =================================================
            // COPY ONLY MATCHED IMAGE
            // =================================================

            File.Copy(
                imagePath,
                destinationPath,
                true
            );


            // =================================================
            // DATABASE
            // =================================================

            var attendance =
                new AttendanceModel
                {
                    UserId =
                        user.Id,

                    UserName =
                        user.Name,

                    AttendanceTime =
                        now,

                    ImagePath =
                        $"/uploads/attendance/{fileName}"
                };


            context.Attendance.Add(
                attendance
            );


            await context.SaveChangesAsync();


            _logger.LogInformation(
                "========================================"
            );

            _logger.LogInformation(
                "KNOWN USER ATTENDANCE SAVED"
            );

            _logger.LogInformation(
                "User       : {Name}",
                user.Name
            );

            _logger.LogInformation(
                "UserId     : {Id}",
                user.Id
            );

            _logger.LogInformation(
                "Similarity : {Similarity:F4}",
                similarity
            );

            _logger.LogInformation(
                "Image      : {Image}",
                destinationPath
            );

            _logger.LogInformation(
                "========================================"
            );
        }


        // =========================================================
        // STRANGER ATTENDANCE
        // =========================================================

        private async Task CreateStrangerAttendance(
            AppDbContext context,

            string imagePath,

            double similarity = 0)
        {
            DateTime now =
                DateTime.Now;


            // =================================================
            // STRANGER FOLDER
            // =================================================

            string strangerFolder =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "strangers"
                );


            Directory.CreateDirectory(
                strangerFolder
            );


            // =================================================
            // FILE NAME
            // =================================================

            string fileName =
                $"Stranger_{now:yyyyMMdd_HHmmssfff}.jpg";


            string destinationPath =
                Path.Combine(
                    strangerFolder,
                    fileName
                );


            // =================================================
            // COPY STRANGER IMAGE
            // =================================================

            File.Copy(
                imagePath,
                destinationPath,
                true
            );


            // =================================================
            // DATABASE
            // =================================================

            var stranger =
                new StrangerAttendanceModel
                {
                    UserId = 0,

                    UserName = "Stranger",

                    AttendanceTime = now,

                    ImagePath =
                        $"/uploads/strangers/{fileName}"
                };


            context.StrangerAttendance.Add(
                stranger
            );


            await context.SaveChangesAsync();


            _logger.LogWarning(
                "========================================"
            );

            _logger.LogWarning(
                "STRANGER ATTENDANCE SAVED"
            );

            _logger.LogWarning(
                "Similarity : {Similarity:F4}",
                similarity
            );

            _logger.LogWarning(
                "Image      : {Image}",
                destinationPath
            );

            _logger.LogWarning(
                "========================================"
            );
        }


        // =========================================================
        // SAFE FILE NAME
        // =========================================================

        private string MakeSafeFileName(
            string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "User";
            }


            foreach (
                char c in
                Path.GetInvalidFileNameChars())
            {
                name =
                    name.Replace(
                        c,
                        '_'
                    );
            }


            return name
                .Trim()
                .Replace(
                    " ",
                    "_"
                );
        }


        // =========================================================
        // WAIT FOR FTP UPLOAD
        // =========================================================

        private async Task WaitForFile(
            string filePath)
        {
            long previousSize = -1;


            for (int i = 0; i < 30; i++)
            {
                try
                {
                    if (!File.Exists(
                        filePath))
                    {
                        await Task.Delay(1000);

                        continue;
                    }


                    FileInfo fileInfo =
                        new FileInfo(
                            filePath
                        );


                    long currentSize =
                        fileInfo.Length;


                    // =============================================
                    // FILE SIZE STABLE
                    // =============================================

                    if (currentSize > 0 &&
                        currentSize ==
                            previousSize)
                    {
                        return;
                    }


                    previousSize =
                        currentSize;
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