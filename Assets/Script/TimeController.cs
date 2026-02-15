using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Behavior;
using UnityEngine.UI;

namespace Script
{
    
    public class TimeController : MonoBehaviour
    {
        [Range(0, 23)] public int hour;
        
        private float _hourTimer;
        
        [Tooltip("Time in seconds between automatic hour increments")]
        public float hourIncrementInterval = 10f;
        
        [Serializable]
        public class TimeOfDayEvent
        {
            [Tooltip("Event name")]
            public string name;
            
            [Tooltip("Event channel to trigger")]
            public EventChannel eventChannel;
            
            [Tooltip("Hour when the event is triggered (0-23)")]
            public int triggerHour;
            
            [Tooltip("Key for manual event triggering (optional)")]
            public Key shortcutKey = Key.None;
            
            [Tooltip("UI button for manual event triggering (optional)")]
            public Button uiButton;
            
            [HideInInspector]
            public bool hasBeenTriggered;
        }
        
        [Header("Time of Day Events Setup")]
        [Tooltip("List of events triggered at specific hours")]
        public List<TimeOfDayEvent> timeOfDayEvents = new List<TimeOfDayEvent>();
        
        [Header("Time Speed Settings")]
        [Tooltip("UI slider for time speed control")]
        public Slider timeSpeedSlider;
        
        [Tooltip("Minimum time speed")]
        [Range(0f, 1f)]
        public float minTimeSpeed;
        
        [Tooltip("Maximum time speed")]
        [Range(1f, 10f)]
        public float maxTimeSpeed = 5f;
        
        [Tooltip("Initial time speed")]
        [Range(0f, 10f)]
        public float initialTimeSpeed = 1f;
        
        [Header("Sun Settings")]
        [Tooltip("Sun transform for rotation")]
        public Transform sunTransform;
        
        [Tooltip("Sun rotation axis")]
        public Vector3 sunRotationAxis = Vector3.right;
        
        [Tooltip("Initial sun rotation angle")]
        public float sunInitialRotation;
        
        [Header("Time Display Settings")]
        [Tooltip("UI text field to display time")]
        public TMPro.TextMeshProUGUI timeDisplayText;
          
        private int _previousHour = -1;
        
        private string _currentActiveEvent;
        
        private BehaviorGraphAgent[] _cachedAgents;
        
        void Start()
        {
            Debug.Log(" TimeController: Start method called - script is active!");
            Debug.Log($" TimeController: Initial hour = {hour}");
            
            Time.timeScale = initialTimeSpeed;
            Debug.Log($"Time.timeScale set to: {Time.timeScale}");
            
            if (timeSpeedSlider != null)
            {
                timeSpeedSlider.minValue = minTimeSpeed;
                timeSpeedSlider.maxValue = maxTimeSpeed;
                timeSpeedSlider.value = initialTimeSpeed;
                timeSpeedSlider.onValueChanged.AddListener(OnTimeSpeedChanged);
                Debug.Log($"Time speed slider configured: min={minTimeSpeed}, max={maxTimeSpeed}, initial={initialTimeSpeed}");
            }
            else
            {
                Debug.LogWarning("Time speed slider is not assigned! Time speed control will not be available.");
            }
            
            _cachedAgents = FindObjectsByType<BehaviorGraphAgent>(FindObjectsSortMode.None);
            Debug.Log($"Cached {_cachedAgents.Length} BehaviorGraphAgent(s) in the scene");
            
            if (timeOfDayEvents == null || timeOfDayEvents.Count == 0)
            {
                Debug.LogWarning("TimeController: No time of day events configured! Please add events to the timeOfDayEvents list.");
            }
            else
            {
                Debug.Log($"TimeController: Configured {timeOfDayEvents.Count} time of day events:");
                int validEvents = 0;
                int invalidEvents = 0;
                
                foreach (var evt in timeOfDayEvents)
                {
                    if (evt.eventChannel == null)
                    {
                        Debug.LogError($"Event '{evt.name}' at hour {evt.triggerHour} has NO EventChannel assigned!");
                        invalidEvents++;
                    }
                    else
                    {
                        Debug.Log($"{evt.name} at hour {evt.triggerHour}" + 
                                  (evt.shortcutKey != Key.None ? $" (shortcut: {evt.shortcutKey})" : "") +
                                  $" → EventChannel: {evt.eventChannel.name}");
                        validEvents++;
                    }
                }
                
                Debug.Log($"Events summary: {validEvents} valid, {invalidEvents} invalid");
                
                if (invalidEvents > 0)
                {
                    Debug.LogWarning($"Please assign EventChannel assets to {invalidEvents} invalid event(s)!");
                }
            }
            
            if (timeOfDayEvents != null)
            {
                foreach (var evt in timeOfDayEvents)
                {
                    if (evt.uiButton != null)
                    {
                        var eventCopy = evt;
                        evt.uiButton.onClick.AddListener(() => OnUIButtonClicked(eventCopy));
                        Debug.Log($"🖱️ UI Button registered for event: {evt.name}");
                    }
                }
            }
        }
        
