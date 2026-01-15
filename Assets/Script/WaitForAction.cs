using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Script
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Wait for", story: "[Agent] is waiting for a [trigger]", category: "Action", id: "f84e13ddaa2feb6f64b8494bc75b229c")]
    public class WaitForAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> agent;
        [SerializeReference] public BlackboardVariable<GameObject> trigger;
        
        // Відстань, на якій тригер вважається "прибувшим"
        // Distance at which the trigger is considered "arrived"
        [SerializeField] 
        [Tooltip("Мінімальна відстань між агентом і тригером для завершення очікування / Minimum distance between agent and trigger to complete waiting")]
        public float arrivalDistance = 1500.0f;

        protected override Status OnStart()
        {
            // Перевіряємо чи призначені агент і тригер
            // Check if agent and trigger are assigned
            if (agent?.Value == null)
            {
                Debug.LogError("WaitForAction: Agent is not assigned!");
                return Status.Failure;
            }

            if (trigger?.Value == null)
            {
                Debug.LogError("WaitForAction: Trigger is not assigned!");
                return Status.Failure;
            }

            Debug.Log($"{agent.Value.name} started waiting for {trigger.Value.name}");
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            // Перевіряємо чи ще існують об'єкти
            // Check if objects still exist
            if (agent?.Value == null || trigger?.Value == null)
            {
                Debug.LogWarning("WaitForAction: Agent or Trigger became null during execution");
                return Status.Failure;
            }

            // Обчислюємо відстань між агентом і тригером
            // Calculate distance between agent and trigger
            float distance = Vector3.Distance(agent.Value.transform.position, trigger.Value.transform.position);
            
            // Якщо тригер наблизився на потрібну відстань - очікування завершено
            // If trigger came close enough - waiting is complete
            if (distance <= arrivalDistance)
            {
                Debug.Log($"{trigger.Value.name} arrived at {agent.Value.name} (distance: {distance:F2})");
                return Status.Success;
            }

            // Продовжуємо чекати
            // Continue waiting
            return Status.Running;
        }

        protected override void OnEnd()
        {
            // Очищення ресурсів якщо потрібно
            // Cleanup resources if needed
        }
    }
}

