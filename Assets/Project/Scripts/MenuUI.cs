using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button soundButton;

    private void Start()
    {
        // Подписываем кнопки
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (soundButton != null)
            soundButton.onClick.AddListener(ToggleSound);
    }

    private void StartGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
        else
            Debug.LogError("GameManager не найден!");
    }

    private void ToggleSound()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ToggleSound();
        else
            Debug.LogError("GameManager не найден!");
    }
}