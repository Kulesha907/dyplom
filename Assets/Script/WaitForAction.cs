using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace Script
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Wait for", story: "[Agent] waits until the [trigger] is near the [object]", category: "Action", id: "f84e13ddaa2feb6f64b8494bc75b229c")]
    public class WaitForAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> agent;
        [SerializeReference] public BlackboardVariable<GameObject> trigger;
        [SerializeReference] public BlackboardVariable<GameObject> @object;
        
        // Відстань, на якій тригер вважається "прибувшим"
        // Distance at which the trigger is considered "arrived"
        [SerializeField] 
        [Tooltip("Мінімальна відстань між тригером і об'єктом для завершення очікування / Minimum distance between trigger and object to complete waiting")]
        public float arrivalDistance = 5.0f;

        protected override Status OnStart()
        {
            // Перевіряємо чи призначені агент, тригер і об'єкт
            // Check if agent, trigger and object are assigned
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

            if (@object?.Value == null)
            {
                Debug.LogError("WaitForAction: Object is not assigned!");
                return Status.Failure;
            }

            Debug.Log($"{agent.Value.name} started waiting for {trigger.Value.name} to arrive at {@object.Value.name}");
            return Status.Running;
        }

        protected override Status OnUpdate()
        {
            // Перевіряємо чи ще існують об'єкти
            // Check if objects still exist
            if (agent?.Value == null || trigger?.Value == null || @object?.Value == null)
            {
                Debug.LogWarning("WaitForAction: Agent, Trigger or Object became null during execution");
                return Status.Failure;
            }

            // Обчислюємо відстань між тригером і об'єктом
            // Calculate distance between trigger and object
            float distance = Vector3.Distance(trigger.Value.transform.position, @object.Value.transform.position);
            
            // Якщо тригер наблизився до об'єкту на потрібну відстань - очікування завершено
            // If trigger came close enough to the object - waiting is complete
            if (distance <= arrivalDistance)
            {
                Debug.Log($"{trigger.Value.name} arrived at {@object.Value.name} (distance: {distance:F2})");
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

