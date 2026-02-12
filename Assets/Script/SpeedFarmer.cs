using UnityEngine;

namespace Script
{
    /// <summary>
    /// Визначає швидкість агента на основі зміни його координат
    /// Determines agent's speed based on coordinate changes
    /// </summary>
    public class SpeedFarmer : MonoBehaviour
    {
        private static readonly int SppedProperty = Animator.StringToHash("Speed");

        [Header("Speed Settings / Налаштування швидкості")]
        [Tooltip("Агент для відстеження / Agent to track")]
        public Transform agent;

        public Animator anim;
        
        [Tooltip("Поточна швидкість (0 або 1) / Current speed (0 or 1)")]
        [SerializeField]
        private int speed;
        
        [Header("Debug")]
        [Tooltip("Показувати логи у консолі / Show debug logs")]
        public bool showDebugLogs;
        
        // Попередня позиція агента / Previous position of the agent
        private Vector3 _previousPosition;
        
        // Мінімальна зміна координат для визначення руху
        // Minimum coordinate change to detect movement
        private const float MovementThreshold = 0.001f;
        
        /// <summary>
        /// Публічний геттер для швидкості
        /// Public getter for speed
        /// </summary>
        public int Speed => speed;
        
        void Start()
        {
            // Якщо агент не призначений, спробувати використати поточний GameObject
            // If agent is not assigned, try to use current GameObject
            if (agent == null)
            {
                agent = transform;
                if (showDebugLogs)
                {
                    Debug.Log($"SpeedFarmer: Agent not assigned, using self: {gameObject.name}");
                }
            }
            
            anim = GetComponent<Animator>();
            
            // Ініціалізація попередньої позиції
            // Initialize previous position
            _previousPosition = agent.position;
        }
        
        void Update()
        {
            if (agent == null)
            {
                Debug.LogWarning("SpeedFarmer: Agent is null!");
                return;
            }
            
            // Отримуємо поточну позицію агента
            // Get current position of the agent
            Vector3 currentPosition = agent.position;
            
            // Обчислюємо зміну позиції
            // Calculate position change
            float positionChange = Vector3.Distance(currentPosition, _previousPosition);
            
            // Визначаємо швидкість: якщо координати змінились - швидкість 1, якщо ні - 0
            // Determine speed: if coordinates changed - speed is 1, if not - 0
            int newSpeed = positionChange > MovementThreshold ? 1 : 0;
            
            // Завжди оновлюємо швидкість для точності
            // Always update speed for accuracy
            speed = newSpeed;
            
            anim.SetFloat(SppedProperty, speed);
            
            // Виводимо детальні логи якщо увімкнено
            // Output detailed logs if enabled
            if (showDebugLogs)
            {
                Debug.Log($"SpeedFarmer [{agent.name}] Update: Position={currentPosition}, " +
                          $"Previous={_previousPosition}, Change={positionChange:F6}, Speed={speed}, Time={Time.time:F2}");
            }
            
            // Зберігаємо поточну позицію для наступного кадру
            // Store current position for next frame
            _previousPosition = currentPosition;
        }
        
        /// <summary>
        /// Отримати швидкість як bool (true = рухається, false = стоїть)
        /// Get speed as bool (true = moving, false = stationary)
        /// </summary>
        public bool IsMoving()
        {
            return speed == 1;
        }
        
        /// <summary>
        /// Вручну оновити швидкість (для дебагу)
        /// Manually update speed (for debugging)
        /// </summary>
        public void ForceUpdateSpeed()
        {
            if (agent == null)
            {
                Debug.LogWarning("SpeedFarmer: Cannot force update - agent is null!");
                return;
            }
            
            Vector3 currentPosition = agent.position;
            float positionChange = Vector3.Distance(currentPosition, _previousPosition);
            int newSpeed = positionChange > MovementThreshold ? 1 : 0;
            speed = newSpeed;
            _previousPosition = currentPosition;
            
            Debug.Log($"SpeedFarmer [{agent.name}] Force Update: Speed={speed}, Change={positionChange:F6}");
        }
        
        /// <summary>
        /// Перевірити чи компонент активний
        /// Check if component is active
        /// </summary>
        void OnEnable()
        {
            if (showDebugLogs)
            {
                Debug.Log($"SpeedFarmer [{gameObject.name}]: Component enabled at {Time.time:F2}");
            }
        }
        
        void OnDisable()
        {
            if (showDebugLogs)
            {
                Debug.Log($"SpeedFarmer [{gameObject.name}]: Component disabled at {Time.time:F2}");
            }
        }
    }
}