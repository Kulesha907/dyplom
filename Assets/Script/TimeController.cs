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
        
        // Канал події "Ранок" (необов'язково, створюється автоматично якщо не призначено)
        // "Morning" event channel (optional, created automatically if not assigned)
        [Tooltip("Optional: Assign Morning EventChannel asset here. If not assigned, it will be created at runtime.")]
        public Morning morningEventChannel;

        // Прапорець чи було вже відправлено ранкову подію
        // Flag indicating if the morning event has been sent
        private bool _morningSent;
        
        // Внутрішнє посилання на канал події Morning
        // Internal reference to the Morning event channel
        private Morning _runtimeMorningEvent;

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

            // Якщо канал події Morning не призначений, створюємо його під час виконання
            // If morningEventChannel is not assigned, create a runtime instance
            if (morningEventChannel == null)
            {
                Debug.LogWarning("TimeController: Morning EventChannel not assigned. Creating runtime instance.");
                _runtimeMorningEvent = ScriptableObject.CreateInstance<Morning>();
            }
            else
            {
                _runtimeMorningEvent = morningEventChannel;
                Debug.Log("TimeController: Using assigned Morning EventChannel.");
            }
        }

        /// <summary>
        /// Викликається кожен кадр
        /// Called every frame
        /// </summary>
        void Update()
        {
            // Перевіряємо чи натиснута клавіша M (використовується нова Input System)
            // Check if M key is pressed (using new Input System)
            if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            {
                Debug.Log("M key pressed!");
                // Встановлюємо годину на 1 (ранок)
                // Set hour to 1 (morning)
                hour = 1;
                Debug.Log($"Hour set to: {hour}");
            }

            // Якщо настала 1-а година або пізніше, і ще не відправляли ранкову подію
            // If it's 1 o'clock or later, and morning event hasn't been sent yet
            if (hour >= 1 && !_morningSent)
            {
                _morningSent = true;
                Debug.Log("Triggering Morning event");
                
                if (agent != null)
                {
                    // Намагаємося знайти та запустити змінну Morning в blackboard агента
                    // Try to find and trigger Morning event channel variable in agent's blackboard
                    var blackboard = agent.BlackboardReference;
                    if (blackboard != null)
                    {
                        // Отримуємо змінну Morning з blackboard
                        // Get the Morning variable from blackboard
                        if (blackboard.GetVariable<Morning>("Morning", out var morningEventVar))
                        {
                            // Відправляємо подію через змінну blackboard
                            // Send the event through the blackboard variable
                            morningEventVar.Value?.SendEventMessage();
                            Debug.Log("Morning event triggered through blackboard successfully");
                        }
                        else
                        {
                            // Запасний варіант: намагаємося відправити глобальну подію
                            // Fallback: try sending global event
                            if (_runtimeMorningEvent != null)
                            {
                                _runtimeMorningEvent.SendEventMessage();
                                Debug.Log("Morning event triggered globally (no blackboard variable found)");
                            }
                            else
                            {
                                Debug.LogError("Cannot trigger Morning event: no Morning variable in blackboard and no runtime event!");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Cannot trigger Morning event: agent blackboard is null!");
                    }
                }
                else
                {
                    Debug.LogError("Cannot trigger Morning event: agent is null!");
                }
            }
        }
    }
}