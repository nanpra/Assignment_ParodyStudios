using GravityPuzzle.Core;
using GravityPuzzle.Gravity;
using UnityEngine;

namespace GravityPuzzle.Player
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float jumpForce = 6f;
        [SerializeField] private float jumpCooldown = 0.35f;

        [Header("Gravity")]
        [SerializeField] private float gravityForce = 25f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckDistance = 0.3f;

        [Header("Free Fall")]
        [SerializeField] private float maxAirTimeBeforeDeath = 2f;

        private Rigidbody body;
        private CapsuleCollider capsule;
        private PlayerAnimationController animationController;
        private Camera mainCam;

        private Vector2 moveInput;
        private bool jumpRequested;

        private Vector3 defaultForwardDirection;
        private Vector3 currentFacingDirection;
        private Vector3 previousGravityDirection;
        private Vector3 gravityDirection;
        private Vector3 moveDirection;
        private Vector3 velocity;

        private float nextJumpAllowedTime;
        private float airTimer;
        private bool isGrounded;

        public bool IsGrounded => isGrounded;

        private void Awake()
        {
            Instance = this;

            body = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();
            animationController = GetComponent<PlayerAnimationController>();
            mainCam = Camera.main;

            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;

            defaultForwardDirection = transform.forward.normalized;
            if (defaultForwardDirection.sqrMagnitude < 0.0001f)
                defaultForwardDirection = Vector3.forward;

            currentFacingDirection = defaultForwardDirection;
            previousGravityDirection = gravityDirection;
        }

        private void Update()
        {
            ReadInput();

            if (Input.GetKeyDown(KeyCode.Space))
                jumpRequested = true;

            animationController.UpdateAnimationState(
                moveDirection,
                velocity,
                isGrounded,
                gravityDirection);
        }

        private void FixedUpdate()
        {
            UpdateGravityDirection();
            HandleGroundCheck();
            HandleGravityAlignmentAndFacing();
            HandleJump();
            HandleGravity();
            HandleMovement();
            HandleFreeFall();

            velocity = body.linearVelocity;
        }

        private void ReadInput()
        {
            moveInput.x = 0f;
            moveInput.y = 0f;

            if (Input.GetKey(KeyCode.A))
                moveInput.x = -1f;
            else if (Input.GetKey(KeyCode.D))
                moveInput.x = 1f;

            if (Input.GetKey(KeyCode.S))
                moveInput.y = -1f;
            else if (Input.GetKey(KeyCode.W))
                moveInput.y = 1f;
        }

        private void UpdateGravityDirection()
        {
            gravityDirection =
                GravityManager.Instance.CurrentGravity.normalized;
        }

        private void HandleGroundCheck()
        {
            Vector3 castOrigin =
                transform.TransformPoint(capsule.center);

            float horizontalScale =
                Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));

            float verticalScale =
                Mathf.Abs(transform.lossyScale.y);

            float castRadius =
                Mathf.Max(0.05f, capsule.radius * horizontalScale * 0.95f);

            float halfHeight =
                Mathf.Max(capsule.height * verticalScale * 0.5f, castRadius);

            float castDistance =
                Mathf.Max(0.05f, halfHeight - castRadius) + groundCheckDistance;

            isGrounded = Physics.SphereCast(
                castOrigin,
                castRadius,
                gravityDirection,
                out _,
                castDistance,
                groundMask,
                QueryTriggerInteraction.Ignore);
        }

        private void HandleGravityAlignmentAndFacing()
        {
            Vector3 gravityUp = -gravityDirection;

            if (mainCam == null)
                mainCam = Camera.main;

            bool gravityChanged =
                Vector3.Angle(previousGravityDirection, gravityDirection) > 1f;

            Vector3 desiredForward =
                currentFacingDirection;

            // ONLY while pressing W follow camera direction
            if (moveInput.y > 0.1f && mainCam != null)
            {
                desiredForward =
                    Vector3.ProjectOnPlane(
                        mainCam.transform.forward,
                        gravityDirection).normalized;
            }

            // Preserve facing direction during gravity changes
            desiredForward =
                Vector3.ProjectOnPlane(
                    desiredForward,
                    gravityDirection).normalized;

            if (desiredForward.sqrMagnitude < 0.0001f)
            {
                desiredForward =
                    Vector3.ProjectOnPlane(
                        transform.forward,
                        gravityDirection).normalized;
            }

            if (desiredForward.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation =
                Quaternion.LookRotation(
                    desiredForward,
                    gravityUp);

            float blendSpeed =
                gravityChanged ? 12f : rotationSpeed;

            float rotationBlend =
                1f - Mathf.Exp(-blendSpeed * Time.fixedDeltaTime);

            Quaternion smoothedRotation =
                Quaternion.Slerp(
                    body.rotation,
                    targetRotation,
                    rotationBlend);

            body.MoveRotation(smoothedRotation);

            currentFacingDirection =
                Vector3.ProjectOnPlane(
                    smoothedRotation * Vector3.forward,
                    gravityDirection).normalized;

            previousGravityDirection = gravityDirection;
        }

        private void HandleJump()
        {
            if (!jumpRequested)
                return;

            jumpRequested = false;

            if (!isGrounded)
                return;

            if (Time.time < nextJumpAllowedTime)
                return;

            Vector3 currentVelocity = body.linearVelocity;
            float gravityVelocity =
                Vector3.Dot(currentVelocity, gravityDirection);

            if (gravityVelocity > 0f)
                currentVelocity -= gravityDirection * gravityVelocity;

            currentVelocity += -gravityDirection * jumpForce;
            body.linearVelocity = currentVelocity;

            isGrounded = false;
            nextJumpAllowedTime = Time.time + jumpCooldown;
        }

        private void HandleGravity()
        {
            body.AddForce(
                gravityDirection * gravityForce,
                ForceMode.Acceleration);
        }

        private void HandleMovement()
        {
            GetReferenceMovementBasis(out Vector3 forward, out Vector3 right);

            Vector3 desiredMoveDirection =
                (forward * moveInput.y + right * moveInput.x).normalized;

            moveDirection = desiredMoveDirection;

            Vector3 desiredPlanarVelocity = moveDirection * moveSpeed;

            Vector3 currentVelocity = body.linearVelocity;
            float gravityVelocity =
                Vector3.Dot(currentVelocity, gravityDirection);

            Vector3 gravityComponent =
                gravityDirection * gravityVelocity;

            body.linearVelocity =
                desiredPlanarVelocity + gravityComponent;

            body.angularVelocity = Vector3.zero;
        }

        private void GetReferenceMovementBasis(out Vector3 forward, out Vector3 right)
        {
            Vector3 gravityUp = -gravityDirection;

            if (mainCam == null)
                mainCam = Camera.main;

            if (mainCam != null)
            {
                forward =
                    Vector3.ProjectOnPlane(
                        mainCam.transform.forward,
                        gravityDirection).normalized;
            }
            else
            {
                forward =
                    Vector3.ProjectOnPlane(
                        defaultForwardDirection,
                        gravityDirection).normalized;
            }

            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.ProjectOnPlane(Vector3.forward, gravityDirection).normalized;

            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.Cross(gravityUp, Vector3.right).normalized;

            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.Cross(gravityUp, Vector3.forward).normalized;

            right = Vector3.Cross(gravityUp, forward).normalized;
        }

        private void HandleFreeFall()
        {
            if (isGrounded)
            {
                airTimer = 0f;
                return;
            }

            airTimer += Time.fixedDeltaTime;

            if (airTimer >= maxAirTimeBeforeDeath)
                GameManager.Instance.LoseGame();
        }
    }
}