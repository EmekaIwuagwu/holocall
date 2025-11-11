/**
 * HoloCall Manager - Main entry point for HoloCall Unity client
 * Manages application lifecycle and coordinates all subsystems
 */

using UnityEngine;
using System;

namespace HoloCall.Core
{
    public class HoloCallManager : MonoBehaviour
    {
        // Singleton instance
        public static HoloCallManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private string signalingServerUrl = "ws://localhost:8080/ws";
        [SerializeField] private PlatformType platform = PlatformType.Desktop;

        [Header("References")]
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private GameObject avatarSystemPrefab;
        [SerializeField] private GameObject captureSystemPrefab;

        // Current user info
        public string UserId { get; private set; }
        public string DisplayName { get; private set; }
        public string CurrentRoomId { get; private set; }

        // Events
        public event Action OnInitialized;
        public event Action<string> OnRoomJoined;
        public event Action OnRoomLeft;
        public event Action<string> OnError;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            DetectPlatform();
        }

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize HoloCall system
        /// </summary>
        public void Initialize()
        {
            Debug.Log($"[HoloCall] Initializing for platform: {platform}");

            // Initialize network manager
            if (networkManager == null)
            {
                networkManager = gameObject.AddComponent<NetworkManager>();
            }

            networkManager.Initialize(signalingServerUrl);

            // Platform-specific initialization
            InitializePlatformSpecificSystems();

            OnInitialized?.Invoke();
            Debug.Log("[HoloCall] Initialization complete");
        }

        /// <summary>
        /// Authenticate user and get JWT token
        /// </summary>
        public void Authenticate(string email, string displayName, Action<bool, string> callback)
        {
            DisplayName = displayName;

            // Call backend API to get JWT token
            var authUrl = signalingServerUrl.Replace("ws://", "http://").Replace("/ws", "/api/auth/login");

            StartCoroutine(AuthenticateCoroutine(authUrl, email, displayName, (success, token, userId) =>
            {
                if (success)
                {
                    UserId = userId;
                    networkManager.SetAuthToken(token);
                    callback?.Invoke(true, "Authentication successful");
                }
                else
                {
                    callback?.Invoke(false, "Authentication failed");
                }
            }));
        }

