using UnityEngine;
using UnityEngine.AI;

namespace Script
{
    /// <summary>
    /// Контролер анімації NPC - синхронізує анімацію ходьби зі швидкістю NavMeshAgent
    /// NPC Animation Controller - synchronizes walking animation with NavMeshAgent speed
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    public class NPCAnimationController : MonoBehaviour
    {
        // Посилання на NavMeshAgent
        // Reference to NavMeshAgent
        private NavMeshAgent _agent;
        
        // Посилання на Animator
        // Reference to Animator
        private Animator _animator;
        
        // Кешований хеш параметра Speed для оптимізації
        // Cached hash of Speed parameter for optimization
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        
        /// <summary>
        /// Викликається при ініціалізації об'єкта
        /// Called when the object is initialized
        /// </summary>
        void Start()
        {
            // Отримуємо компоненти
            // Get components
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            
            // Перевірка чи компоненти знайдені
            // Check if components are found
            if (_agent == null)
            {
                Debug.LogError($"❌ NPCAnimationController on {gameObject.name}: NavMeshAgent component not found!");
            }
            
            if (_animator == null)
            {
                Debug.LogError($"❌ NPCAnimationController on {gameObject.name}: Animator component not found!");
            }
            else
            {
                Debug.Log($"✅ NPCAnimationController initialized on {gameObject.name}");
            }
        }
        
        /// <summary>
        /// Викликається кожен кадр
        /// Called every frame
        /// </summary>
        void Update()
        {
            // Перевірка чи компоненти існують
            // Check if components exist
            if (_agent == null || _animator == null)
            {
                return;
            }
            
            // Обчислюємо швидкість агента (magnitude вектору швидкості)
            // Calculate agent speed (magnitude of velocity vector)
            float speed = _agent.velocity.magnitude;
            
            // Встановлюємо параметр Speed в аніматорі (використовуємо хеш для оптимізації)
            // Set Speed parameter in animator (using hash for optimization)
            _animator.SetFloat(SpeedHash, speed);
        }
    }
}
