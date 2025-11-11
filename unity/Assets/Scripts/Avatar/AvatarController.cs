/**
 * Avatar Controller - Handles avatar animation with face tracking
 * Supports ARKit (iOS) and ARCore (Android) face tracking
 */

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

namespace HoloCall.Avatar
{
    [RequireComponent(typeof(ARFaceManager))]
    public class AvatarController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ARFaceManager faceManager;
        [SerializeField] private SkinnedMeshRenderer avatarFaceRenderer;
        [SerializeField] private Transform avatarHead;

        [Header("Settings")]
        [SerializeField] private bool enableFaceTracking = true;
        [SerializeField] private bool enableLipSync = true;
        [SerializeField] private float blendShapeMultiplier = 1.0f;

        // Tracked face
        private ARFace trackedFace;

        // Blendshape mapping (ARKit/ARCore -> Avatar)
        private Dictionary<string, string> blendShapeMap;

        // Audio-driven lipsync (fallback)
        private float lipSyncValue = 0f;

        private void Awake()
        {
            if (faceManager == null)
                faceManager = GetComponent<ARFaceManager>();

            InitializeBlendShapeMapping();
        }

        private void OnEnable()
        {
            if (faceManager != null)
            {
                faceManager.facesChanged += OnFacesChanged;
            }
        }

        private void OnDisable()
        {
            if (faceManager != null)
            {
                faceManager.facesChanged -= OnFacesChanged;
            }
        }

        private void Update()
        {
            if (enableFaceTracking && trackedFace != null)
            {
                UpdateAvatarFromFaceTracking();
            }
            else if (enableLipSync)
            {
                UpdateAudioLipSync();
            }
        }

        /// <summary>
        /// Handle face tracking updates
        /// </summary>
        private void OnFacesChanged(ARFacesChangedEventArgs args)
        {
            // Use first detected face
            if (args.added.Count > 0)
            {
                trackedFace = args.added[0];
                Debug.Log("[AvatarController] Face tracking started");
            }

            if (args.removed.Count > 0 && trackedFace != null)
            {
                if (args.removed.Contains(trackedFace))
                {
                    trackedFace = null;
                    Debug.Log("[AvatarController] Face tracking lost");
                }
            }
        }

        /// <summary>
        /// Update avatar from face tracking data
        /// </summary>
        private void UpdateAvatarFromFaceTracking()
        {
            if (trackedFace == null || avatarFaceRenderer == null)
                return;

            // Get blendshape coefficients from ARFace
            using (var blendShapes = trackedFace.GetBlendShapeCoefficients(Allocator.Temp))
            {
                foreach (var kvp in blendShapes)
                {
                    string blendShapeName = kvp.Key.ToString();
                    float coefficient = kvp.Value;

                    // Apply to avatar if mapping exists
                    if (blendShapeMap.ContainsKey(blendShapeName))
                    {
                        string avatarBlendShapeName = blendShapeMap[blendShapeName];
                        int blendShapeIndex = avatarFaceRenderer.sharedMesh.GetBlendShapeIndex(avatarBlendShapeName);

                        if (blendShapeIndex >= 0)
                        {
                            avatarFaceRenderer.SetBlendShapeWeight(
                                blendShapeIndex,
                                coefficient * 100f * blendShapeMultiplier
                            );
                        }
                    }
                }
            }

            // Update head pose
            if (avatarHead != null)
            {
                Pose headPose = trackedFace.transform.GetLocalPose();
                avatarHead.localPosition = headPose.position;
                avatarHead.localRotation = headPose.rotation;
            }

            // Send avatar state to network
            SendAvatarStateToNetwork();
        }

        /// <summary>
        /// Audio-driven lipsync fallback
        /// </summary>
        private void UpdateAudioLipSync()
        {
            if (avatarFaceRenderer == null)
                return;

            // Get audio level from microphone
            // This is a simplified version - in production, use proper audio analysis
            lipSyncValue = GetAudioLevel();

            // Apply to jaw and mouth blendshapes
            int jawOpenIndex = avatarFaceRenderer.sharedMesh.GetBlendShapeIndex("jawOpen");
            int mouthOpenIndex = avatarFaceRenderer.sharedMesh.GetBlendShapeIndex("mouthOpen");

            if (jawOpenIndex >= 0)
            {
                avatarFaceRenderer.SetBlendShapeWeight(jawOpenIndex, lipSyncValue * 100f);
            }

            if (mouthOpenIndex >= 0)
            {
                avatarFaceRenderer.SetBlendShapeWeight(mouthOpenIndex, lipSyncValue * 80f);
            }
        }

        /// <summary>
        /// Get current audio level for lipsync
        /// </summary>
        private float GetAudioLevel()
        {
            // In production, analyze microphone input
            // For now, return random value for demo
            return Random.Range(0f, 0.5f);
        }

