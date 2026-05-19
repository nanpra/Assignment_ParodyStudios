using System.Collections.Generic;
using UnityEngine;

namespace GravityPuzzle.CameraSystem
{
    public class DynamicCameraObstacleAssigner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;

        [Header("Surface Settings")]
        [SerializeField] private List<GameObject> gravitySurfaces = new();

        [Header("Layer Names")]
        [SerializeField] private string activeGroundLayer = "Ground";
        [SerializeField] private string inactiveObstacleLayer = "CameraObstacle";

        [Header("Detection")]
        [SerializeField] private float rayDistance = 5f;
        [SerializeField] private LayerMask detectionMask = ~0;

        private GameObject currentSurface;

        private int groundLayerIndex;
        private int obstacleLayerIndex;

        private void Awake()
        {
            groundLayerIndex =
                LayerMask.NameToLayer(activeGroundLayer);

            obstacleLayerIndex =
                LayerMask.NameToLayer(inactiveObstacleLayer);

            InitializeAllSurfaces();
        }

        private void FixedUpdate()
        {
            UpdateActiveSurface();
        }

        private void InitializeAllSurfaces()
        {
            foreach (GameObject surface in gravitySurfaces)
            {
                if (surface == null)
                    continue;

                surface.layer = obstacleLayerIndex;
            }
        }

        private void UpdateActiveSurface()
        {
            if (player == null)
                return;

            Vector3 rayOrigin =
                player.position;

            Vector3 rayDirection =
                -player.up;

            if (!Physics.Raycast(
                    rayOrigin,
                    rayDirection,
                    out RaycastHit hit,
                    rayDistance,
                    detectionMask,
                    QueryTriggerInteraction.Ignore))
            {
                return;
            }

            GameObject hitObject =
                hit.collider.gameObject;

            if (!gravitySurfaces.Contains(hitObject))
                return;

            if (currentSurface == hitObject)
                return;

            // Reset all surfaces to CameraObstacle
            foreach (GameObject surface in gravitySurfaces)
            {
                if (surface == null)
                    continue;

                surface.layer = obstacleLayerIndex;
            }

            // Set current walked surface to Ground
            hitObject.layer = groundLayerIndex;

            currentSurface = hitObject;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (player == null)
                return;

            Gizmos.color = Color.cyan;

            Gizmos.DrawRay(
                player.position,
                -player.up * rayDistance);
        }
#endif
    }
}