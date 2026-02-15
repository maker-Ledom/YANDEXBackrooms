using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
public class MenuUI : MonoBehaviour
{
    [DllImport("__Internal")] 
    private static extern void GameReadyJS();
    [DllImport("__Internal")] 
    private static extern void ShowFullscreenAdJS();
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
        #if !UNITY_EDITOR && UNITY_WEBGL
        ShowFullscreenAdJS();
        #endif
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