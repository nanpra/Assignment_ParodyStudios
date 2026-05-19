using DG.Tweening;
using GravityPuzzle.Core;
using TMPro;
using UnityEngine;

namespace GravityPuzzle.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [Header("Timer")]
        [SerializeField] private float gameTime = 120f;
        [SerializeField] private TMP_Text timerText;

        [Header("Cube Collection")]
        [SerializeField] private TMP_Text cubeText;

        [Header("Animations")]
        [SerializeField] private RectTransform timerRect;
        [SerializeField] private RectTransform cubeRect;

        private bool timerEnded;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            UpdateCubeText(0, 5);
        }

        private void Update()
        {
            if (timerEnded)
                return;

            UpdateTimer();
        }

        private void UpdateTimer()
        {
            gameTime -= Time.deltaTime;

            if (gameTime <= 0f)
            {
                gameTime = 0f;
                timerEnded = true;

                GameManager.Instance.LoseGame();
            }

            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);

            timerText.text =
                $"{minutes:00}:{seconds:00}";

            HandleLowTimeEffects();
        }

        public void UpdateCubeText(int collected, int total)
        {
            cubeText.text =
                $"Cubes Collected- {collected}/{total}";

            AnimateCubeCollection();
        }

        private void AnimateCubeCollection()
        {
            cubeRect.DOKill();
            cubeRect.localScale = Vector3.one;

            cubeRect
                .DOScale(1.25f, 0.15f)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    cubeRect.DOScale(1f, 0.15f);
                });
        }

        private void HandleLowTimeEffects()
        {
            if (gameTime > 10f)
            {
                timerText.color = Color.black;
                return;
            }

            timerText.color = Color.red;

            timerRect.DOKill();

            timerRect
                .DOScale(1.15f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }
}