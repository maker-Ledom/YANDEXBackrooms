using UnityEngine;
using System.Collections;
public class Enemy : MonoBehaviour
{
    [Header("Настройки движения")]
    public float patrolSpeed = 2f;      // Скорость ходьбы на патруле
    public float chaseSpeed = 4f;       // Скорость бега за игроком
    public float rotationSpeed = 5f;    // Скорость поворота

    [Header("Зрение и дистанции")]
    public float sightRange = 10f;       // Радиус срабатывания триггера (задается коллайдером)
    public float losePlayerRadius = 15f; // Если игрок ушел дальше этого радиуса, картина его теряет

    [Header("Маршрут")]
    public Transform[] waypoints;        // Массив точек патруля (задайте в инспекторе)
    private int currentWaypointIndex = 0;

    [Header("Компоненты")]
    public Transform player;             // Ссылка на игрока (перетащите игрока в инспектор)
    private UnityEngine.AI.NavMeshAgent agent; // Навигационный агент (искусственный интеллект)

    private enum State { Patrol, Chase, ReturnToPatrol }
    private State currentState = State.Patrol;

    private Vector3 startPosition;       // Начальная позиция (на случай возврата)
    private float distanceToPlayer;

    void Start()
    {
        // Добавляем и настраиваем NavMeshAgent
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<UnityEngine.AI.NavMeshAgent>();
        }

        agent.speed = patrolSpeed;
        agent.angularSpeed = rotationSpeed * 100; // NavMeshAgent использует свои настройки угловой скорости
        agent.acceleration = 8f;
        agent.stoppingDistance = 1f; // Останавливается в 1 метре от цели

        startPosition = transform.position;

        // Если игрок не назначен в инспекторе, найдем его по тегу
        if (player == null && GameObject.FindGameObjectWithTag("Player") != null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // Стартуем к первой точке
        GoToNextWaypoint();
    }

    void Update()
    {
        // Вычисляем дистанцию до игрока
        if (player != null)
        {
            distanceToPlayer = Vector3.Distance(transform.position, player.position);
        }

        // Конечный автомат состояний
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Chase:
                Chase();
                break;
            case State.ReturnToPatrol:
                ReturnToPatrol();
                break;
        }
    }

    // Состояние 1: Патрулирование
    void Patrol()
    {
        agent.speed = patrolSpeed;

        // Если мы видим игрока (триггер сработал отдельно), переключаемся
        // Проверка на видение происходит в OnTriggerEnter/Stay, но для надежности проверим дистанцию
        if (player != null && distanceToPlayer <= sightRange)
        {
            currentState = State.Chase;
            return;
        }

        // Если дошли до точки, идем к следующей
        if (!agent.pathPending && agent.remainingDistance < agent.stoppingDistance)
        {
            GoToNextWaypoint();
        }
    }

    // Состояние 2: Преследование
    void Chase()
    {
        agent.speed = chaseSpeed;

        if (player != null)
        {
            agent.SetDestination(player.position);

            // Проверяем, не убежал ли игрок слишком далеко
            if (distanceToPlayer > losePlayerRadius)
            {
                currentState = State.ReturnToPatrol;
            }
        }
    }

    // Состояние 3: Возврат на маршрут
    void ReturnToPatrol()
    {
        agent.speed = patrolSpeed;

        // Находим ближайшую точку маршрута, чтобы вернуться к патрулю
        if (agent.remainingDistance < agent.stoppingDistance)
        {
            // Возвращаемся к обычному патрулированию
            currentState = State.Patrol;
        }
    }

    // Метод для перехода к следующей точке маршрута
    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("Нет точек маршрута! Картина стоит на месте.");
            return;
        }

        // Циклический переход по точкам
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    // Обработка входа в триггер (зону зрения)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Убедитесь, что у игрока стоит тег "Player"
        {
            // Начинаем преследование, если мы не в нем уже
            if (currentState != State.Chase)
            {
                currentState = State.Chase;
            }
        }
    }

    // Если игрок стоит в зоне видимости, продолжаем преследование
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && currentState == State.Patrol)
        {
            currentState = State.Chase;
        }
    }

    // Визуализация радиусов в редакторе (для удобства настройки)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, losePlayerRadius);
    }
}