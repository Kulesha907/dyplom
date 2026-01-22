using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Behavior;

namespace Script
{
   
    /// Контролер часу - управляє годинами та відправляє події для системи поведінки
    /// Time Controller - manages hours and sends events to the behavior system
    
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

        // Клас для опису події яка відбувається в певну годину
        // Class for describing an event that occurs at a specific hour
        [Serializable]
        public class TimeOfDayEvent
        {
            [Tooltip("Назва події / Event name")]
            public string name;
            
            [Tooltip("Канал події для викликання / Event channel to trigger")]
            public EventChannel eventChannel;
            
            [Tooltip("Година коли викликається подія (0-23) / Hour when the event is triggered (0-23)")]
            public int triggerHour;
            
            [Tooltip("Клавіша для ручного виклику події (опціонально) / Key for manual event triggering (optional)")]
            public Key shortcutKey = Key.None;
            
            // Внутрішній прапорець для відстеження чи подія була відправлена
            // Internal flag to track if the event has been sent
            [HideInInspector]
            public bool hasBeenTriggered;
        }
        
        [Header("Налаштування подій часу доби / Time of Day Events Setup")]
        [Tooltip("Список подій які викликаються в певні години / List of events triggered at specific hours")]
        public List<TimeOfDayEvent> timeOfDayEvents = new List<TimeOfDayEvent>();
          
        // Попередня година для відстеження змін
        // Previous hour to track changes
        private int _previousHour = -1;
        
        // Поточна активна подія (для відстеження завершення)
        // Current active event (for tracking completion)
        private string _currentActiveEvent;
        
        /// Викликається при ініціалізації об'єкта
        /// Called when the object is initialized
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
            
