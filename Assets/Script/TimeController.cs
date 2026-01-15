using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Behavior;

namespace Script
{
    /// <summary>
    /// Контролер часу - управляє годинами та відправляє події для системи поведінки
    /// Time Controller - manages hours and sends events to the behavior system
    /// </summary>
    public class TimeController : MonoBehaviour
    {
        // Поточна година (0-23)
        // Current hour (0-23)
        [Range(0, 23)] public int hour;

        // Посилання на агента з Behavior Graph
        // Reference to the Behavior Graph agent
        public BehaviorGraphAgent agent;
        
        // Таймер для автоматичного збільшення години
        // Timer for automatic hour increment
        private float _hourTimer;
        
        // Інтервал у секундах для збільшення години
        // Interval in seconds to increment the hour
        [Tooltip("Час у секундах між автоматичним збільшенням години / Time in seconds between automatic hour increments")]
        public float hourIncrementInterval = 10f;
        
        // Канали подій для різних частин доби (необов'язково, створюються автоматично якщо не призначені)
        // Event channels for different times of day (optional, created automatically if not assigned)
        [Header("Event Channels (optional)")]
        [Tooltip("Optional: Assign Morning EventChannel asset here. If not assigned, it will be created at runtime.")]
        public Morning morningEventChannel;
        
        [Tooltip("Optional: Assign Afternoon EventChannel asset here. If not assigned, it will be created at runtime.")]
        public Afternoon afternoonEventChannel;
        
        [Tooltip("Optional: Assign Evening EventChannel asset here. If not assigned, it will be created at runtime.")]
        public Evening eveningEventChannel;
        
        [Tooltip("Optional: Assign Night EventChannel asset here. If not assigned, it will be created at runtime.")]
        public Night nightEventChannel;

        // Прапорці для відстеження чи були відправлені події для кожної частини доби
        // Flags to track if events have been sent for each time of day
        private bool _morningSent;
        private bool _afternoonSent;
        private bool _eveningSent;
        private bool _nightSent;
        
        // Внутрішні посилання на канали подій
        // Internal references to event channels
        private Morning _runtimeMorningEvent;
        private Afternoon _runtimeAfternoonEvent;
        private Evening _runtimeEveningEvent;
        private Night _runtimeNightEvent;
        
        // Попередня година для відстеження змін
        // Previous hour to track changes
        private int _previousHour = -1;

        /// <summary>
        /// Викликається при ініціалізації об'єкта
        /// Called when the object is initialized
        /// </summary>
        void Start()
        {
            Debug.Log("TimeController: Start method called - script is active!");
            Debug.Log($"TimeController: Initial hour = {hour}");
            
            // Перевірка чи призначений агент
            // Check if agent is assigned
            if (agent == null)
            {
                Debug.LogWarning("TimeController: BehaviorGraphAgent is not assigned! Please assign it in the Inspector.");
            }

            // Ініціалізація EventChannel для всіх частин доби
            // Initialize EventChannels for all times of day
            _runtimeMorningEvent = InitializeEventChannel(morningEventChannel, "Morning");
            _runtimeAfternoonEvent = InitializeEventChannel(afternoonEventChannel, "Afternoon");
            _runtimeEveningEvent = InitializeEventChannel(eveningEventChannel, "Evening");
            _runtimeNightEvent = InitializeEventChannel(nightEventChannel, "Night");
            
            _previousHour = hour;
        }

        /// <summary>
        /// Ініціалізує EventChannel (створює runtime instance якщо не призначений)
        /// Initializes an EventChannel (creates runtime instance if not assigned)
        /// </summary>
        private T InitializeEventChannel<T>(T assignedChannel, string eventName) where T : ScriptableObject
        {
            if (assignedChannel == null)
            {
                Debug.LogWarning($"TimeController: {eventName} EventChannel not assigned. Creating runtime instance.");
                return ScriptableObject.CreateInstance<T>();
            }
            else
            {
                Debug.Log($"TimeController: Using assigned {eventName} EventChannel.");
                return assignedChannel;
            }
        }

