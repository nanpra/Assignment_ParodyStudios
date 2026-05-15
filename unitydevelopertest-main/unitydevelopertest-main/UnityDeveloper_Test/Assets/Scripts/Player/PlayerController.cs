using UnityEngine;
using GravityPuzzle.Gravity;

namespace GravityPuzzle.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private float jumpForce = 7f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundRadius = 0.2f;
        [SerializeField] private LayerMask groundMask;

        private Rigidbody rb;
        private Camera mainCam;

        private bool isGrounded;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            mainCam = Camera.main;

            rb.useGravity = true;
        }

        private void Update()
        {
            GroundCheck();
            HandleJump();
        }

        private void FixedUpdate()
        {
            HandleMovement();
            AlignToGravity();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 camForward = Vector3.ProjectOnPlane(mainCam.transform.forward,
                -GravityManager.Instance.CurrentGravity).normalized;

            Vector3 camRight = Vector3.ProjectOnPlane(mainCam.transform.right,
                -GravityManager.Instance.CurrentGravity).normalized;

            Vector3 moveDirection = (camForward * vertical + camRight * horizontal).normalized;

            Vector3 velocity = rb.linearVelocity;
            Vector3 gravityVelocity =
                Vector3.Project(velocity, GravityManager.Instance.CurrentGravity);

            Vector3 targetVelocity = moveDirection * moveSpeed + gravityVelocity;

            rb.linearVelocity = targetVelocity;

            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(moveDirection, -GravityManager.Instance.CurrentGravity);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime);
            }
        }

        private void HandleJump()
        {
            if (!isGrounded)
                return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.AddForce(
                    -GravityManager.Instance.CurrentGravity * jumpForce,
                    ForceMode.Impulse);
            }
        }

        private void GroundCheck()
        {
            isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundRadius,
                groundMask);
        }

        private void AlignToGravity()
        {
            Quaternion targetRotation =
                Quaternion.FromToRotation(transform.up,
                    -GravityManager.Instance.CurrentGravity)
                * transform.rotation;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                10f * Time.fixedDeltaTime);
        }

        public bool IsGrounded()
        {
            return isGrounded;
        }
    }
}