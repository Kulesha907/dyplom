using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Behavior;
using UnityEngine.UI;

namespace Script
{
   
    /// Контролер часу - управляє годинами та відправляє події для системи поведінки
    /// Time Controller - manages hours and sends events to the behavior system
    
    public class TimeController : MonoBehaviour
    {
        // Поточна година (0-23)
        // Current hour (0-23)
        [Range(0, 23)] public int hour;
        
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
            
            [Tooltip("UI кнопка для ручного виклику події (опціонально) / UI button for manual event triggering (optional)")]
            public Button uiButton;
            
            // Внутрішній прапорець для відстеження чи подія була відправлена
            // Internal flag to track if the event has been sent
            [HideInInspector]
            public bool hasBeenTriggered;
        }
        
        [Header("Налаштування подій часу доби / Time of Day Events Setup")]
        [Tooltip("Список подій які викликаються в певні години / List of events triggered at specific hours")]
        public List<TimeOfDayEvent> timeOfDayEvents = new List<TimeOfDayEvent>();
        
        [Header("Налаштування швидкості часу / Time Speed Settings")]
        [Tooltip("UI слайдер для керування швидкістю часу / UI slider for time speed control")]
        public Slider timeSpeedSlider;
        
        [Tooltip("Мінімальна швидкість часу / Minimum time speed")]
        [Range(0f, 1f)]
        public float minTimeSpeed;
        
        [Tooltip("Максимальна швидкість часу / Maximum time speed")]
        [Range(1f, 10f)]
        public float maxTimeSpeed = 5f;
        
        [Tooltip("Початкова швидкість часу / Initial time speed")]
        [Range(0f, 10f)]
        public float initialTimeSpeed = 1f;
          
        // Попередня година для відстеження змін
        // Previous hour to track changes
        private int _previousHour = -1;
        
        // Поточна активна подія (для відстеження завершення)
        // Current active event (for tracking completion)
        private string _currentActiveEvent;
        
        // Кешовані агенти на сцені (для оптимізації)
        // Cached agents in the scene (for optimization)
        private BehaviorGraphAgent[] _cachedAgents;
        
        /// Викликається при ініціалізації об'єкта
        /// Called when the object is initialized
        void Start()
        {
            Debug.Log("⏰ TimeController: Start method called - script is active!");
            Debug.Log($"⏰ TimeController: Initial hour = {hour}");
            
            // Ініціалізуємо швидкість часу
            // Initialize time speed
            Time.timeScale = initialTimeSpeed;
            Debug.Log($"⏱️ Time.timeScale set to: {Time.timeScale}");
            
            // Налаштовуємо UI слайдер
            // Setup UI slider
            if (timeSpeedSlider != null)
            {
                timeSpeedSlider.minValue = minTimeSpeed;
                timeSpeedSlider.maxValue = maxTimeSpeed;
                timeSpeedSlider.value = initialTimeSpeed;
                timeSpeedSlider.onValueChanged.AddListener(OnTimeSpeedChanged);
                Debug.Log($"🎚️ Time speed slider configured: min={minTimeSpeed}, max={maxTimeSpeed}, initial={initialTimeSpeed}");
            }
            else
            {
                Debug.LogWarning("⚠️ Time speed slider is not assigned! Time speed control will not be available.");
            }
            
            // Кешуємо всі агенти на сцені
            // Cache all agents in the scene
            _cachedAgents = FindObjectsByType<BehaviorGraphAgent>(FindObjectsSortMode.None);
            Debug.Log($"🔄 Cached {_cachedAgents.Length} BehaviorGraphAgent(s) in the scene");
            
            // Валідація налаштувань подій
            // Validate event settings
            if (timeOfDayEvents == null || timeOfDayEvents.Count == 0)
            {
                Debug.LogWarning("⚠️ TimeController: No time of day events configured! Please add events to the timeOfDayEvents list.");
            }
            else
            {
                Debug.Log($"📋 TimeController: Configured {timeOfDayEvents.Count} time of day events:");
                int validEvents = 0;
                int invalidEvents = 0;
                
                foreach (var evt in timeOfDayEvents)
                {
                    if (evt.eventChannel == null)
                    {
                        Debug.LogError($"❌ Event '{evt.name}' at hour {evt.triggerHour} has NO EventChannel assigned!");
                        invalidEvents++;
                    }
                    else
                    {
                        Debug.Log($"  ✅ {evt.name} at hour {evt.triggerHour}" + 
                                  (evt.shortcutKey != Key.None ? $" (shortcut: {evt.shortcutKey})" : "") +
                                  $" → EventChannel: {evt.eventChannel.name}");
                        validEvents++;
                    }
                }
                
                Debug.Log($"📊 Events summary: {validEvents} valid, {invalidEvents} invalid");
                
                if (invalidEvents > 0)
                {
                    Debug.LogWarning($"⚠️ Please assign EventChannel assets to {invalidEvents} invalid event(s)!");
                }
            }
            
            // Підписуємося на події UI кнопок
            // Subscribe to UI button events
            if (timeOfDayEvents != null)
            {
                foreach (var evt in timeOfDayEvents)
                {
                    if (evt.uiButton != null)
                    {
                        // Створюємо локальну копію змінної для замикання
                        // Create local copy for closure
                        var eventCopy = evt;
                        evt.uiButton.onClick.AddListener(() => OnUIButtonClicked(eventCopy));
                        Debug.Log($"🖱️ UI Button registered for event: {evt.name}");
                    }
                }
            }
        }
        
