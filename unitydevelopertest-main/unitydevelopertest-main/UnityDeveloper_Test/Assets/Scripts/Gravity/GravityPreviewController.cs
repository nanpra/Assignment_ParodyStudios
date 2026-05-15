using UnityEngine;
using GravityPuzzle.Gravity;

namespace GravityPuzzle.Player
{
    public class GravityPreviewController : MonoBehaviour
    {
        [SerializeField] private GameObject hologramPrefab;

        private GameObject hologramInstance;
        private GravityDirection selectedDirection;

        private void Start()
        {
            hologramInstance = Instantiate(hologramPrefab);
            hologramInstance.SetActive(false);
        }

        private void Update()
        {
            HandleDirectionSelection();
            HandleGravityApply();
        }

        private void HandleDirectionSelection()
        {
            bool hasSelection = false;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                selectedDirection = GravityDirection.Forward;
                hasSelection = true;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                selectedDirection = GravityDirection.Backward;
                hasSelection = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                selectedDirection = GravityDirection.Left;
                hasSelection = true;
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                selectedDirection = GravityDirection.Right;
                hasSelection = true;
            }

            if (!hasSelection)
                return;

            ShowPreview();
        }

        private void ShowPreview()
        {
            hologramInstance.SetActive(true);

            Vector3 direction =
                GravityManager.Instance.CurrentGravity;

            hologramInstance.transform.SetPositionAndRotation(
                transform.position + direction * -2f,
                Quaternion.LookRotation(direction));
        }

        private void HandleGravityApply()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                GravityManager.Instance.ApplyGravity(selectedDirection);
                hologramInstance.SetActive(false);
            }
        }
    }
}