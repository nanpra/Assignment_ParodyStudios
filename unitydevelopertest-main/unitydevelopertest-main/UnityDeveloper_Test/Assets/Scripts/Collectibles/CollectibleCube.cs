using DG.Tweening;
using GravityPuzzle.Core;
using UnityEngine;

namespace GravityPuzzle.Collectibles
{
    [RequireComponent(typeof(BoxCollider))]
    public class CollectibleCube : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float collectDuration = 0.25f;
        [SerializeField] private float moveUpAmount = 1f;

        private Collider cubeCollider;
        private bool collected;

        private void Awake()
        {
            cubeCollider = GetComponent<Collider>();
            cubeCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected)
                return;

            if (!other.CompareTag("Player"))
                return;

            Collect();
        }

        private void Collect()
        {
            collected = true;

            cubeCollider.enabled = false;

            GameManager.Instance.CollectCube();

            Sequence sequence = DOTween.Sequence();

            sequence.Append(
                transform.DOMoveY(
                    transform.position.y + moveUpAmount,
                    collectDuration));

            sequence.Join(
                transform.DOScale(
                    Vector3.zero,
                    collectDuration)
                .SetEase(Ease.OutBounce));

            sequence.OnComplete(() =>
            {
                Destroy(gameObject);
            });
        }
    }
}