        /// <summary>
        /// Send avatar state to network
        /// </summary>
        private void SendAvatarStateToNetwork()
        {
            if (trackedFace == null)
                return;

            // Get current blendshape values
            float[] blendShapeValues = new float[52];

            using (var blendShapes = trackedFace.GetBlendShapeCoefficients(Allocator.Temp))
            {
                int index = 0;
                foreach (var kvp in blendShapes)
                {
                    if (index < 52)
                    {
                        blendShapeValues[index] = kvp.Value;
                        index++;
                    }
                }
            }

            // Get head pose
            Pose headPose = trackedFace.transform.GetLocalPose();

            // Create avatar state message
            var avatarState = new AvatarState
            {
                headPosition = headPose.position,
                headRotation = headPose.rotation,
                blendShapes = QuantizeBlendShapes(blendShapeValues),
                timestamp = Time.time
            };

            // Send via DataChannel (low latency)
            // TODO: Integrate with NetworkManager
            Debug.Log($"[AvatarController] Sending avatar state: {blendShapeValues.Length} blendshapes");
        }

        /// <summary>
        /// Quantize blendshapes to reduce bandwidth (float -> byte)
        /// </summary>
        private byte[] QuantizeBlendShapes(float[] shapes)
        {
            byte[] quantized = new byte[shapes.Length];
            for (int i = 0; i < shapes.Length; i++)
            {
                quantized[i] = (byte)(Mathf.Clamp01(shapes[i]) * 255);
            }
            return quantized;
        }

        /// <summary>
        /// Receive and apply avatar state from network
        /// </summary>
        public void ApplyNetworkAvatarState(AvatarState state)
        {
            if (avatarFaceRenderer == null)
                return;

            // Apply blendshapes
            float[] dequantized = DequantizeBlendShapes(state.blendShapes);

            for (int i = 0; i < dequantized.Length && i < avatarFaceRenderer.sharedMesh.blendShapeCount; i++)
            {
                avatarFaceRenderer.SetBlendShapeWeight(i, dequantized[i] * 100f);
            }

            // Apply head pose
            if (avatarHead != null)
            {
                avatarHead.localPosition = state.headPosition;
                avatarHead.localRotation = state.headRotation;
            }
        }

        /// <summary>
        /// Dequantize blendshapes (byte -> float)
        /// </summary>
        private float[] DequantizeBlendShapes(byte[] quantized)
        {
            float[] shapes = new float[quantized.Length];
            for (int i = 0; i < quantized.Length; i++)
            {
                shapes[i] = quantized[i] / 255f;
            }
            return shapes;
        }

        /// <summary>
        /// Initialize blendshape mapping between ARKit/ARCore and avatar
        /// </summary>
        private void InitializeBlendShapeMapping()
        {
            blendShapeMap = new Dictionary<string, string>
            {
                // ARKit -> Avatar blendshape mapping
                // Eyes
                { "EyeBlinkLeft", "eyeBlinkLeft" },
                { "EyeBlinkRight", "eyeBlinkRight" },
                { "EyeLookUpLeft", "eyeLookUpLeft" },
                { "EyeLookUpRight", "eyeLookUpRight" },
                { "EyeLookDownLeft", "eyeLookDownLeft" },
                { "EyeLookDownRight", "eyeLookDownRight" },
                { "EyeLookInLeft", "eyeLookInLeft" },
                { "EyeLookInRight", "eyeLookInRight" },
                { "EyeLookOutLeft", "eyeLookOutLeft" },
                { "EyeLookOutRight", "eyeLookOutRight" },

                // Jaw
                { "JawOpen", "jawOpen" },
                { "JawForward", "jawForward" },
                { "JawLeft", "jawLeft" },
                { "JawRight", "jawRight" },

                // Mouth
                { "MouthSmileLeft", "mouthSmileLeft" },
                { "MouthSmileRight", "mouthSmileRight" },
                { "MouthFrownLeft", "mouthFrownLeft" },
                { "MouthFrownRight", "mouthFrownRight" },
                { "MouthPucker", "mouthPucker" },
                { "MouthFunnel", "mouthFunnel" },
                { "MouthLeft", "mouthLeft" },
                { "MouthRight", "mouthRight" },

                // Eyebrows
                { "BrowInnerUp", "browInnerUp" },
                { "BrowDownLeft", "browDownLeft" },
                { "BrowDownRight", "browDownRight" },
                { "BrowOuterUpLeft", "browOuterUpLeft" },
                { "BrowOuterUpRight", "browOuterUpRight" },

                // Cheeks
                { "CheekPuff", "cheekPuff" },
                { "CheekSquintLeft", "cheekSquintLeft" },
                { "CheekSquintRight", "cheekSquintRight" }

                // Add more mappings as needed (52 total for ARKit)
            };
        }

        /// <summary>
        /// Set avatar face renderer
        /// </summary>
        public void SetAvatarRenderer(SkinnedMeshRenderer renderer)
        {
            avatarFaceRenderer = renderer;
        }

        /// <summary>
        /// Set avatar head transform
        /// </summary>
        public void SetAvatarHead(Transform head)
        {
            avatarHead = head;
        }
    }

    // Avatar state data structure
    [System.Serializable]
    public class AvatarState
    {
        public Vector3 headPosition;
        public Quaternion headRotation;
        public byte[] blendShapes; // 52 bytes for ARKit
        public float timestamp;
    }
}
