using UnityEngine;
using UnityEngine.UI;

namespace Script
{
    public class TimeSpeedButtons : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Time speed slider")]
        public Slider timeSpeedSlider;
        
        [Header("Speed Buttons")]
        [Tooltip("Pause button")]
        public Button pauseButton;
        
        [Tooltip("Normal speed button")]
        public Button normalButton;
        
        [Tooltip("Double speed button")]
        public Button fastButton;
        
        [Tooltip("Ultra speed button")]
        public Button ultraButton;
        
        [Header("Speed Values")]
        [Tooltip("Pause speed")]
        public float pauseSpeed;
        
        [Tooltip("Normal speed")]
        public float normalSpeed = 1f;
        
        [Tooltip("Fast speed")]
        public float fastSpeed = 2f;
        
        [Tooltip("Ultra speed")]
        public float ultraSpeed = 5f;
        
        void Start()
        {
            if (timeSpeedSlider == null)
            {
                Debug.LogError("TimeSpeedButtons: Slider is not assigned!");
                return;
            }
            
            if (pauseButton != null)
                pauseButton.onClick.AddListener(() => SetTimeSpeed(pauseSpeed));
            
            if (normalButton != null)
                normalButton.onClick.AddListener(() => SetTimeSpeed(normalSpeed));
            
            if (fastButton != null)
                fastButton.onClick.AddListener(() => SetTimeSpeed(fastSpeed));
            
            if (ultraButton != null)
                ultraButton.onClick.AddListener(() => SetTimeSpeed(ultraSpeed));
            
            Debug.Log("TimeSpeedButtons: All buttons configured successfully");
        }
        
        private void SetTimeSpeed(float speed)
        {
            if (timeSpeedSlider != null)
            {
                timeSpeedSlider.value = speed;
                Debug.Log($"🎚Time speed set to: {speed}x via button");
            }
        }
        
        public void SetPause() => SetTimeSpeed(pauseSpeed);
        public void SetNormal() => SetTimeSpeed(normalSpeed);
        public void SetFast() => SetTimeSpeed(fastSpeed);
        public void SetUltra() => SetTimeSpeed(ultraSpeed);
    }
}