        private System.Collections.IEnumerator AuthenticateCoroutine(
            string url,
            string email,
            string displayName,
            Action<bool, string, string> callback)
        {
            using (UnityEngine.Networking.UnityWebRequest request = new UnityEngine.Networking.UnityWebRequest(url, "POST"))
            {
                var json = $"{{\"email\":\"{email}\",\"displayName\":\"{displayName}\"}}";
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

                request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
                    if (response.success)
                    {
                        callback?.Invoke(true, response.data.token, response.data.user.userId);
                    }
                    else
                    {
                        callback?.Invoke(false, "", "");
                    }
                }
                else
                {
                    callback?.Invoke(false, "", "");
                }
            }
        }

        /// <summary>
        /// Create a new room
        /// </summary>
        public void CreateRoom(Action<string> onSuccess, Action<string> onError)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                onError?.Invoke("Not authenticated");
                return;
            }

            networkManager.CreateRoom(platform, GetPlatformCapabilities(), DisplayName, (roomId) =>
            {
                CurrentRoomId = roomId;
                OnRoomJoined?.Invoke(roomId);
                onSuccess?.Invoke(roomId);
            }, onError);
        }

        /// <summary>
        /// Join an existing room
        /// </summary>
        public void JoinRoom(string roomId, Action onSuccess, Action<string> onError)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                onError?.Invoke("Not authenticated");
                return;
            }

            networkManager.JoinRoom(roomId, platform, GetPlatformCapabilities(), DisplayName, () =>
            {
                CurrentRoomId = roomId;
                OnRoomJoined?.Invoke(roomId);
                onSuccess?.Invoke();
            }, onError);
        }

        /// <summary>
        /// Leave current room
        /// </summary>
        public void LeaveRoom()
        {
            if (!string.IsNullOrEmpty(CurrentRoomId))
            {
                networkManager.LeaveRoom(CurrentRoomId);
                CurrentRoomId = null;
                OnRoomLeft?.Invoke();
            }
        }

        /// <summary>
        /// Detect current platform
        /// </summary>
        private void DetectPlatform()
        {
#if UNITY_EDITOR
            platform = PlatformType.Desktop;
#elif UNITY_STANDALONE
            platform = PlatformType.Desktop;
#elif UNITY_ANDROID
            platform = PlatformType.Android;
#elif UNITY_IOS
            platform = PlatformType.iOS;
#elif UNITY_VR || OCULUS
            platform = PlatformType.VR;
#else
            platform = PlatformType.Desktop;
#endif
            Debug.Log($"[HoloCall] Platform detected: {platform}");
        }

        /// <summary>
        /// Initialize platform-specific systems
        /// </summary>
        private void InitializePlatformSpecificSystems()
        {
            switch (platform)
            {
                case PlatformType.Desktop:
                    InitializeDesktopSystems();
                    break;
                case PlatformType.Android:
                case PlatformType.iOS:
                    InitializeMobileSystems();
                    break;
                case PlatformType.VR:
                    InitializeVRSystems();
                    break;
            }
        }

        private void InitializeDesktopSystems()
        {
            Debug.Log("[HoloCall] Initializing desktop systems");
            // Initialize depth camera capture
            // Initialize point cloud renderer
        }

        private void InitializeMobileSystems()
        {
            Debug.Log("[HoloCall] Initializing mobile systems");
            // Initialize AR Foundation
            // Initialize face tracking
            // Initialize avatar system
        }

        private void InitializeVRSystems()
        {
            Debug.Log("[HoloCall] Initializing VR systems");
            // Initialize XR SDK
            // Initialize hand tracking
            // Initialize avatar system
        }

        /// <summary>
        /// Get platform capabilities
        /// </summary>
        private PlatformCapabilities GetPlatformCapabilities()
        {
            var capabilities = new PlatformCapabilities();

            switch (platform)
            {
                case PlatformType.Desktop:
                    capabilities.volumetric = true;
                    capabilities.avatar = false;
                    capabilities.ar = false;
                    capabilities.vr = false;
                    capabilities.screenShare = true;
                    capabilities.faceTracking = false;
                    capabilities.handTracking = false;
                    capabilities.spatialAudio = true;
                    break;

                case PlatformType.Android:
                case PlatformType.iOS:
                    capabilities.volumetric = false;
                    capabilities.avatar = true;
                    capabilities.ar = true;
                    capabilities.vr = false;
                    capabilities.screenShare = false;
                    capabilities.faceTracking = true;
                    capabilities.handTracking = false;
                    capabilities.spatialAudio = true;
                    break;

                case PlatformType.VR:
                    capabilities.volumetric = false;
                    capabilities.avatar = true;
                    capabilities.ar = false;
                    capabilities.vr = true;
                    capabilities.screenShare = false;
                    capabilities.faceTracking = true;
                    capabilities.handTracking = true;
                    capabilities.spatialAudio = true;
                    break;
            }

            return capabilities;
        }

        private void OnApplicationQuit()
        {
            LeaveRoom();
        }

        // Helper classes for JSON parsing
        [Serializable]
        private class AuthResponse
        {
            public bool success;
            public AuthData data;
        }

        [Serializable]
        private class AuthData
        {
            public string token;
            public UserData user;
        }

        [Serializable]
        private class UserData
        {
            public string userId;
            public string email;
            public string displayName;
        }
    }

    // Enums and data structures
    public enum PlatformType
    {
        Desktop,
        Android,
        iOS,
        VR
    }

    [Serializable]
    public class PlatformCapabilities
    {
        public bool volumetric;
        public bool avatar;
        public bool ar;
        public bool vr;
        public bool screenShare;
        public bool faceTracking;
        public bool handTracking;
        public bool spatialAudio;
    }
}
