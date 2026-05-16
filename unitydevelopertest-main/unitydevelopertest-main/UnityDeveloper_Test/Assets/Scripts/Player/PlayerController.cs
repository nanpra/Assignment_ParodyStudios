using GravityPuzzle.Core;
using GravityPuzzle.Gravity;
using UnityEngine;

namespace GravityPuzzle.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerAnimationController))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float jumpForce = 10f;

        [Header("Gravity")]
        [SerializeField] private float gravityForce = 25f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float groundCheckDistance = 0.3f;

        [Header("Free Fall")]
        [SerializeField] private float maxAirTimeBeforeDeath = 2f;

        private CharacterController controller;
        private Camera mainCam;
        private PlayerAnimationController animationController;

        private Vector3 velocity;
        private Vector3 moveDirection;

        private bool isGrounded;
        private float airTimer;
        private Vector3 airborneMomentum;

        public bool IsGrounded => isGrounded;

        private void Awake()
        {
            Instance = this;

            controller = GetComponent<CharacterController>();
            animationController = GetComponent<PlayerAnimationController>();

            mainCam = Camera.main;
        }

        private void Update()
        {
            CheckGround();
            HandleMovement();
            HandleGravity();
            HandleJump();
            HandleFreeFall();
            AlignToGravity();

            controller.Move(velocity * Time.deltaTime);

            animationController.UpdateAnimationState(
                moveDirection,
                velocity,
                isGrounded,
                GravityManager.Instance.CurrentGravity);
        }

        private void HandleMovement()
        {
            if(!isGrounded)
                return;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 gravityUp =
                -GravityManager.Instance.CurrentGravity.normalized;

            Vector3 camForward =
                Vector3.ProjectOnPlane(mainCam.transform.forward, gravityUp).normalized;

            Vector3 camRight =
                Vector3.ProjectOnPlane(mainCam.transform.right, gravityUp).normalized;

            moveDirection =
                (camForward * vertical + camRight * horizontal).normalized;

            Vector3 movement =
                moveDirection * moveSpeed;

            Vector3 gravityVelocity =
                Vector3.Project(
                    velocity,
                    GravityManager.Instance.CurrentGravity);

            velocity =
                moveDirection * moveSpeed + gravityVelocity;

            controller.Move(movement * Time.deltaTime);

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(moveDirection, gravityUp);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);
            }
        }

        private void HandleGravity()
        {
            velocity += gravityForce
                * Time.deltaTime
                * GravityManager.Instance.CurrentGravity;
        }

        private void HandleJump()
        {
            if (!isGrounded)
                return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                airborneMomentum =
                    Vector3.ProjectOnPlane(
                        velocity,
                        GravityManager.Instance.CurrentGravity);

                velocity =
                    airborneMomentum +
                    (-GravityManager.Instance.CurrentGravity.normalized * jumpForce);

                isGrounded = false;
            }
        }

        private void CheckGround()
        {
            Vector3 gravityDirection =
                GravityManager.Instance.CurrentGravity.normalized;

            Vector3 origin =
                transform.position;

            bool hitGround = Physics.Raycast(
                origin,
                gravityDirection,
                controller.height * 0.6f + groundCheckDistance,
                groundMask,
                QueryTriggerInteraction.Ignore);

            isGrounded = hitGround;

            if (isGrounded)
            {
                float gravityVelocity =
                    Vector3.Dot(velocity, gravityDirection);

                if (gravityVelocity > 0)
                    velocity -= gravityDirection * gravityVelocity;
            }
        }

        private void HandleFreeFall()
        {
            if (isGrounded)
            {
                airTimer = 0f;
                return;
            }

            airTimer += Time.deltaTime;

            if (airTimer >= maxAirTimeBeforeDeath)
                GameManager.Instance.LoseGame();
        }

        private void AlignToGravity()
        {
            Quaternion targetRotation =
                Quaternion.FromToRotation(
                    transform.up,
                    -GravityManager.Instance.CurrentGravity.normalized)
                * transform.rotation;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.deltaTime);
        }

        private void OnDrawGizmosSelected()
        {
            if (GravityManager.Instance == null)
                return;

            Gizmos.color = Color.green;

            Vector3 gravityDirection =
                GravityManager.Instance.CurrentGravity.normalized;

            Gizmos.DrawRay(
                transform.position,
                gravityDirection * 2f);
        }
    }
}