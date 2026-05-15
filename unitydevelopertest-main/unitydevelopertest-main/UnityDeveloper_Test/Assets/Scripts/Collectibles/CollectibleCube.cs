using GravityPuzzle.Core;
using UnityEngine;

namespace GravityPuzzle.Collectibles
{
    public class CollectibleCube : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                return;

            GameManager.Instance.CollectCube();
            Destroy(gameObject);
        }
    }
}