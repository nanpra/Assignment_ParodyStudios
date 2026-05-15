using UnityEngine;
using TMPro;

namespace GravityPuzzle.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private float gameTime = 120f;

        [SerializeField] private TMP_Text timerText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject winPanel;

        [SerializeField] private int totalCubes;

        private int collectedCubes;
        private bool gameEnded;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (gameEnded)
                return;

            UpdateTimer();
        }

        private void UpdateTimer()
        {
            gameTime -= Time.deltaTime;

            if (gameTime <= 0f)
            {
                gameTime = 0f;
                LoseGame();
            }

            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);

            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        public void CollectCube()
        {
            collectedCubes++;

            if (collectedCubes >= totalCubes)
                WinGame();
        }

        public void LoseGame()
        {
            if (gameEnded)
                return;

            gameEnded = true;
            Time.timeScale = 0f;
            gameOverPanel.SetActive(true);
        }

        private void WinGame()
        {
            gameEnded = true;
            Time.timeScale = 0f;
            winPanel.SetActive(true);
        }
    }
}