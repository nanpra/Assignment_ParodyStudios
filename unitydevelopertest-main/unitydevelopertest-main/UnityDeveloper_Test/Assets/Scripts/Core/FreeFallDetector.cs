using GravityPuzzle.Player;
using UnityEngine;

namespace GravityPuzzle.Core
{
    public class FreeFallDetector : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private float freeFallTime = 3f;

        private float timer;

        private void Update()
        {
            if (player.IsGrounded())
            {
                timer = 0f;
                return;
            }

            timer += Time.deltaTime;

            if (timer >= freeFallTime)
            {
                GameManager.Instance.LoseGame();
            }
        }
    }
}