        private void OnUIButtonClicked(TimeOfDayEvent evt)
        {
            Debug.Log($"UI Button clicked for event: {evt.name}");
            TriggerEventManually(evt, "UI button");
        }
        
        private void OnTimeSpeedChanged(float value)
        {
            Time.timeScale = value;
            Debug.Log($"Time speed changed to: {value:F2}x (Time.timeScale = {Time.timeScale})");
        }
        
        private void TriggerEventManually(TimeOfDayEvent evt, string source)
        {
            if (evt.eventChannel == null)
            {
                Debug.LogError($"Cannot trigger event '{evt.name}' via {source}: EventChannel is NOT assigned!");
                return;
            }
            
            hour = evt.triggerHour;
            _hourTimer = 0f;
            ResetEventFlags();
            evt.hasBeenTriggered = false;
            TriggerEventByName(evt.name, evt.eventChannel);
            evt.hasBeenTriggered = true;
            _previousHour = hour;
            Debug.Log($"Hour set to: {hour} ({evt.name}) - Event triggered via {source}");
        }
        
        void Update()
        {
            _hourTimer += Time.deltaTime;
            
            UpdateSunRotation();
            
            UpdateTimeDisplay();
            
            if (_hourTimer >= hourIncrementInterval)
            {
                _hourTimer = 0f;
                hour++;
                
                hour = hour % 24;
                
                Debug.Log($"Auto-increment: Hour is now {hour}");
            }
            
            if (Keyboard.current != null && timeOfDayEvents != null)
            {
                foreach (var evt in timeOfDayEvents)
                {
                    if (evt.shortcutKey != Key.None && Keyboard.current[evt.shortcutKey].wasPressedThisFrame)
                    {
                        Debug.Log($"{evt.shortcutKey} key pressed - setting time to {evt.name}!");
                        TriggerEventManually(evt, $"key {evt.shortcutKey}");
                        return;
                    }
                }
            }

            if (hour != _previousHour)
            {
                Debug.Log($"Hour changed from {_previousHour} to {hour}");
                _previousHour = hour;
                
                ResetEventFlags();
                
                if (timeOfDayEvents != null)
                {
                    foreach (var evt in timeOfDayEvents)
                    {
                        if (hour == evt.triggerHour && !evt.hasBeenTriggered)
                        {
                            evt.hasBeenTriggered = true;
                            TriggerEventByName(evt.name, evt.eventChannel);
                        }
                    }
                }
            }
        }
        
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
        
        private void TriggerEventByName(string eventName, EventChannel eventChannel)
        {
            Debug.Log($"Triggering {eventName} event at hour {hour}");
            
            if (eventChannel == null)
            {
                Debug.LogError($"Cannot trigger {eventName} event: eventChannel is null! Please assign an EventChannel in the Inspector.");
                return;
            }
            
            if (_currentActiveEvent != null && _currentActiveEvent != eventName)
            {
                Debug.Log($"Stopping previous event: {_currentActiveEvent}");
                
                Debug.Log($"Restarting {_cachedAgents.Length} BehaviorGraphAgent(s) to stop previous behavior");
                
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
            

            eventChannel.SendEventMessage(Array.Empty<BlackboardVariable>());
            
            Debug.Log($"{eventName} event sent successfully to all listening agents");
            
            _currentActiveEvent = eventName;
        }
        
        private void UpdateSunRotation()
        {
            if (sunTransform == null)
            {
                return;
            }

            float dayProgress = (hour + _hourTimer / hourIncrementInterval) / 24.0f;
            
            float rotationAngle = dayProgress * 360f + sunInitialRotation;
            
            sunTransform.rotation = Quaternion.Euler(sunRotationAxis * rotationAngle);
        }
        
        private void UpdateTimeDisplay()
        {
            if (timeDisplayText == null)
            {
                return;
            }
            
            int minutes = Mathf.FloorToInt((_hourTimer / hourIncrementInterval) * 60f);
            
            timeDisplayText.text = $"{hour:D2}:{minutes:D2}";
        }
    }
}