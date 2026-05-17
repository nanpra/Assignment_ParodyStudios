using System;
using UnityEngine;

namespace GravityPuzzle.Gravity
{
    public class GravityManager : MonoBehaviour
    {
        public static GravityManager Instance;

        // EVENTS
        public static event Action OnGravityUp;
        public static event Action OnGravityDown;
        public static event Action OnGravityLeft;
        public static event Action OnGravityRight;
        public static event Action OnGravityConfirm;

        public Vector3 CurrentGravity { get; private set; }
            = Vector3.down;

        [Header("Smoothing")]
        [SerializeField] private float gravityTransitionSpeed = 8f;

        private Vector3 targetGravity = Vector3.down;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            ApplyGravity(GravityDirection.Down);
        }

        private void Update()
        {
            HandleGravityInput();
            SmoothGravity();
        }

        private void HandleGravityInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
                OnGravityUp?.Invoke();

            else if (Input.GetKeyDown(KeyCode.DownArrow))
                OnGravityDown?.Invoke();

            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                OnGravityLeft?.Invoke();

            else if (Input.GetKeyDown(KeyCode.RightArrow))
                OnGravityRight?.Invoke();

            else if (Input.GetKeyDown(KeyCode.Return))
                OnGravityConfirm?.Invoke();
        }

        public void ApplyGravity(GravityDirection direction)
        {
            ApplyGravity(DirectionToVector(direction));
        }

        public void ApplyGravity(Vector3 gravityVector)
        {
            if (gravityVector.sqrMagnitude < 0.0001f)
                return;

            targetGravity = gravityVector.normalized;
        }

        private void SmoothGravity()
        {
            if (Vector3.Dot(CurrentGravity, targetGravity) > 0.9999f)
            {
                CurrentGravity = targetGravity;
                return;
            }

            CurrentGravity = Vector3.Slerp(
                CurrentGravity,
                targetGravity,
                gravityTransitionSpeed * Time.deltaTime).normalized;
        }

        public static GravityDirection VectorToDirection(Vector3 gravityVector)
        {
            Vector3 normalized = gravityVector.normalized;
            float bestDot = float.NegativeInfinity;
            GravityDirection bestDirection = GravityDirection.Down;

            foreach (GravityDirection direction in Enum.GetValues(typeof(GravityDirection)))
            {
                float dot = Vector3.Dot(normalized, DirectionToVector(direction));

                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestDirection = direction;
                }
            }

            return bestDirection;
        }

        private static Vector3 DirectionToVector(GravityDirection direction)
        {
            return direction switch
            {
                GravityDirection.Down => Vector3.down,
                GravityDirection.Up => Vector3.up,
                GravityDirection.Left => Vector3.left,
                GravityDirection.Right => Vector3.right,
                GravityDirection.Forward => Vector3.forward,
                GravityDirection.Backward => Vector3.back,
                _ => Vector3.down,
            };
        }
    }
}
