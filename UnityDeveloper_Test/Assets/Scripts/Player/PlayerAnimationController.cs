using UnityEngine;

namespace GravityPuzzle.Player
{
    public class PlayerAnimationController : MonoBehaviour
    {
        private Animator animator;

        private readonly int SpeedHash =
            Animator.StringToHash("Speed");

        private readonly int IsJumpingHash =
            Animator.StringToHash("IsJumping");

        private readonly int IsFallingHash =
            Animator.StringToHash("IsFalling");

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void UpdateAnimationState(
            Vector3 moveDirection,
            Vector3 velocity,
            bool isGrounded,
            Vector3 gravityDirection)
        {
            float moveSpeed = moveDirection.magnitude;

            animator.SetFloat(SpeedHash, moveSpeed);

            if (isGrounded)
            {
                animator.SetBool(IsJumpingHash, false);
                animator.SetBool(IsFallingHash, false);
                return;
            }

            float verticalVelocity =
                Vector3.Dot(velocity, gravityDirection);

            bool isJumping = verticalVelocity < -0.1f;
            bool isFalling = verticalVelocity > 0.1f;

            animator.SetBool(IsJumpingHash, isJumping);
            animator.SetBool(IsFallingHash, isFalling);
        }
    }
}