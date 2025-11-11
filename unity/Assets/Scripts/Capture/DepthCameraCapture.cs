/**
 * Depth Camera Capture - Desktop volumetric capture system
 * Supports Intel RealSense, Azure Kinect, and webcam fallback
 */

using UnityEngine;
using System;
using System.Collections.Generic;

namespace HoloCall.Capture
{
    public class DepthCameraCapture : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private CameraType cameraType = CameraType.RealSense;
        [SerializeField] private int targetFrameRate = 30;
        [SerializeField] private Vector2Int depthResolution = new Vector2Int(640, 480);
        [SerializeField] private Vector2Int rgbResolution = new Vector2Int(1280, 720);

        [Header("Point Cloud Settings")]
        [SerializeField] private int maxPoints = 30000;
        [SerializeField] private float minDepth = 0.3f; // meters
        [SerializeField] private float maxDepth = 4.0f; // meters
        [SerializeField] private bool enableBackgroundRemoval = true;

        [Header("Compression")]
        [SerializeField] private CompressionType compressionType = CompressionType.Octree;
        [SerializeField] private int compressionLevel = 5;

        // Captured data
        private PointCloud currentPointCloud;
        private Texture2D rgbTexture;
        private ushort[] depthData;
        private byte[] rgbData;

        // Camera interface (abstract for multi-camera support)
        private IDepthCamera camera;

        // Performance tracking
        private float lastCaptureTime;
        private int frameCount;

        public event Action<PointCloudData> OnPointCloudCaptured;

        private void Start()
        {
            InitializeCamera();
        }

        private void Update()
        {
            if (camera != null && camera.IsConnected())
            {
                CaptureFrame();
            }
        }

        /// <summary>
        /// Initialize depth camera based on type
        /// </summary>
        private void InitializeCamera()
        {
            Debug.Log($"[DepthCapture] Initializing {cameraType} camera");

            try
            {
                switch (cameraType)
                {
                    case CameraType.RealSense:
                        camera = new RealSenseCamera();
                        break;

                    case CameraType.Kinect:
                        camera = new AzureKinectCamera();
                        break;

                    case CameraType.OAK_D:
                        camera = new OAKDCamera();
                        break;

                    case CameraType.Webcam:
                        camera = new WebcamFallback();
                        break;

                    default:
                        Debug.LogError($"[DepthCapture] Unknown camera type: {cameraType}");
                        return;
                }

                camera.Initialize(depthResolution, rgbResolution, targetFrameRate);

                if (camera.IsConnected())
                {
                    Debug.Log($"[DepthCapture] Camera initialized successfully");
                }
                else
                {
                    Debug.LogError($"[DepthCapture] Camera failed to connect");
                    FallbackToWebcam();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DepthCapture] Camera initialization error: {ex.Message}");
                FallbackToWebcam();
            }
        }

        /// <summary>
        /// Fallback to webcam if depth camera fails
        /// </summary>
        private void FallbackToWebcam()
        {
            Debug.LogWarning("[DepthCapture] Falling back to webcam");
            cameraType = CameraType.Webcam;
            camera = new WebcamFallback();
            camera.Initialize(depthResolution, rgbResolution, targetFrameRate);
        }

        /// <summary>
        /// Capture single frame (depth + RGB)
        /// </summary>
        private void CaptureFrame()
        {
            // Throttle to target frame rate
            float now = Time.time;
            if (now - lastCaptureTime < (1f / targetFrameRate))
            {
                return;
            }
            lastCaptureTime = now;

            // Capture depth and RGB
            depthData = camera.CaptureDepthFrame();
            rgbData = camera.CaptureRGBFrame();

            if (depthData == null || rgbData == null)
            {
                return;
            }

            // Generate point cloud
            Vector3[] points = DepthToPointCloud(depthData);
            Color[] colors = MapRGBToPoints(rgbData, points);

            // Background removal
            if (enableBackgroundRemoval)
            {
                (points, colors) = RemoveBackground(points, colors, depthData);
            }

            // Downsample if needed
            if (points.Length > maxPoints)
            {
                (points, colors) = DownsamplePoints(points, colors, maxPoints);
            }

            // Compress
            byte[] compressed = CompressPointCloud(points, colors);

            // Create point cloud data
            var pointCloudData = new PointCloudData
            {
                points = points,
                colors = colors,
                compressedData = compressed,
                timestamp = Time.time,
                pointCount = points.Length
            };

            currentPointCloud = new PointCloud
            {
                points = points,
                colors = colors
            };

            // Notify listeners
            OnPointCloudCaptured?.Invoke(pointCloudData);

            frameCount++;
        }

