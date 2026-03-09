using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using TMPro;

namespace Script
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Talk", story: "[Agent] says a [sentence]", category: "Action", id: "c358a4937d65c81e8271e8c8628f1413")]
    public partial class TalkAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<string> Sentence;
        
        [SerializeField] 
        [Tooltip("Duration to display text (seconds)")]
        public float displayDuration = 3f;
    
        private TextMeshProUGUI _textComponent;
        private Canvas _canvas;
        private float _timer;
        private string _previousText;

    protected override Status OnStart()
    {
        if (Agent.Value == null)
        {
            Debug.LogError("TalkAction: Agent is null!");
            return Status.Failure;
        }

        _canvas = Agent.Value.GetComponentInChildren<Canvas>();
        
        if (_canvas == null)
        {
            Debug.LogError($"TalkAction: No Canvas found on agent {Agent.Value.name}!");
            return Status.Failure;
        }

        _textComponent = _canvas.GetComponentInChildren<TextMeshProUGUI>();
        
        if (_textComponent == null)
        {
            Debug.LogError($"TalkAction: No TextMeshProUGUI component found in Canvas on {Agent.Value.name}!");
            return Status.Failure;
        }

        _previousText = _textComponent.text;
        
        if (!string.IsNullOrEmpty(_previousText))
        {
            _textComponent.text = _previousText + "\n" + Sentence.Value;
        }
        else
        {
            _textComponent.text = Sentence.Value;
        }
        
        _canvas.gameObject.SetActive(true);
        _timer = 0f;
        
        Debug.Log($"{Agent.Value.name} says: \"{Sentence.Value}\"");
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value == null || _canvas == null || _textComponent == null)
        {
            return Status.Failure;
        }

        _timer += Time.deltaTime;
        
        if (_timer >= displayDuration)
        {
            Debug.Log($"{Agent.Value.name} finished talking after {_timer:F2} seconds");
            
            _textComponent.text = _previousText;
            
            if (string.IsNullOrEmpty(_previousText))
            {
                _canvas.gameObject.SetActive(false);
            }
            
            return Status.Success;
        }
        
        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_textComponent != null)
        {
            _textComponent.text = _previousText;
        }
        
        if (_canvas != null && string.IsNullOrEmpty(_previousText))
        {
            _canvas.gameObject.SetActive(false);
        }
        
        _timer = 0f;
    }
}
}

