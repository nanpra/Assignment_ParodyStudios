using UnityEngine;

namespace GravityPuzzle.Gravity
{
    public class GravityManager : MonoBehaviour
    {
        public static GravityManager Instance;

        [SerializeField] private float gravityStrength = 25f;

        public Vector3 CurrentGravity { get; private set; } = Vector3.down;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            ApplyGravity(GravityDirection.Down);
        }

        public void ApplyGravity(GravityDirection direction)
        {
            switch (direction)
            {
                case GravityDirection.Down:
                    CurrentGravity = Vector3.down;
                    break;

                case GravityDirection.Up:
                    CurrentGravity = Vector3.up;
                    break;

                case GravityDirection.Left:
                    CurrentGravity = Vector3.left;
                    break;

                case GravityDirection.Right:
                    CurrentGravity = Vector3.right;
                    break;

                case GravityDirection.Forward:
                    CurrentGravity = Vector3.forward;
                    break;

                case GravityDirection.Backward:
                    CurrentGravity = Vector3.back;
                    break;
            }

            Physics.gravity = CurrentGravity * gravityStrength;
        }
    }
}