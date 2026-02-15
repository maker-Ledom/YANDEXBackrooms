using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
public class UIGamePanel : MonoBehaviour
{
        [DllImport("__Internal")] 
    private static extern void GameReadyJS();
    [DllImport("__Internal")] 
    private static extern void ShowFullscreenAdJS();
    [Header("UI References")]
    [SerializeField] private Button soundButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button goMenuButton;
    [SerializeField] private GameObject panel;

    private bool isPaused = false;

    private void Start()
    {
        // Подписываем кнопки
        if (soundButton != null)
            soundButton.onClick.AddListener(ToggleSound);

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

        if (goMenuButton != null)
            goMenuButton.onClick.AddListener(GoToMenu);

        // Начальное состояние: панель скрыта, курсор заблокирован
        panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Обработка нажатия ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                PauseGame();
            else
                ContinueGame();
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        panel.SetActive(true);

        // Пауза времени
        if (GameManager.Instance != null)
            GameManager.Instance.SetPause(true);
        else
            Time.timeScale = 0f;

        // Разблокировать курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ContinueGame()
    {
        isPaused = false;
        panel.SetActive(false);

        // Снять паузу
        if (GameManager.Instance != null)
            GameManager.Instance.SetPause(false);
        else
            Time.timeScale = 1f;

        // Заблокировать курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ToggleSound()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ToggleSound();
        else
            Debug.LogError("GameManager не найден!");
    }

    private void GoToMenu()
    {
        // Сбрасываем паузу и разблокируем курсор перед загрузкой меню
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        #if !UNITY_EDITOR && UNITY_WEBGL
        ShowFullscreenAdJS();
        #endif
        if (GameManager.Instance != null)
            GameManager.Instance.LoadMenuScene();
        else
            Debug.LogError("GameManager не найден!");
    }
}