        /// <summary>
        /// Викликається кожен кадр
        /// Called every frame
        /// </summary>
        void Update()
        {
            // Автоматичне збільшення години кожні N секунд
            // Automatic hour increment every N seconds
            _hourTimer += Time.deltaTime;
            if (_hourTimer >= hourIncrementInterval)
            {
                _hourTimer = 0f;
                hour++;
                
                // Обмежуємо годину діапазоном 0-23 (циклічно)
                // Limit hour to 0-23 range (cyclically)
                hour = hour % 24;
                
                Debug.Log($"Auto-increment: Hour is now {hour}");
            }
            
            // Перевіряємо клавіші для зміни часу доби (використовується нова Input System)
            // Check keys for changing time of day (using new Input System)
            if (Keyboard.current != null)
            {
                // M - Ранок (Morning) - 5:00
                if (Keyboard.current.mKey.wasPressedThisFrame)
                {
                    Debug.Log("M key pressed - setting time to Morning!");
                    hour = 5;
                    _hourTimer = 0f; // Скидаємо таймер / Reset timer
                    Debug.Log($"Hour set to: {hour} (Morning)");
                }
                
                // A - День/Обід (Afternoon) - 13:00
                if (Keyboard.current.aKey.wasPressedThisFrame)
                {
                    Debug.Log("A key pressed - setting time to Afternoon!");
                    hour = 13;
                    _hourTimer = 0f; // Скидаємо таймер / Reset timer
                    Debug.Log($"Hour set to: {hour} (Afternoon)");
                }
                
                // E - Вечір (Evening) - 18:00
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    Debug.Log("E key pressed - setting time to Evening!");
                    hour = 18;
                    _hourTimer = 0f; // Скидаємо таймер / Reset timer
                    Debug.Log($"Hour set to: {hour} (Evening)");
                }
                
                // N - Ніч (Night) - 22:00
                if (Keyboard.current.nKey.wasPressedThisFrame)
                {
                    Debug.Log("N key pressed - setting time to Night!");
                    hour = 22;
                    _hourTimer = 0f; // Скидаємо таймер / Reset timer
                    Debug.Log($"Hour set to: {hour} (Night)");
                }
            }

            // Перевіряємо чи змінилася година
            // Check if the hour has changed
            if (hour != _previousHour)
            {
                Debug.Log($"Hour changed from {_previousHour} to {hour}");
                _previousHour = hour;
                
                // Скидаємо прапорці для нової години
                // Reset flags for the new hour
                ResetEventFlags();
            }

            // Перевірка та виклик подій залежно від години
            // Check and trigger events based on the hour
            
            // Ранок (Morning) - 5:00
            if (hour >= 5 && hour < 13 && !_morningSent)
            {
                _morningSent = true;
                TriggerEvent(_runtimeMorningEvent, "Morning");
            }
            
            // День (Afternoon) - 13:00
            if (hour >= 13 && hour < 18 && !_afternoonSent)
            {
                _afternoonSent = true;
                TriggerEvent(_runtimeAfternoonEvent, "Afternoon");
            }
            
            // Вечір (Evening) - 18:00
            if (hour >= 18 && hour < 22 && !_eveningSent)
            {
                _eveningSent = true;
                TriggerEvent(_runtimeEveningEvent, "Evening");
            }
            
            // Ніч (Night) - 22:00 і 00:00
            if ((hour >= 22 || hour < 5) && !_nightSent)
            {
                _nightSent = true;
                TriggerEvent(_runtimeNightEvent, "Night");
            }
        }

        /// <summary>
        /// Скидає всі прапорці подій (викликається при зміні години)
        /// Resets all event flags (called when hour changes)
        /// </summary>
        private void ResetEventFlags()
        {
            _morningSent = false;
            _afternoonSent = false;
            _eveningSent = false;
            _nightSent = false;
        }

        /// <summary>
        /// Викликає подію через EventChannel
        /// Triggers an event through an EventChannel
        /// </summary>
        private void TriggerEvent<T>(T eventChannel, string eventName) where T : EventChannelBase
        {
            Debug.Log($"Triggering {eventName} event at hour {hour}");
            
            if (agent != null)
            {
                // Намагаємося знайти та запустити змінну в blackboard агента
                // Try to find and trigger the variable in agent's blackboard
                var blackboard = agent.BlackboardReference;
                if (blackboard != null)
                {
                    // Отримуємо змінну з blackboard
                    // Get the variable from blackboard
                    if (blackboard.GetVariable<T>(eventName, out var eventVar))
                    {
                        // Відправляємо подію через змінну blackboard
                        // Send the event through the blackboard variable
                        eventVar.Value?.SendEventMessage(System.Array.Empty<BlackboardVariable>());
                        Debug.Log($"{eventName} event triggered through blackboard successfully");
                    }
                    else
                    {
                        // Запасний варіант: намагаємося відправити глобальну подію
                        // Fallback: try sending global event
                        if (eventChannel != null)
                        {
                            eventChannel.SendEventMessage(System.Array.Empty<BlackboardVariable>());
                            Debug.Log($"{eventName} event triggered globally (no blackboard variable found)");
                        }
                        else
                        {
                            Debug.LogError($"Cannot trigger {eventName} event: no {eventName} variable in blackboard and no runtime event!");
                        }
                    }
                }
                else
                {
                    Debug.LogError($"Cannot trigger {eventName} event: agent blackboard is null!");
                }
            }
            else
            {
                Debug.LogError($"Cannot trigger {eventName} event: agent is null!");
            }
        }
    }
}