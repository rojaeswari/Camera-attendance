using FaceAiSharp;
using FaceAiSharp.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CameraAttendance.Services
{
    public class FaceRecognitionResult
    {
        public bool IsMatch { get; set; }

        public string? UserName { get; set; }

        public double Similarity { get; set; }

        public double FaceConfidence { get; set; }
    }


    public class FaceRecognitionService
    {
        private readonly ILogger<FaceRecognitionService> _logger;

        private readonly IFaceDetectorWithLandmarks _detector;

        private readonly IFaceEmbeddingsGenerator _recognizer;


        // =========================================================
        // MATCH THRESHOLD
        // =========================================================

        // 0.42 = default matching threshold
        //
        // If known user is coming as Stranger:
        // check similarity first.
        //
        // Don't blindly reduce this value.
        // =========================================================

        private const double MATCH_THRESHOLD = 0.42;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public FaceRecognitionService(
            ILogger<FaceRecognitionService> logger)
        {
            _logger = logger;


            // =====================================================
            // FACE DETECTOR
            // =====================================================

            _detector =
                FaceAiSharpBundleFactory
                    .CreateFaceDetectorWithLandmarks();


            // =====================================================
            // ARC FACE EMBEDDING GENERATOR
            // =====================================================

            _recognizer =
                FaceAiSharpBundleFactory
                    .CreateFaceEmbeddingsGenerator();


            _logger.LogInformation(
                "Face Recognition Service initialized"
            );
        }


        // =========================================================
        // RECOGNIZE
        // =========================================================

        public FaceRecognitionResult Recognize(
            string cameraImagePath,
            string registeredImagePath)
        {
            try
            {
                // =================================================
                // CHECK CAMERA IMAGE
                // =================================================

                if (!File.Exists(cameraImagePath))
                {
                    _logger.LogWarning(
                        "Camera image not found: {Path}",
                        cameraImagePath
                    );

                    return new FaceRecognitionResult
                    {
                        IsMatch = false,
                        Similarity = 0
                    };
                }


                // =================================================
                // CHECK REGISTERED IMAGE
                // =================================================

                if (!File.Exists(registeredImagePath))
                {
                    _logger.LogWarning(
                        "Registered image not found: {Path}",
                        registeredImagePath
                    );

                    return new FaceRecognitionResult
                    {
                        IsMatch = false,
                        Similarity = 0
                    };
                }


                // =================================================
                // LOAD CAMERA IMAGE
                // =================================================

                using Image<Rgb24> cameraImage =
                    Image.Load<Rgb24>(
                        cameraImagePath
                    );


                // =================================================
                // LOAD REGISTERED IMAGE
                // =================================================

                using Image<Rgb24> registeredImage =
                    Image.Load<Rgb24>(
                        registeredImagePath
                    );


                // =================================================
                // AUTO ORIENTATION
                // =================================================

                cameraImage.Mutate(
                    x => x.AutoOrient()
                );

                registeredImage.Mutate(
                    x => x.AutoOrient()
                );


                // =================================================
                // DETECT CAMERA FACES
                // =================================================

                var cameraFaces =
                    _detector.DetectFaces(
                        cameraImage
                    );


                int cameraFaceCount =
                    cameraFaces?.Count ?? 0;


                _logger.LogInformation(
                    "Camera faces detected: {Count} | Image: {Image}",
                    cameraFaceCount,
                    Path.GetFileName(cameraImagePath)
                );


                // =================================================
                // NO CAMERA FACE
                // =================================================

                if (cameraFaces == null ||
                    cameraFaces.Count == 0)
                {
                    _logger.LogWarning(
                        "NO FACE DETECTED IN CAMERA IMAGE: {Image}",
                        Path.GetFileName(cameraImagePath)
                    );

                    return new FaceRecognitionResult
                    {
                        IsMatch = false,
                        Similarity = 0,
                        FaceConfidence = 0
                    };
                }


                // =================================================
                // DETECT REGISTERED FACES
                // =================================================

                var registeredFaces =
                    _detector.DetectFaces(
                        registeredImage
                    );


                if (registeredFaces == null ||
                    registeredFaces.Count == 0)
                {
                    _logger.LogWarning(
                        "NO FACE DETECTED IN REGISTERED IMAGE: {Image}",
                        Path.GetFileName(registeredImagePath)
                    );

                    return new FaceRecognitionResult
                    {
                        IsMatch = false,
                        Similarity = 0,
                        FaceConfidence = 0
                    };
                }


                // =================================================
                // GET BEST REGISTERED FACE
                // =================================================

                var registeredFace =
                    registeredFaces
                        .Where(x => x.Landmarks != null)
                        .OrderByDescending(
                            x => x.Confidence
                        )
                        .FirstOrDefault();


                if (registeredFace == null)
                {
                    _logger.LogWarning(
                        "Registered face landmarks not available: {Image}",
                        Path.GetFileName(registeredImagePath)
                    );

                    return new FaceRecognitionResult
                    {
                        IsMatch = false,
                        Similarity = 0
                    };
                }


                // =================================================
                // ALIGN REGISTERED FACE
                // =================================================

                using Image<Rgb24> alignedRegistered =
                    registeredImage.Clone();


                _recognizer.AlignFaceUsingLandmarks(
                    alignedRegistered,
                    registeredFace.Landmarks!
                );


                // =================================================
                // GENERATE REGISTERED EMBEDDING
                // =================================================

                var registeredEmbedding =
                    _recognizer.GenerateEmbedding(
                        alignedRegistered
                    );


                // =================================================
                // BEST SIMILARITY
                // =================================================

                double bestSimilarity =
                    double.MinValue;

                double bestConfidence = 0;


                // =================================================
                // COMPARE EVERY CAMERA FACE
                // =================================================

                foreach (var cameraFace in cameraFaces)
                {
                    try
                    {
                        if (cameraFace.Landmarks == null)
                        {
                            continue;
                        }


                        // =========================================
                        // ALIGN CAMERA FACE
                        // =========================================

                        using Image<Rgb24> alignedCamera =
                            cameraImage.Clone();


                        _recognizer.AlignFaceUsingLandmarks(
                            alignedCamera,
                            cameraFace.Landmarks!
                        );


                        // =========================================
                        // GENERATE CAMERA EMBEDDING
                        // =========================================

                        var cameraEmbedding =
                            _recognizer.GenerateEmbedding(
                                alignedCamera
                            );


                        // =========================================
                        // COSINE / DOT SIMILARITY
                        // =========================================

                        double similarity =
                            cameraEmbedding.Dot(
                                registeredEmbedding
                            );


                        _logger.LogInformation(
                            "FACE CHECK | Camera: {Camera} | Registered: {Registered} | Similarity: {Similarity:F4} | Confidence: {Confidence:F4}",
                            Path.GetFileName(cameraImagePath),
                            Path.GetFileName(registeredImagePath),
                            similarity,
                            cameraFace.Confidence
                        );


                        // =========================================
                        // KEEP BEST
                        // =========================================

                        if (similarity > bestSimilarity)
                        {
                            bestSimilarity =
                                similarity;

                            bestConfidence =
                                (double)cameraFace.Confidence;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error processing camera face"
                        );
                    }
                }


                // =================================================
                // NO VALID EMBEDDING
                // =================================================

                if (bestSimilarity == double.MinValue)
                {
                    _logger.LogWarning(
                        "Unable to generate valid face embedding"
                    );

                    return new FaceRecognitionResult
                    {
                        IsMatch = false,
                        Similarity = 0
                    };
                }


                // =================================================
                // FINAL MATCH
                // =================================================

                bool isMatch =
                    bestSimilarity >= MATCH_THRESHOLD;


                _logger.LogInformation(
                    "----------------------------------------"
                );

                _logger.LogInformation(
                    "FACE RESULT"
                );

                _logger.LogInformation(
                    "Registered Image : {Image}",
                    Path.GetFileName(registeredImagePath)
                );

                _logger.LogInformation(
                    "Similarity       : {Similarity:F4}",
                    bestSimilarity
                );

                _logger.LogInformation(
                    "Threshold        : {Threshold:F2}",
                    MATCH_THRESHOLD
                );

                _logger.LogInformation(
                    "Match            : {Match}",
                    isMatch
                );

                _logger.LogInformation(
                    "----------------------------------------"
                );


                return new FaceRecognitionResult
                {
                    IsMatch =
                        isMatch,

                    Similarity =
                        bestSimilarity,

                    FaceConfidence =
                        bestConfidence
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Face recognition error"
                );

                return new FaceRecognitionResult
                {
                    IsMatch = false,
                    Similarity = 0,
                    FaceConfidence = 0
                };
            }
        }
    }
}