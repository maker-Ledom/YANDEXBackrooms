using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f; // ограничение взгляда вверх/вниз

    [Header("Flashlight Settings")]
    public KeyCode flashlightKey = KeyCode.F; // клавиша включения фонарика
    public Light flashlight; // ссылка на компонент света

    [Header("Footstep Sounds")]
    public AudioSource footstepAudioSource; // компонент для воспроизведения звуков
    [SerializeField] public AudioClip[] walkFootstepSounds; // массив звуков шагов при ходьбе
    [SerializeField] public AudioClip[] runFootstepSounds; // массив звуков шагов при беге
    public float walkFootstepInterval = 0.5f; // интервал между шагами при ходьбе
    public float runFootstepInterval = 0.3f; // интервал между шагами при беге
    public float footstepVolume = 0.5f; // громкость звуков шагов

    [Header("Jump & Landing Sounds")]
    public AudioSource jumpAudioSource; // отдельный источник для прыжка (рекомендуется)
    public AudioClip jumpSound; // звук прыжка
    public AudioClip[] landingSounds; // массив звуков приземления (для разнообразия)
    public float jumpVolume = 0.7f;
    public float landingVolume = 0.6f;
    public float landingSoundDelay = 0.1f; // небольшая задержка перед звуком приземления

    private CharacterController controller;
    private Camera playerCamera;
    private Vector3 velocity;
    private bool isGrounded;

    private float xRotation = 0f; // для поворота камеры по вертикали
    private bool isFlashlightOn = true; // состояние фонарика
    
    // Для звуков шагов
    private float footstepTimer = 0f;
    private bool wasMoving = false;
    private bool wasRunning = false;
    private Vector3 lastPosition;

    // Для звуков прыжка и приземления
    private bool wasGrounded = true;
    private bool hasJumped = false;
    private float landingTimer = 0f;
    private bool isLandingScheduled = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController не найден на объекте Player!");
        }
        playerCamera = GetComponentInChildren<Camera>();
        
        // Блокируем курсор в центре экрана
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Инициализация фонарика
        if (flashlight == null)
        {
            // Пытаемся найти фонарик автоматически
            flashlight = GetComponentInChildren<Light>();
        }
        
        // Включаем фонарик при старте
        if (flashlight != null)
        {
            flashlight.enabled = isFlashlightOn;
        }

        // Инициализация AudioSource для шагов, если не назначен
        if (footstepAudioSource == null)
        {
            footstepAudioSource = GetComponent<AudioSource>();
            if (footstepAudioSource == null)
            {
                footstepAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Инициализация AudioSource для прыжка, если не назначен
        if (jumpAudioSource == null)
        {
            // Пробуем найти отдельный компонент или создаем новый
            AudioSource[] sources = GetComponents<AudioSource>();
            if (sources.Length > 1)
            {
                jumpAudioSource = sources[1];
            }
            else
            {
                jumpAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        lastPosition = transform.position;
        wasGrounded = controller.isGrounded;
    }

    void Update()
    {
        bool isGameActive = Time.timeScale > 0;

        if (isGameActive)
        {
            HandleMovement();
            HandleMouseLook();   // внутри добавим дополнительную проверку на lock курсора
            HandleFlashlight();
            HandleFootsteps();
            HandleJumpAndLanding();
        }
    }

    void HandleMovement()
    {
        // Проверяем, стоит ли персонаж на земле
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Получаем ввод по горизонтали и вертикали
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Вектор движения относительно направления взгляда
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Бег (удерживайте Shift)
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Прыжок
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            hasJumped = true; // отмечаем, что совершили прыжок
        }

        // Применяем гравитацию
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleFlashlight()
    {
        // Проверяем нажатие клавиши для включения/выключения фонарика
        if (Input.GetKeyDown(flashlightKey))
        {
            ToggleFlashlight();
        }
    }

    void HandleFootsteps()
    {
        // Проверяем, движется ли игрок
        bool isMoving = (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) && isGrounded;
        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);

        // Если игрок изменил темп движения (с ходьбы на бег или наоборот)
        if (isMoving && wasMoving && wasRunning != isRunning)
        {
            // Останавливаем текущий звук при смене темпа
            if (footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Stop();
            }
            footstepTimer = 0f;
            PlayFootstepSound(isRunning);
        }
        // Если игрок начал двигаться
        else if (isMoving && !wasMoving)
        {
            footstepTimer = 0f;
            PlayFootstepSound(isRunning);
        }
        // Если игрок продолжает двигаться
        else if (isMoving)
        {
            float currentInterval = isRunning ? runFootstepInterval : walkFootstepInterval;
            footstepTimer += Time.deltaTime;
            
            if (footstepTimer >= currentInterval)
            {
                footstepTimer = 0f;
                PlayFootstepSound(isRunning);
            }
        }
        // Если игрок остановился
        else if (wasMoving && !isMoving)
        {
            footstepTimer = 0f;
            // Можно также остановить звук при остановке
            if (footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Stop();
            }
        }

        wasMoving = isMoving;
        wasRunning = isRunning;
    }

    void HandleJumpAndLanding()
    {
        // Обработка звука прыжка
        if (hasJumped)
        {
            PlayJumpSound();
            hasJumped = false;
        }

        // Обработка звука приземления
        if (wasGrounded != isGrounded)
        {
            if (!wasGrounded && isGrounded)
            {
                // Только что приземлились
                ScheduleLandingSound();
            }
            wasGrounded = isGrounded;
        }

        // Обработка отложенного звука приземления
        if (isLandingScheduled)
        {
            landingTimer += Time.deltaTime;
            if (landingTimer >= landingSoundDelay)
            {
                PlayLandingSound();
                isLandingScheduled = false;
                landingTimer = 0f;
            }
        }
    }

    void PlayFootstepSound(bool isRunning)
    {
        if (footstepAudioSource == null) return;

        AudioClip[] clips = isRunning ? runFootstepSounds : walkFootstepSounds;
        
        if (clips != null && clips.Length > 0)
        {
            int randomIndex = Random.Range(0, clips.Length);
            footstepAudioSource.pitch = Random.Range(0.9f, 1.1f);
            footstepAudioSource.volume = footstepVolume;
            footstepAudioSource.clip = clips[randomIndex];
            footstepAudioSource.Play(); // Play() автоматически остановит текущий звук
        }
    }

    void PlayJumpSound()
    {
        if (jumpAudioSource != null && jumpSound != null)
        {
            jumpAudioSource.pitch = Random.Range(0.95f, 1.05f); // небольшая вариация
            jumpAudioSource.volume = jumpVolume;
            jumpAudioSource.PlayOneShot(jumpSound);
        }
    }

    void PlayLandingSound()
    {
        if (jumpAudioSource != null && landingSounds != null && landingSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, landingSounds.Length);
            jumpAudioSource.pitch = Random.Range(0.9f, 1.1f);
            jumpAudioSource.volume = landingVolume;
            jumpAudioSource.PlayOneShot(landingSounds[randomIndex]);
        }
        else if (jumpAudioSource != null && jumpSound != null)
        {
            // Если нет отдельных звуков приземления, используем звук прыжка (но с меньшей громкостью)
            jumpAudioSource.pitch = Random.Range(0.9f, 1.1f);
            jumpAudioSource.volume = landingVolume * 0.8f;
            jumpAudioSource.PlayOneShot(jumpSound);
        }
    }

    void ScheduleLandingSound()
    {
        // Отменяем предыдущий запланированный звук, если был
        isLandingScheduled = false;
        landingTimer = 0f;
        
        // Планируем новый звук приземления
        isLandingScheduled = true;
    }

    public void ToggleFlashlight()
    {
        if (flashlight != null)
        {
            isFlashlightOn = !isFlashlightOn;
            flashlight.enabled = isFlashlightOn;
        }
    }

    public void SetFlashlight(bool state)
    {
        if (flashlight != null)
        {
            isFlashlightOn = state;
            flashlight.enabled = isFlashlightOn;
        }
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Проверяем, столкнулись ли мы с врагом
        if (hit.gameObject.CompareTag("Enemy"))
        {
            StartCoroutine(RestartScene());
        }
        if (hit.gameObject.CompareTag("Finish"))
        {
            StartCoroutine(FinishGame());
        }
    }
    private IEnumerator RestartScene()
    {
        // Блокируем управление игроком
        enabled = false;
        
        // Опционально: проиграть звук смерти
        // AudioSource.PlayClipAtPoint(deathSound, transform.position);
        
        Debug.Log("Игрок пойман! Перезагрузка через 1 секунду...");
        
        // Ждем 1 секунду
        yield return new WaitForSeconds(1f);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (GameManager.Instance != null)
            GameManager.Instance.LoadMenuScene();
        else
            Debug.LogError("GameManager не найден!");
    }
    private IEnumerator FinishGame()
    {
        enabled = false;
        
        // Опционально: проиграть звук смерти
        // AudioSource.PlayClipAtPoint(deathSound, transform.position);
        
        Debug.Log("Игрок пройдена");
        
        // Ждем 1 секунду
        yield return new WaitForSeconds(2f);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (GameManager.Instance != null)
            GameManager.Instance.LoadMenuScene();
        else
            Debug.LogError("GameManager не найден!");
    }
}