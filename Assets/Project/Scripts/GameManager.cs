using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string menuSceneName = "MenuScene";
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Sound Settings")]
    [SerializeField] private bool isSoundMuted = false; // начальное состояние звука

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Синглтон с сохранением между сценами
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Применяем сохранённое состояние звука при старте
        SetSoundState(!isSoundMuted);
    }

    // ===== Методы для кнопок меню =====
    // Вызывается кнопкой "Старт" в меню
    public void StartGame()
    {
        Time.timeScale = 1f; // На случай, если игра была на паузе
        SceneManager.LoadScene(gameSceneName);
    }

    // Вызывается кнопкой "Вкл/Выкл звук" в любой сцене
    public void ToggleSound()
    {
        isSoundMuted = !isSoundMuted;
        SetSoundState(!isSoundMuted);
    }

    // ===== Методы для панели в игровой сцене =====
    // Вызывается кнопкой "Выход в меню"
    public void LoadMenuScene()
    {
        Time.timeScale = 1f; // Сбрасываем паузу, если она была
        SceneManager.LoadScene(menuSceneName);
    }

    // Дополнительно: метод для паузы/возобновления игры (опционально)
    public void SetPause(bool pause)
    {
        Time.timeScale = pause ? 0f : 1f;
    }

    // ===== Внутренние методы =====
    private void SetSoundState(bool enabled)
    {
        AudioListener.volume = enabled ? 1f : 0f;
    }
    // ===== Свойства для UI =====
    public bool IsSoundMuted => isSoundMuted;
}