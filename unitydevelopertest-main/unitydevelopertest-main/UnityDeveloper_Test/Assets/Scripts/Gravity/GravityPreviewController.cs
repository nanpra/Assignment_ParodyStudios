using DG.Tweening;
using UnityEngine;

namespace GravityPuzzle.Gravity
{
    public class GravityPreviewController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform headPoint;
        [SerializeField] private GameObject hologramPrefab;

        [Header("Animation")]
        [SerializeField] private float rotateDuration = 0.3f;

        private Transform hologramFootPivot;
        private Transform hologramRoot;

        private Vector3 selectedGravityVector;
        private Camera mainCam;
        private bool hasSelection;

        // Cached preview values
        private Quaternion previewRotation;
        private Vector3 localHeadOffsetFromFoot;

        private void Awake()
        {
            GameObject instance =
                Instantiate(hologramPrefab);

            hologramFootPivot =
                new GameObject("GravityHologramFootPivot").transform;

            hologramRoot = instance.transform;
            hologramRoot.SetParent(hologramFootPivot, false);

            mainCam = Camera.main;

            hologramFootPivot.gameObject.SetActive(false);
        }

        private void Start()
        {
            if (GravityManager.Instance == null)
                return;

            previewRotation = transform.rotation;
        }

        private void LateUpdate()
        {
            if (hologramFootPivot == null || !hasSelection)
                return;

            UpdateHologramPlacement();
        }

        private void UpdateHologramPlacement()
        {
            if (headPoint == null)
                return;

            // Keep the hologram head pinned to the player's head,
            // while the root keeps rotating around the hologram feet pivot.
            Vector3 worldHeadOffset =
                hologramFootPivot.rotation * localHeadOffsetFromFoot;

            hologramFootPivot.position =
                headPoint.position - worldHeadOffset;
        }

        private void OnEnable()
        {
            GravityManager.OnGravityUp += HandleGravityUp;
            GravityManager.OnGravityDown += HandleGravityDown;
            GravityManager.OnGravityLeft += HandleGravityLeft;
            GravityManager.OnGravityRight += HandleGravityRight;
            GravityManager.OnGravityConfirm += ConfirmGravity;
        }

        private void OnDisable()
        {
            GravityManager.OnGravityUp -= HandleGravityUp;
            GravityManager.OnGravityDown -= HandleGravityDown;
            GravityManager.OnGravityLeft -= HandleGravityLeft;
            GravityManager.OnGravityRight -= HandleGravityRight;
            GravityManager.OnGravityConfirm -= ConfirmGravity;
        }

        // INPUT CALLBACKS
        private void HandleGravityUp()
        {
            if (mainCam == null)
                return;

            PreviewGravity(mainCam.transform.forward);
        }

        private void HandleGravityDown()
        {
            if (mainCam == null)
                return;

            PreviewGravity(-mainCam.transform.forward);
        }

        private void HandleGravityLeft()
        {
            if (mainCam == null)
                return;

            PreviewGravity(-mainCam.transform.right);
        }

        private void HandleGravityRight()
        {
            if (mainCam == null)
                return;

            PreviewGravity(mainCam.transform.right);
        }

        private void PreviewGravity(
            Vector3 desiredViewDirection)
        {
            if (GravityManager.Instance == null)
                return;

            Vector3 currentGravity =
                GravityManager.Instance.CurrentGravity.normalized;

            Vector3 projectedDirection =
                Vector3.ProjectOnPlane(
                    desiredViewDirection,
                    currentGravity).normalized;

            if (projectedDirection.sqrMagnitude < 0.0001f)
                return;

            selectedGravityVector =
                SnapToNearestAxis(projectedDirection);

            // Hologram "up" is opposite to selected gravity direction.
            Vector3 targetUp = -selectedGravityVector;

            previewRotation =
                Quaternion.FromToRotation(
                    transform.up,
                    targetUp) * transform.rotation;

            if (!hasSelection)
            {
                hasSelection = true;
                hologramFootPivot.gameObject.SetActive(true);
                UpdateHologramPlacement();
                hologramFootPivot.rotation = previewRotation;
            }

            hologramFootPivot
                .DOKill();

            hologramFootPivot
                .DORotateQuaternion(
                    previewRotation,
                    rotateDuration)
                .SetEase(Ease.OutSine);
        }

        private static Vector3 SnapToNearestAxis(
            Vector3 direction)
        {
            Vector3[] axes =
            {
                Vector3.left,
                Vector3.right,
                Vector3.up,
                Vector3.down,
                Vector3.forward,
                Vector3.back
            };

            Vector3 normalized =
                direction.normalized;

            float bestDot = float.NegativeInfinity;
            Vector3 bestAxis = Vector3.down;

            for (int i = 0; i < axes.Length; i++)
            {
                float dot = Vector3.Dot(
                    normalized,
                    axes[i]);

                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestAxis = axes[i];
                }
            }

            return bestAxis;
        }

        private void ConfirmGravity()
        {
            if (!hasSelection)
                return;

            GravityManager.Instance.ApplyGravity(
                selectedGravityVector);

            hologramFootPivot.DOKill();
            hologramFootPivot.gameObject.SetActive(false);
            hasSelection = false;
        }
    }
}