            // Валідація налаштувань подій
            // Validate event settings
            if (timeOfDayEvents == null || timeOfDayEvents.Count == 0)
            {
                Debug.LogWarning("TimeController: No time of day events configured! Please add events to the timeOfDayEvents list.");
            }
            else
            {
                Debug.Log($"TimeController: Configured {timeOfDayEvents.Count} time of day events:");
                foreach (var evt in timeOfDayEvents)
                {
                    if (evt.eventChannel == null)
                    {
                        Debug.LogWarning($"TimeController: Event '{evt.name}' at hour {evt.triggerHour} has no EventChannel assigned!");
                    }
                    else
                    {
                        Debug.Log($"  - {evt.name} at hour {evt.triggerHour}" + (evt.shortcutKey != Key.None ? $" (shortcut: {evt.shortcutKey})" : ""));
                    }
                }
            }
        }
        
        /// Викликається кожен кадр
        /// Called every frame
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
            
            // Перевіряємо клавіші для ручного виклику подій
            // Check keyboard shortcuts for manual event triggering
            if (Keyboard.current != null && timeOfDayEvents != null)
            {
                for (int i = 0; i < timeOfDayEvents.Count; i++)
                {
                    var evt = timeOfDayEvents[i];
                    if (evt.shortcutKey != Key.None && Keyboard.current[evt.shortcutKey].wasPressedThisFrame)
                    {
                        Debug.Log($"{evt.shortcutKey} key pressed - setting time to {evt.name}!");
                        hour = evt.triggerHour;
                        _hourTimer = 0f; // Скидаємо таймер / Reset timer
                        ResetEventFlags(); // Скидаємо прапорці подій / Reset event flags
                        evt.hasBeenTriggered = false; // Дозволяємо відправити подію / Allow event to be sent
                        TriggerEventByName(evt.name, evt.eventChannel); // Відразу викликаємо подію / Trigger event immediately
                        evt.hasBeenTriggered = true; // Позначаємо що подію відправлено / Mark event as sent
                        _previousHour = hour; // Оновлюємо попередню годину щоб уникнути повторного виклику / Update previous hour to avoid duplicate trigger
                        Debug.Log($"Hour set to: {hour} ({evt.name}) - Event triggered");
                        return; // Виходимо з Update щоб уникнути додаткових перевірок / Exit Update to avoid additional checks
                    }
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
            if (timeOfDayEvents != null)
            {
                for (int i = 0; i < timeOfDayEvents.Count; i++)
                {
                    var evt = timeOfDayEvents[i];
                    // Перевіряємо чи настав час для цієї події і чи вона ще не була викликана
                    // Check if it's time for this event and if it hasn't been triggered yet
                    if (hour == evt.triggerHour && !evt.hasBeenTriggered)
                    {
                        evt.hasBeenTriggered = true;
                        TriggerEventByName(evt.name, evt.eventChannel);
                    }
                }
            }
        }
        
        /// Скидає всі прапорці подій (викликається при зміні години)
        /// Resets all event flags (called when hour changes)
        private void ResetEventFlags()
        {
            if (timeOfDayEvents != null)
            {
                foreach (var evt in timeOfDayEvents)
                {
                    evt.hasBeenTriggered = false;
                }
            }
        }
        
        /// Викликає подію через EventChannel
        /// Triggers an event through an EventChannel
        private void TriggerEventByName(string eventName, EventChannel eventChannel)
        {
            Debug.Log($"Triggering {eventName} event at hour {hour}");
            
            // Перевірка чи призначений EventChannel
            // Check if EventChannel is assigned
            if (eventChannel == null)
            {
                Debug.LogError($"Cannot trigger {eventName} event: eventChannel is null! Please assign an EventChannel in the Inspector.");
                return;
            }
            
            // Завершуємо попередню подію, якщо вона існує і відрізняється від нової
            // Complete previous event if it exists and is different from the new one
            if (_currentActiveEvent != null && _currentActiveEvent != eventName)
            {
                Debug.Log($"Stopping previous event: {_currentActiveEvent}");
                
                // КРИТИЧНО: Спочатку завершуємо подію в blackboard
                // CRITICAL: First complete the event in blackboard
                CompleteEvent(_currentActiveEvent);
                
                // КРИТИЧНО: Перезапускаємо агента і запускаємо нову подію через корутину
                // CRITICAL: Restart the agent and trigger new event via coroutine
                if (agent != null && agent.enabled)
                {
                    Debug.Log("Restarting Behavior Graph Agent to stop previous behavior");
                    
                    // Вимикаємо агента
                    // Disable the agent
                    agent.enabled = false;
                    agent.End();
                    
                    // Перезапускаємо і відправляємо нову подію через корутину
                    // Restart and send new event via coroutine
                    StartCoroutine(RestartAgentAndTriggerEvent(eventName, eventChannel));
                    return; // Виходимо щоб не відправити подію двічі / Exit to avoid sending event twice
                }
            }
            
            // Якщо немає попередньої події або агент вимкнений - відправляємо подію одразу
            // If no previous event or agent is disabled - send event immediately
            SendEventToAgent(eventName, eventChannel);
        }
        
        /// <summary>
        /// Відправляє подію агенту та встановлює blackboard змінні
        /// Sends event to agent and sets blackboard variables
        /// </summary>
        private void SendEventToAgent(string eventName, EventChannel eventChannel)
        {
            if (agent != null)
            {
                var blackboard = agent.BlackboardReference;
                if (blackboard != null)
                {
                    // Скидаємо прапорець завершення для нової події ПЕРЕД її запуском
                    // Reset completion flag for the new event BEFORE triggering it
                    string completionVarName = $"{eventName}Completed";
                    if (blackboard.GetVariable<bool>(completionVarName, out var completionVar))
                    {
                        completionVar.Value = false;
                        Debug.Log($"{eventName} completion flag reset to false");
                    }
                    
                    // Встановлюємо активність нової події
                    // Set the new event as active
                    string activeVarName = $"{eventName}Active";
                    if (blackboard.GetVariable<bool>(activeVarName, out var activeVar))
                    {
                        activeVar.Value = true;
                        Debug.Log($"{eventName} marked as active in blackboard variable {activeVarName}");
                    }
                }
                else
                {
                    Debug.LogWarning($"Agent blackboard is null, events may not work properly");
                }
            }
            else
            {
                Debug.LogWarning($"Agent is null, only global event will be sent");
            }
            
            // ГОЛОВНЕ: Відправляємо подію через EventChannel (це працює глобально для всіх агентів)
            // MAIN: Send event through EventChannel (this works globally for all agents)
            eventChannel.SendEventMessage(Array.Empty<BlackboardVariable>());
            Debug.Log($"{eventName} event sent through EventChannel successfully");
            
            // Встановлюємо поточну активну подію
            // Set current active event
            _currentActiveEvent = eventName;
        }
        
        /// Завершує попередню подію
        /// Completes a previous event
        
        private void CompleteEvent(string eventName)
        {
            Debug.Log($"Completing and deactivating previous event: {eventName}");
            
            if (agent != null)
            {
                var blackboard = agent.BlackboardReference;
                if (blackboard != null)
                {
                    // Намагаємося встановити змінну завершення події в blackboard
                    // Try to set event completion variable in blackboard
                    string completionVarName = $"{eventName}Completed";
                    
                    if (blackboard.GetVariable<bool>(completionVarName, out var completionVar))
                    {
                        completionVar.Value = true;
                        Debug.Log($"{eventName} marked as completed in blackboard variable {completionVarName}");
                    }
                    else
                    {
                        Debug.Log($"No completion variable found for {eventName} (expected variable name: {completionVarName})");
                    }
                    
                    // Додатково: намагаємося скинути активність події
                    // Additionally: try to reset event activity
                    string activeVarName = $"{eventName}Active";
                    if (blackboard.GetVariable<bool>(activeVarName, out var activeVar))
                    {
                        activeVar.Value = false;
                        Debug.Log($"{eventName} marked as inactive in blackboard variable {activeVarName}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Перезапускає агента в наступному кадрі та відправляє нову подію
        /// Restarts the agent in the next frame and sends new event
        /// </summary>
        private System.Collections.IEnumerator RestartAgentAndTriggerEvent(string eventName, EventChannel eventChannel)
        {
            // Чекаємо один кадр щоб всі зміни в blackboard застосувалися
            // Wait one frame for all blackboard changes to apply
            yield return null;
            
            if (agent != null)
            {
                agent.enabled = true;
                agent.Start();
                Debug.Log("Behavior Graph Agent restarted successfully");
                
                // Чекаємо ще один кадр щоб агент повністю ініціалізувався
                // Wait one more frame for agent to fully initialize
                yield return null;
                
                // Тепер відправляємо нову подію
                // Now send the new event
                SendEventToAgent(eventName, eventChannel);
            }
        }
    }
}