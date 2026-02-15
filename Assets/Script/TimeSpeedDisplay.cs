using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Script
{
    public class TimeSpeedDisplay : MonoBehaviour
    {
        [Tooltip("Time speed slider")]
        public Slider timeSpeedSlider;
        
        [Tooltip("Text field to display speed")]
        public TextMeshProUGUI speedText;
        
        [Tooltip("Display format (e.g., '0.0x' or '0.00x')")]
        public string displayFormat = "0.0x";
        
        void Start()
        {
            if (timeSpeedSlider != null)
            {
                timeSpeedSlider.onValueChanged.AddListener(UpdateSpeedDisplay);
                
                UpdateSpeedDisplay(timeSpeedSlider.value);
            }
            else
            {
                Debug.LogWarning("TimeSpeedDisplay: Slider is not assigned!");
            }
        }
        
        private void UpdateSpeedDisplay(float value)
        {
            if (speedText != null)
            {
                speedText.text = value.ToString(displayFormat);
            }
        }
    }
}

