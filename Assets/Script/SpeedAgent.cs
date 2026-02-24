using UnityEngine;

namespace Script
{
    public class SpeedFarmer : MonoBehaviour
    {
        private static readonly int SppedProperty = Animator.StringToHash("Speed");

        [Header("Speed Settings")]
        [Tooltip("Agent to track")]
        public Transform agent;

        public Animator anim;
        
        [Tooltip("Current speed (0 or 1)")]
        [SerializeField]
        private int speed;
        
        [Header("Debug")]
        [Tooltip("Show debug logs")]
        public bool showDebugLogs;
        
        private Vector3 _previousPosition;
        
        private const float MovementThreshold = 0.001f;
        
        public int Speed => speed;
        
        void Start()
        {
            if (agent == null)
            {
                agent = transform;
                if (showDebugLogs)
                {
                    Debug.Log($"SpeedFarmer: Agent not assigned, using self: {gameObject.name}");
                }
            }
            
            anim = GetComponent<Animator>();
            
            _previousPosition = agent.position;
        }
        
        void Update()
        {
            if (agent == null)
            {
                Debug.LogWarning("SpeedFarmer: Agent is null!");
                return;
            }
            
            Vector3 currentPosition = agent.position;
            
            float positionChange = Vector3.Distance(currentPosition, _previousPosition);
            
            int newSpeed = positionChange > MovementThreshold ? 1 : 0;
            
            speed = newSpeed;
            
            anim.SetFloat(SppedProperty, speed);
            
            if (showDebugLogs)
            {
                Debug.Log($"SpeedFarmer [{agent.name}] Update: Position={currentPosition}, " +
                          $"Previous={_previousPosition}, Change={positionChange:F6}, Speed={speed}, Time={Time.time:F2}");
            }
            
            _previousPosition = currentPosition;
        }
        
        public bool IsMoving()
        {
            return speed == 1;
        }
        
        public void ForceUpdateSpeed()
        {
            if (agent == null)
            {
                Debug.LogWarning("SpeedFarmer: Cannot force update - agent is null!");
                return;
            }
            
            Vector3 currentPosition = agent.position;
            float positionChange = Vector3.Distance(currentPosition, _previousPosition);
            int newSpeed = positionChange > MovementThreshold ? 1 : 0;
            speed = newSpeed;
            _previousPosition = currentPosition;
            
            Debug.Log($"SpeedFarmer [{agent.name}] Force Update: Speed={speed}, Change={positionChange:F6}");
        }
        
        void OnEnable()
        {
            if (showDebugLogs)
            {
                Debug.Log($"SpeedFarmer [{gameObject.name}]: Component enabled at {Time.time:F2}");
            }
        }
        
        void OnDisable()
        {
            if (showDebugLogs)
            {
                Debug.Log($"SpeedFarmer [{gameObject.name}]: Component disabled at {Time.time:F2}");
            }
        }
    }
}