        /// Викликається при натисканні на UI кнопку події
        /// Called when an event's UI button is clicked
        private void OnUIButtonClicked(TimeOfDayEvent evt)
        {
            Debug.Log($"🖱️ UI Button clicked for event: {evt.name}");
            TriggerEventManually(evt, "UI button");
        }
        
        /// Викликається при зміні значення слайдера швидкості часу
        /// Called when time speed slider value changes
        private void OnTimeSpeedChanged(float value)
        {
            Time.timeScale = value;
            Debug.Log($"⏱️ Time speed changed to: {value:F2}x (Time.timeScale = {Time.timeScale})");
        }
        
        /// Ручне викликання події (через клавішу або UI кнопку)
        /// Manually trigger an event (via keyboard shortcut or UI button)
        private void TriggerEventManually(TimeOfDayEvent evt, string source)
        {
            // Перевірка чи призначений EventChannel
            // Check if EventChannel is assigned
            if (evt.eventChannel == null)
            {
                Debug.LogError($"❌ Cannot trigger event '{evt.name}' via {source}: EventChannel is NOT assigned!");
                return;
            }
            
            // Встановлюємо годину та викликаємо подію
            // Set hour and trigger event
            hour = evt.triggerHour;
            _hourTimer = 0f; // Скидаємо таймер / Reset timer
            ResetEventFlags(); // Скидаємо прапорці подій / Reset event flags
            evt.hasBeenTriggered = false; // Дозволяємо відправити подію / Allow event to be sent
            TriggerEventByName(evt.name, evt.eventChannel); // Відразу викликаємо подію / Trigger event immediately
            evt.hasBeenTriggered = true; // Позначаємо що подію відправлено / Mark event as sent
            _previousHour = hour; // Оновлюємо попередню годину щоб уникнути повторного виклику / Update previous hour to avoid duplicate trigger
            Debug.Log($"✅ Hour set to: {hour} ({evt.name}) - Event triggered via {source}");
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
                
                Debug.Log($"⏰ Auto-increment: Hour is now {hour}");
            }
            
            // Перевіряємо клавіші для ручного виклику подій
            // Check keyboard shortcuts for manual event triggering
            if (Keyboard.current != null && timeOfDayEvents != null)
            {
                foreach (var evt in timeOfDayEvents)
                {
                    if (evt.shortcutKey != Key.None && Keyboard.current[evt.shortcutKey].wasPressedThisFrame)
                    {
                        Debug.Log($"⌨️ {evt.shortcutKey} key pressed - setting time to {evt.name}!");
                        TriggerEventManually(evt, $"key {evt.shortcutKey}");
                        return; // Виходимо з Update щоб уникнути додаткових перевірок / Exit Update to avoid additional checks
                    }
                }
            }

            // Перевіряємо чи змінилася година
            // Check if the hour has changed
            if (hour != _previousHour)
            {
                Debug.Log($"🔄 Hour changed from {_previousHour} to {hour}");
                _previousHour = hour;
                
                // Скидаємо прапорці для нової години
                // Reset flags for the new hour
                ResetEventFlags();
                
                // Перевірка та виклик подій залежно від години
                // Check and trigger events based on the hour
                if (timeOfDayEvents != null)
                {
                    foreach (var evt in timeOfDayEvents)
                    {
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
            Debug.Log($"🔔 Triggering {eventName} event at hour {hour}");
            
            // Перевірка чи призначений EventChannel
            // Check if EventChannel is assigned
            if (eventChannel == null)
            {
                Debug.LogError($"❌ Cannot trigger {eventName} event: eventChannel is null! Please assign an EventChannel in the Inspector.");
                return;
            }
            
            // Логування зміни події
            // Log event change
            if (_currentActiveEvent != null && _currentActiveEvent != eventName)
            {
                Debug.Log($"⏹️ Stopping previous event: {_currentActiveEvent}");
                
                // КРИТИЧНО: Перезапускаємо ВСІ агенти на сцені щоб зупинити попередню поведінку
                // CRITICAL: Restart ALL agents in the scene to stop previous behavior
                Debug.Log($"🔄 Restarting {_cachedAgents.Length} BehaviorGraphAgent(s) to stop previous behavior");
                
                foreach (var agent in _cachedAgents)
                {
                    if (agent != null && agent.enabled)
                    {
                        Debug.Log($"  ↻ Restarting agent: {agent.gameObject.name}");
                        agent.End();
                        agent.Start();
                    }
                }
            }
            
            // Відправляємо подію через EventChannel
            // All agents listening to this EventChannel will receive it automatically
            // Всі агенти що слухають цей EventChannel отримають подію автоматично
            eventChannel.SendEventMessage(Array.Empty<BlackboardVariable>());
            
            Debug.Log($"✅ {eventName} event sent successfully to all listening agents");
            
            // Встановлюємо поточну активну подію
            // Set current active event
            _currentActiveEvent = eventName;
        }
    }
}