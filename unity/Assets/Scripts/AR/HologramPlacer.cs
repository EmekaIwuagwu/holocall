/**
 * Hologram Placer - AR Foundation system for placing holograms in real world
 * Supports ARCore (Android) and ARKit (iOS)
 */

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

namespace HoloCall.AR
{
    [RequireComponent(typeof(ARRaycastManager))]
    [RequireComponent(typeof(ARAnchorManager))]
    public class HologramPlacer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARAnchorManager anchorManager;
        [SerializeField] private GameObject hologramPrefab;
        [SerializeField] private ARPlaneManager planeManager;

        [Header("Placement Settings")]
        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float maxScale = 2.0f;
        [SerializeField] private float defaultScale = 1.0f;

        // Hologram tracking
        private Dictionary<string, GameObject> participantHolograms = new Dictionary<string, GameObject>();
        private Dictionary<string, ARAnchor> participantAnchors = new Dictionary<string, ARAnchor>();

        // Placement state
        private string selectedParticipantId;
        private GameObject currentPlacementPreview;
        private bool isPlacementMode = false;

        // Touch gesture tracking
        private float initialPinchDistance;
        private float currentScale = 1.0f;

        private void Awake()
        {
            if (raycastManager == null)
                raycastManager = GetComponent<ARRaycastManager>();

            if (anchorManager == null)
                anchorManager = GetComponent<ARAnchorManager>();
        }

        private void Update()
        {
            if (isPlacementMode)
            {
                HandlePlacement();
            }
            else
            {
                HandleManipulation();
            }
        }

        /// <summary>
        /// Start placement mode for a participant
        /// </summary>
        public void StartPlacementMode(string participantId)
        {
            selectedParticipantId = participantId;
            isPlacementMode = true;

            // Create preview hologram
            if (currentPlacementPreview == null && hologramPrefab != null)
            {
                currentPlacementPreview = Instantiate(hologramPrefab);
                currentPlacementPreview.SetActive(false);
                SetPreviewAlpha(currentPlacementPreview, 0.5f);
            }

            Debug.Log($"[HologramPlacer] Started placement mode for {participantId}");
        }

