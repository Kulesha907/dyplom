using UnityEngine;
using UnityEngine.UI;

namespace Script
{
    /// Керує кнопками швидкого встановлення швидкості часу
    /// Manages quick time speed preset buttons
    public class TimeSpeedButtons : MonoBehaviour
    {
        [Header("Посилання / References")]
        [Tooltip("Слайдер швидкості часу / Time speed slider")]
        public Slider timeSpeedSlider;
        
        [Header("Кнопки швидкості / Speed Buttons")]
        [Tooltip("Кнопка паузи / Pause button")]
        public Button pauseButton;
        
        [Tooltip("Кнопка нормальної швидкості / Normal speed button")]
        public Button normalButton;
        
        [Tooltip("Кнопка подвійної швидкості / Double speed button")]
        public Button fastButton;
        
        [Tooltip("Кнопка максимальної швидкості / Ultra speed button")]
        public Button ultraButton;
        
        [Header("Значення швидкості / Speed Values")]
        [Tooltip("Швидкість паузи / Pause speed")]
        public float pauseSpeed;
        
        [Tooltip("Нормальна швидкість / Normal speed")]
        public float normalSpeed = 1f;
        
        [Tooltip("Швидка швидкість / Fast speed")]
        public float fastSpeed = 2f;
        
        [Tooltip("Ультра швидкість / Ultra speed")]
        public float ultraSpeed = 5f;
        
        void Start()
        {
            // Перевірка чи призначений слайдер
            // Check if slider is assigned
            if (timeSpeedSlider == null)
            {
                Debug.LogError("❌ TimeSpeedButtons: Slider is not assigned!");
                return;
            }
            
            // Підписуємось на події кнопок
            // Subscribe to button events
            if (pauseButton != null)
                pauseButton.onClick.AddListener(() => SetTimeSpeed(pauseSpeed));
            
            if (normalButton != null)
                normalButton.onClick.AddListener(() => SetTimeSpeed(normalSpeed));
            
            if (fastButton != null)
                fastButton.onClick.AddListener(() => SetTimeSpeed(fastSpeed));
            
            if (ultraButton != null)
                ultraButton.onClick.AddListener(() => SetTimeSpeed(ultraSpeed));
            
            Debug.Log("✅ TimeSpeedButtons: All buttons configured successfully");
        }
        
        /// Встановлює швидкість часу через слайдер
        /// Sets time speed through the slider
        private void SetTimeSpeed(float speed)
        {
            if (timeSpeedSlider != null)
            {
                timeSpeedSlider.value = speed;
                Debug.Log($"🎚️ Time speed set to: {speed}x via button");
            }
        }
        
        /// Публічний метод для встановлення швидкості (можна викликати з UnityEvent)
        /// Public method to set speed (can be called from UnityEvent)
        public void SetPause() => SetTimeSpeed(pauseSpeed);
        public void SetNormal() => SetTimeSpeed(normalSpeed);
        public void SetFast() => SetTimeSpeed(fastSpeed);
        public void SetUltra() => SetTimeSpeed(ultraSpeed);
    }
}