        /// <summary>
        /// Convert depth data to 3D point cloud
        /// </summary>
        private Vector3[] DepthToPointCloud(ushort[] depthData)
        {
            List<Vector3> points = new List<Vector3>();

            // Camera intrinsics (simplified - in production, get from camera)
            float focalLengthX = depthResolution.x / 2f;
            float focalLengthY = depthResolution.y / 2f;
            float centerX = depthResolution.x / 2f;
            float centerY = depthResolution.y / 2f;

            int width = depthResolution.x;
            int height = depthResolution.y;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    ushort depth = depthData[index];

                    // Convert depth to meters
                    float depthMeters = depth / 1000f;

                    // Filter by depth range
                    if (depthMeters < minDepth || depthMeters > maxDepth)
                    {
                        continue;
                    }

                    // Convert to 3D point using pinhole camera model
                    Vector3 point = new Vector3(
                        (x - centerX) * depthMeters / focalLengthX,
                        -(y - centerY) * depthMeters / focalLengthY,
                        depthMeters
                    );

                    points.Add(point);
                }
            }

            return points.ToArray();
        }

        /// <summary>
        /// Map RGB colors to points
        /// </summary>
        private Color[] MapRGBToPoints(byte[] rgbData, Vector3[] points)
        {
            Color[] colors = new Color[points.Length];

            // Simplified RGB mapping - in production, use proper camera alignment
            for (int i = 0; i < points.Length; i++)
            {
                if (i * 3 + 2 < rgbData.Length)
                {
                    colors[i] = new Color(
                        rgbData[i * 3] / 255f,
                        rgbData[i * 3 + 1] / 255f,
                        rgbData[i * 3 + 2] / 255f
                    );
                }
                else
                {
                    colors[i] = Color.white;
                }
            }

            return colors;
        }

        /// <summary>
        /// Remove background points
        /// </summary>
        private (Vector3[], Color[]) RemoveBackground(
            Vector3[] points,
            Color[] colors,
            ushort[] depthData)
        {
            List<Vector3> filteredPoints = new List<Vector3>();
            List<Color> filteredColors = new List<Color>();

            // Simple background removal: remove points beyond threshold
            float backgroundThreshold = 2.5f; // meters

            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].z < backgroundThreshold)
                {
                    filteredPoints.Add(points[i]);
                    filteredColors.Add(colors[i]);
                }
            }

            return (filteredPoints.ToArray(), filteredColors.ToArray());
        }

        /// <summary>
        /// Downsample points using voxel grid
        /// </summary>
        private (Vector3[], Color[]) DownsamplePoints(
            Vector3[] points,
            Color[] colors,
            int targetCount)
        {
            // Simple uniform downsampling
            int stride = Mathf.Max(1, points.Length / targetCount);

            List<Vector3> downsampled = new List<Vector3>();
            List<Color> downsampledColors = new List<Color>();

            for (int i = 0; i < points.Length; i += stride)
            {
                downsampled.Add(points[i]);
                downsampledColors.Add(colors[i]);
            }

            return (downsampled.ToArray(), downsampledColors.ToArray());
        }

        /// <summary>
        /// Compress point cloud data
        /// </summary>
        private byte[] CompressPointCloud(Vector3[] points, Color[] colors)
        {
            List<byte> compressed = new List<byte>();

            switch (compressionType)
            {
                case CompressionType.Octree:
                    compressed.AddRange(CompressWithOctree(points, colors));
                    break;

                case CompressionType.Quantize:
                    compressed.AddRange(QuantizePoints(points, colors));
                    break;

                case CompressionType.Raw:
                    compressed.AddRange(SerializeRaw(points, colors));
                    break;
            }

            return compressed.ToArray();
        }

        /// <summary>
        /// Octree-based compression (simplified)
        /// </summary>
        private byte[] CompressWithOctree(Vector3[] points, Color[] colors)
        {
            // Simplified octree compression
            // In production, use proper octree implementation

            List<byte> data = new List<byte>();

            // Header: point count (4 bytes)
            data.AddRange(BitConverter.GetBytes(points.Length));

            // Quantize positions to 16-bit integers
            for (int i = 0; i < points.Length; i++)
            {
                short x = (short)(points[i].x * 1000);
                short y = (short)(points[i].y * 1000);
                short z = (short)(points[i].z * 1000);

                data.AddRange(BitConverter.GetBytes(x));
                data.AddRange(BitConverter.GetBytes(y));
                data.AddRange(BitConverter.GetBytes(z));

                // Colors: 3 bytes (RGB)
                data.Add((byte)(colors[i].r * 255));
                data.Add((byte)(colors[i].g * 255));
                data.Add((byte)(colors[i].b * 255));
            }

            return data.ToArray();
        }

        /// <summary>
        /// Quantize points to reduce size
        /// </summary>
        private byte[] QuantizePoints(Vector3[] points, Color[] colors)
        {
            List<byte> data = new List<byte>();

            data.AddRange(BitConverter.GetBytes(points.Length));

            for (int i = 0; i < points.Length; i++)
            {
                // 16-bit quantization for positions
                ushort x = (ushort)((points[i].x + 2f) * 10000);
                ushort y = (ushort)((points[i].y + 2f) * 10000);
                ushort z = (ushort)(points[i].z * 10000);

                data.AddRange(BitConverter.GetBytes(x));
                data.AddRange(BitConverter.GetBytes(y));
                data.AddRange(BitConverter.GetBytes(z));

                // 8-bit colors
                data.Add((byte)(colors[i].r * 255));
                data.Add((byte)(colors[i].g * 255));
                data.Add((byte)(colors[i].b * 255));
            }

            return data.ToArray();
        }

        /// <summary>
        /// Raw serialization (no compression)
        /// </summary>
        private byte[] SerializeRaw(Vector3[] points, Color[] colors)
        {
            List<byte> data = new List<byte>();

            data.AddRange(BitConverter.GetBytes(points.Length));

            for (int i = 0; i < points.Length; i++)
            {
                data.AddRange(BitConverter.GetBytes(points[i].x));
                data.AddRange(BitConverter.GetBytes(points[i].y));
                data.AddRange(BitConverter.GetBytes(points[i].z));

                data.AddRange(BitConverter.GetBytes(colors[i].r));
                data.AddRange(BitConverter.GetBytes(colors[i].g));
                data.AddRange(BitConverter.GetBytes(colors[i].b));
            }

            return data.ToArray();
        }

        /// <summary>
        /// Get current point cloud
        /// </summary>
        public PointCloud GetCurrentPointCloud()
        {
            return currentPointCloud;
        }

        /// <summary>
        /// Get camera info
        /// </summary>
        public string GetCameraInfo()
        {
            return camera != null ? camera.GetInfo() : "No camera";
        }

        private void OnDestroy()
        {
            camera?.Dispose();
        }
    }

    // Enums
    public enum CameraType
    {
        RealSense,
        Kinect,
        OAK_D,
        Webcam
    }

    public enum CompressionType
    {
        Octree,
        Quantize,
        Raw
    }

    // Data structures
    [Serializable]
    public class PointCloudData
    {
        public Vector3[] points;
        public Color[] colors;
        public byte[] compressedData;
        public float timestamp;
        public int pointCount;
    }

    [Serializable]
    public class PointCloud
    {
        public Vector3[] points;
        public Color[] colors;
    }

    // Camera interface
    public interface IDepthCamera
    {
        void Initialize(Vector2Int depthRes, Vector2Int rgbRes, int fps);
        bool IsConnected();
        ushort[] CaptureDepthFrame();
        byte[] CaptureRGBFrame();
        string GetInfo();
        void Dispose();
    }

    // Mock implementations (replace with actual SDK integrations)
    public class RealSenseCamera : IDepthCamera
    {
        public void Initialize(Vector2Int depthRes, Vector2Int rgbRes, int fps)
        {
            Debug.Log("[RealSense] Initializing...");
            // TODO: Initialize RealSense SDK
        }

        public bool IsConnected() => false; // TODO: Check actual connection

        public ushort[] CaptureDepthFrame()
        {
            // TODO: Capture from RealSense
            return null;
        }

        public byte[] CaptureRGBFrame()
        {
            // TODO: Capture from RealSense
            return null;
        }

        public string GetInfo() => "Intel RealSense D435";

        public void Dispose()
        {
            // TODO: Clean up RealSense resources
        }
    }

    public class AzureKinectCamera : IDepthCamera
    {
        public void Initialize(Vector2Int depthRes, Vector2Int rgbRes, int fps)
        {
            Debug.Log("[Kinect] Initializing...");
            // TODO: Initialize Azure Kinect SDK
        }

        public bool IsConnected() => false;

        public ushort[] CaptureDepthFrame() => null;

        public byte[] CaptureRGBFrame() => null;

        public string GetInfo() => "Azure Kinect DK";

        public void Dispose() { }
    }

    public class OAKDCamera : IDepthCamera
    {
        public void Initialize(Vector2Int depthRes, Vector2Int rgbRes, int fps)
        {
            Debug.Log("[OAK-D] Initializing...");
        }

        public bool IsConnected() => false;

        public ushort[] CaptureDepthFrame() => null;

        public byte[] CaptureRGBFrame() => null;

        public string GetInfo() => "OAK-D Camera";

        public void Dispose() { }
    }

    public class WebcamFallback : IDepthCamera
    {
        private WebCamTexture webcam;

        public void Initialize(Vector2Int depthRes, Vector2Int rgbRes, int fps)
        {
            Debug.Log("[Webcam] Initializing fallback...");
            webcam = new WebCamTexture(rgbRes.x, rgbRes.y, fps);
            webcam.Play();
        }

        public bool IsConnected() => webcam != null && webcam.isPlaying;

        public ushort[] CaptureDepthFrame()
        {
            // No depth data from webcam
            return null;
        }

        public byte[] CaptureRGBFrame()
        {
            if (webcam == null || !webcam.isPlaying)
                return null;

            Color32[] pixels = webcam.GetPixels32();
            byte[] rgb = new byte[pixels.Length * 3];

            for (int i = 0; i < pixels.Length; i++)
            {
                rgb[i * 3] = pixels[i].r;
                rgb[i * 3 + 1] = pixels[i].g;
                rgb[i * 3 + 2] = pixels[i].b;
            }

            return rgb;
        }

        public string GetInfo() => "Webcam Fallback (2D)";

        public void Dispose()
        {
            if (webcam != null)
            {
                webcam.Stop();
            }
        }
    }
}
