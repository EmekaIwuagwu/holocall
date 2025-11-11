/**
 * Network Manager - Handles WebSocket connection and signaling
 * Cross-platform WebRTC peer connection management
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using WebSocketSharp;
using HoloCall.Core;

namespace HoloCall.Core
{
    public class NetworkManager : MonoBehaviour
    {
        private WebSocket webSocket;
        private string authToken;
        private Dictionary<string, PeerConnection> peerConnections = new Dictionary<string, PeerConnection>();

        // Events
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnError;
        public event Action<Participant> OnParticipantJoined;
        public event Action<string> OnParticipantLeft;

        /// <summary>
        /// Initialize network manager with signaling server URL
        /// </summary>
        public void Initialize(string signalingUrl)
        {
            Debug.Log($"[NetworkManager] Initializing with URL: {signalingUrl}");
        }

        /// <summary>
        /// Set authentication token
        /// </summary>
        public void SetAuthToken(string token)
        {
            authToken = token;
            ConnectToSignaling();
        }

        /// <summary>
        /// Connect to signaling server
        /// </summary>
        private void ConnectToSignaling()
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("[NetworkManager] Cannot connect without auth token");
                return;
            }

            var wsUrl = HoloCallManager.Instance != null
                ? $"ws://localhost:8080/ws?token={authToken}"
                : $"ws://localhost:8080/ws?token={authToken}";

            webSocket = new WebSocket(wsUrl);

            webSocket.OnOpen += (sender, e) =>
            {
                Debug.Log("[NetworkManager] WebSocket connected");
                OnConnected?.Invoke();
            };

            webSocket.OnMessage += (sender, e) =>
            {
                HandleSignalingMessage(e.Data);
            };

            webSocket.OnError += (sender, e) =>
            {
                Debug.LogError($"[NetworkManager] WebSocket error: {e.Message}");
                OnError?.Invoke(e.Message);
            };

            webSocket.OnClose += (sender, e) =>
            {
                Debug.Log("[NetworkManager] WebSocket disconnected");
                OnDisconnected?.Invoke();
            };

            webSocket.Connect();
        }

        /// <summary>
        /// Create room
        /// </summary>
        public void CreateRoom(
            PlatformType platform,
            PlatformCapabilities capabilities,
            string displayName,
            Action<string> onSuccess,
            Action<string> onError)
        {
            var message = new
            {
                type = "create_room",
                platform = platform.ToString().ToLower(),
                displayName = displayName,
                capabilities = capabilities
            };

            SendMessage(message);

            // Store callback for when room_created message arrives
            // In production, use a proper callback system
        }

        /// <summary>
        /// Join room
        /// </summary>
        public void JoinRoom(
            string roomId,
            PlatformType platform,
            PlatformCapabilities capabilities,
            string displayName,
            Action onSuccess,
            Action<string> onError)
        {
            var message = new
            {
                type = "join_room",
                roomId = roomId,
                platform = platform.ToString().ToLower(),
                displayName = displayName,
                capabilities = capabilities
            };

            SendMessage(message);
        }

        /// <summary>
        /// Leave room
        /// </summary>
        public void LeaveRoom(string roomId)
        {
            var message = new
            {
                type = "leave_room",
                roomId = roomId
            };

            SendMessage(message);

            // Close all peer connections
            foreach (var peer in peerConnections.Values)
            {
                peer.Close();
            }
            peerConnections.Clear();
        }

        /// <summary>
        /// Handle incoming signaling messages
        /// </summary>
        private void HandleSignalingMessage(string data)
        {
            try
            {
                var message = JsonUtility.FromJson<SignalingMessage>(data);

                Debug.Log($"[NetworkManager] Received message: {message.type}");

                switch (message.type)
                {
                    case "room_created":
                        HandleRoomCreated(data);
                        break;

                    case "joined_room":
                        HandleJoinedRoom(data);
                        break;

                    case "participant_joined":
                        HandleParticipantJoined(data);
                        break;

                    case "participant_left":
                        HandleParticipantLeft(data);
                        break;

                    case "sdp_offer":
                        HandleSDPOffer(data);
                        break;

                    case "sdp_answer":
                        HandleSDPAnswer(data);
                        break;

                    case "ice_candidate":
                        HandleICECandidate(data);
                        break;

                    case "communication_mode":
                        HandleCommunicationMode(data);
                        break;

                    case "error":
                        HandleError(data);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NetworkManager] Error handling message: {ex.Message}");
            }
        }

        private void HandleRoomCreated(string data)
        {
            var response = JsonUtility.FromJson<RoomCreatedMessage>(data);
            Debug.Log($"[NetworkManager] Room created: {response.roomId}");
        }

        private void HandleJoinedRoom(string data)
        {
            var response = JsonUtility.FromJson<JoinedRoomMessage>(data);
            Debug.Log($"[NetworkManager] Joined room: {response.roomId}");

            // Create peer connections for existing participants
            foreach (var participant in response.participants)
            {
                CreatePeerConnection(participant);
            }
        }

        private void HandleParticipantJoined(string data)
        {
            var message = JsonUtility.FromJson<ParticipantJoinedMessage>(data);
            Debug.Log($"[NetworkManager] Participant joined: {message.participant.userId}");

            CreatePeerConnection(message.participant);
            OnParticipantJoined?.Invoke(message.participant);
        }

        private void HandleParticipantLeft(string data)
        {
            var message = JsonUtility.FromJson<ParticipantLeftMessage>(data);
            Debug.Log($"[NetworkManager] Participant left: {message.userId}");

            if (peerConnections.ContainsKey(message.userId))
            {
                peerConnections[message.userId].Close();
                peerConnections.Remove(message.userId);
            }

            OnParticipantLeft?.Invoke(message.userId);
        }

        private void HandleSDPOffer(string data)
        {
            Debug.Log("[NetworkManager] Received SDP offer");
            // Handle WebRTC SDP offer
            // In production, use Unity WebRTC plugin
        }

        private void HandleSDPAnswer(string data)
        {
            Debug.Log("[NetworkManager] Received SDP answer");
            // Handle WebRTC SDP answer
        }

        private void HandleICECandidate(string data)
        {
            Debug.Log("[NetworkManager] Received ICE candidate");
            // Handle WebRTC ICE candidate
        }

        private void HandleCommunicationMode(string data)
        {
            var message = JsonUtility.FromJson<CommunicationModeMessage>(data);
            Debug.Log($"[NetworkManager] Communication mode with {message.withParticipant}: {message.mode}");
        }

        private void HandleError(string data)
        {
            var message = JsonUtility.FromJson<ErrorMessage>(data);
            Debug.LogError($"[NetworkManager] Server error: {message.message}");
            OnError?.Invoke(message.message);
        }

        /// <summary>
        /// Create peer connection for participant
        /// </summary>
        private void CreatePeerConnection(Participant participant)
        {
            if (peerConnections.ContainsKey(participant.userId))
            {
                return;
            }

            var peerConnection = new PeerConnection(participant);
            peerConnections[participant.userId] = peerConnection;

            Debug.Log($"[NetworkManager] Created peer connection for {participant.userId}");
        }

        /// <summary>
        /// Send message to signaling server
        /// </summary>
        private void SendMessage(object message)
        {
            if (webSocket != null && webSocket.ReadyState == WebSocketState.Open)
            {
                var json = JsonUtility.ToJson(message);
                webSocket.Send(json);
            }
            else
            {
                Debug.LogWarning("[NetworkManager] Cannot send message: WebSocket not connected");
            }
        }

        private void OnDestroy()
        {
            if (webSocket != null && webSocket.ReadyState == WebSocketState.Open)
            {
                webSocket.Close();
            }
        }

        // Message classes
        [Serializable]
        private class SignalingMessage
        {
            public string type;
        }

        [Serializable]
        private class RoomCreatedMessage
        {
            public string type;
            public string roomId;
            public int maxParticipants;
        }

        [Serializable]
        private class JoinedRoomMessage
        {
            public string type;
            public string roomId;
            public Participant[] participants;
        }

        [Serializable]
        private class ParticipantJoinedMessage
        {
            public string type;
            public Participant participant;
        }

        [Serializable]
        private class ParticipantLeftMessage
        {
            public string type;
            public string userId;
        }

        [Serializable]
        private class CommunicationModeMessage
        {
            public string type;
            public string withParticipant;
            public string mode;
        }

        [Serializable]
        private class ErrorMessage
        {
            public string type;
            public string code;
            public string message;
        }
    }

    // Participant class
    [Serializable]
    public class Participant
    {
        public string userId;
        public string displayName;
        public string platform;
        public PlatformCapabilities capabilities;
        public bool isHost;
        public bool isMuted;
        public bool isCameraOff;
    }

    // Peer connection wrapper
    public class PeerConnection
    {
        private Participant participant;

        public PeerConnection(Participant participant)
        {
            this.participant = participant;
        }

        public void Close()
        {
            // Close WebRTC peer connection
            Debug.Log($"[PeerConnection] Closing connection to {participant.userId}");
        }
    }
}