        /// <summary>
        /// Handle hologram placement with touch
        /// </summary>
        private void HandlePlacement()
        {
            if (Input.touchCount == 0)
                return;

            Touch touch = Input.GetTouch(0);

            // Raycast to detect AR planes
            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                ARRaycastHit hit = hits[0];

                // Show preview at raycast position
                if (currentPlacementPreview != null)
                {
                    currentPlacementPreview.SetActive(true);
                    currentPlacementPreview.transform.position = hit.pose.position;
                    currentPlacementPreview.transform.rotation = hit.pose.rotation;
                }

                // Place on tap
                if (touch.phase == TouchPhase.Ended)
                {
                    PlaceHologram(hit.pose);
                }
            }
        }

        /// <summary>
        /// Place hologram at pose and create anchor
        /// </summary>
        private void PlaceHologram(Pose pose)
        {
            // Create anchor at hit position
            ARAnchor anchor = anchorManager.AddAnchor(pose);

            if (anchor != null)
            {
                // Remove preview
                if (currentPlacementPreview != null)
                {
                    Destroy(currentPlacementPreview);
                    currentPlacementPreview = null;
                }

                // Create actual hologram
                GameObject hologram = Instantiate(hologramPrefab, anchor.transform);
                hologram.transform.localPosition = Vector3.zero;
                hologram.transform.localRotation = Quaternion.identity;
                hologram.transform.localScale = Vector3.one * currentScale;

                // Store references
                participantHolograms[selectedParticipantId] = hologram;
                participantAnchors[selectedParticipantId] = anchor;

                // Sync anchor to network
                SyncAnchorToNetwork(selectedParticipantId, anchor, pose);

                isPlacementMode = false;
                selectedParticipantId = null;

                Debug.Log($"[HologramPlacer] Hologram placed for {selectedParticipantId}");
            }
            else
            {
                Debug.LogWarning("[HologramPlacer] Failed to create anchor");
            }
        }

        /// <summary>
        /// Handle hologram manipulation (move, rotate, scale)
        /// </summary>
        private void HandleManipulation()
        {
            if (Input.touchCount == 0)
                return;

            // Single touch: Move hologram
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Moved)
                {
                    HandleMove(touch);
                }
            }
            // Two touches: Pinch to scale or rotate
            else if (Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                {
                    initialPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                }
                else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                {
                    HandlePinchScale(touch0, touch1);
                }
            }
        }

        /// <summary>
        /// Move hologram by dragging
        /// </summary>
        private void HandleMove(Touch touch)
        {
            // Raycast to find new position
            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                ARRaycastHit hit = hits[0];

                // Find closest hologram to touch
                GameObject targetHologram = GetHologramAtScreenPoint(touch.position);

                if (targetHologram != null)
                {
                    targetHologram.transform.position = hit.pose.position;
                }
            }
        }

        /// <summary>
        /// Scale hologram with pinch gesture
        /// </summary>
        private void HandlePinchScale(Touch touch0, Touch touch1)
        {
            float currentDistance = Vector2.Distance(touch0.position, touch1.position);
            float scaleFactor = currentDistance / initialPinchDistance;

            // Find hologram at center of touches
            Vector2 centerPoint = (touch0.position + touch1.position) / 2f;
            GameObject targetHologram = GetHologramAtScreenPoint(centerPoint);

            if (targetHologram != null)
            {
                float newScale = Mathf.Clamp(
                    currentScale * scaleFactor,
                    minScale,
                    maxScale
                );

                targetHologram.transform.localScale = Vector3.one * newScale;
                currentScale = newScale;
            }

            initialPinchDistance = currentDistance;
        }

        /// <summary>
        /// Get hologram at screen point using raycast
        /// </summary>
        private GameObject GetHologramAtScreenPoint(Vector2 screenPoint)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPoint);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                // Check if hit object is a hologram
                foreach (var hologram in participantHolograms.Values)
                {
                    if (hit.collider.gameObject == hologram ||
                        hit.collider.transform.IsChildOf(hologram.transform))
                    {
                        return hologram;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sync anchor position to network for other participants
        /// </summary>
        private void SyncAnchorToNetwork(string participantId, ARAnchor anchor, Pose pose)
        {
            // In production, send anchor data via network manager
            var anchorData = new
            {
                participantId = participantId,
                position = new float[] { pose.position.x, pose.position.y, pose.position.z },
                rotation = new float[]
                {
                    pose.rotation.x, pose.rotation.y, pose.rotation.z, pose.rotation.w
                }
            };

            Debug.Log($"[HologramPlacer] Syncing anchor for {participantId}: {pose.position}");

            // TODO: Send via NetworkManager
            // NetworkManager.SendAnchorSync(anchorData);
        }

        /// <summary>
        /// Receive anchor sync from network
        /// </summary>
        public void OnAnchorSyncReceived(string participantId, Vector3 position, Quaternion rotation)
        {
            if (participantHolograms.ContainsKey(participantId))
            {
                return; // Already placed locally
            }

            // Create anchor at received position
            Pose pose = new Pose(position, rotation);
            ARAnchor anchor = anchorManager.AddAnchor(pose);

            if (anchor != null)
            {
                // Create hologram
                GameObject hologram = Instantiate(hologramPrefab, anchor.transform);
                hologram.transform.localPosition = Vector3.zero;
                hologram.transform.localRotation = Quaternion.identity;

                participantHolograms[participantId] = hologram;
                participantAnchors[participantId] = anchor;

                Debug.Log($"[HologramPlacer] Created hologram from network sync: {participantId}");
            }
        }

        /// <summary>
        /// Remove hologram for participant
        /// </summary>
        public void RemoveHologram(string participantId)
        {
            if (participantHolograms.ContainsKey(participantId))
            {
                Destroy(participantHolograms[participantId]);
                participantHolograms.Remove(participantId);
            }

            if (participantAnchors.ContainsKey(participantId))
            {
                Destroy(participantAnchors[participantId]);
                participantAnchors.Remove(participantId);
            }

            Debug.Log($"[HologramPlacer] Removed hologram for {participantId}");
        }

        /// <summary>
        /// Set preview alpha for placement
        /// </summary>
        private void SetPreviewAlpha(GameObject obj, float alpha)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    Color color = material.color;
                    color.a = alpha;
                    material.color = color;
                }
            }
        }

        /// <summary>
        /// Toggle plane visualization
        /// </summary>
        public void SetPlanesVisible(bool visible)
        {
            if (planeManager != null)
            {
                foreach (var plane in planeManager.trackables)
                {
                    plane.gameObject.SetActive(visible);
                }
            }
        }
    }
}
