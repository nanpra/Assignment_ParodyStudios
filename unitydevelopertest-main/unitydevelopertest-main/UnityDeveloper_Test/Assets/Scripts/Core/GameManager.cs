using DG.Tweening;
using GravityPuzzle.UI;
using UnityEngine;

namespace GravityPuzzle.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Panels")]
        [SerializeField] private CanvasGroup gameLosePanel;
        [SerializeField] private RectTransform gameLosePopup;

        [SerializeField] private CanvasGroup winPanel;
        [SerializeField] private RectTransform winPopup;

        [Header("Gameplay")]
        [SerializeField] private int totalCubesToCollect = 5;

        public int CollectedCubes { get; private set; }

        private bool gameEnded;

        private void Awake()
        {
            Instance = this;

            InitializePanels();
        }

        private void InitializePanels()
        {
            SetupPanel(gameLosePanel, gameLosePopup);
            SetupPanel(winPanel, winPopup);
        }

        private void SetupPanel(
            CanvasGroup panel,
            RectTransform popup)
        {
            panel.alpha = 0f;
            panel.interactable = false;
            panel.blocksRaycasts = false;

            popup.localScale = Vector3.zero;

            panel.gameObject.SetActive(false);
        }

        public void CollectCube()
        {
            if (gameEnded)
                return;

            CollectedCubes++;

            Debug.Log(
                $"Collected Cubes: {CollectedCubes}/{totalCubesToCollect}");

            UIManager.Instance.UpdateCubeText(
                CollectedCubes,
                totalCubesToCollect);

            if (CollectedCubes >= totalCubesToCollect)
            {
                WinGame();
            }
        }

        public void LoseGame()
        {
            if (gameEnded)
                return;

            gameEnded = true;

            AnimatePanel(
                gameLosePanel,
                gameLosePopup);
        }

        private void WinGame()
        {
            if (gameEnded)
                return;

            gameEnded = true;

            AnimatePanel(
                winPanel,
                winPopup);
        }

        private void AnimatePanel(
            CanvasGroup panel,
            RectTransform popup)
        {
            panel.gameObject.SetActive(true);

            panel.interactable = true;
            panel.blocksRaycasts = true;

            panel.alpha = 0f;
            popup.localScale = Vector3.zero;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(
                panel.DOFade(1f, 0.25f));

            sequence.Join(
                popup.DOScale(1f, 0.4f)
                .SetEase(Ease.OutBack));

            sequence.OnComplete(() =>
            {
                Time.timeScale = 0f;
            });
        }
    }
}