using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Script
{
    /// Відображає поточну швидкість часу поруч зі слайдером
    /// Displays current time speed next to the slider
    public class TimeSpeedDisplay : MonoBehaviour
    {
        [Tooltip("Слайдер швидкості часу / Time speed slider")]
        public Slider timeSpeedSlider;
        
        [Tooltip("Текстове поле для відображення швидкості / Text field to display speed")]
        public TextMeshProUGUI speedText;
        
        [Tooltip("Формат відображення (наприклад: '0.0x' або '0.00x') / Display format (e.g., '0.0x' or '0.00x')")]
        public string displayFormat = "0.0x";
        
        void Start()
        {
            if (timeSpeedSlider != null)
            {
                // Підписуємось на зміну значення слайдера
                // Subscribe to slider value changes
                timeSpeedSlider.onValueChanged.AddListener(UpdateSpeedDisplay);
                
                // Відображаємо початкове значення
                // Display initial value
                UpdateSpeedDisplay(timeSpeedSlider.value);
            }
            else
            {
                Debug.LogWarning("⚠️ TimeSpeedDisplay: Slider is not assigned!");
            }
        }
        
        /// Оновлює текстове поле зі швидкістю
        /// Updates the speed text field
        private void UpdateSpeedDisplay(float value)
        {
            if (speedText != null)
            {
                speedText.text = value.ToString(displayFormat);
            }
        }
    }
}

