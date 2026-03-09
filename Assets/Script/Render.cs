using UnityEngine;

namespace Script
{
    public class Render : MonoBehaviour
    {
        [Header("Debug Settings")]
        [Tooltip("Show debug messages")]
        public bool showDebug = true;

        void OnTriggerEnter(Collider other)
        {
            if (showDebug)
            {
                Debug.Log($"[Render] Trigger Enter - Object: {other.gameObject.name}, Tag: '{other.tag}', Has Farmer tag: {other.CompareTag("Farmer")}");
                
                Renderer[] renderers = other.GetComponentsInChildren<Renderer>();
                Debug.Log($"[Render] Found {renderers.Length} renderers on {other.gameObject.name}");
            }
            
            if(other.CompareTag("Farmer"))
            {
                if (showDebug) Debug.Log("[Render] Hiding Farmer renderers");
                SetRenderersEnabled(other.gameObject, false);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (showDebug)
            {
                Debug.Log($"[Render] Trigger Exit - Object: {other.gameObject.name}, Tag: '{other.tag}'");
            }
            
            if(other.CompareTag("Farmer"))
            {
                if (showDebug) Debug.Log("[Render] Showing Farmer renderers");
                SetRenderersEnabled(other.gameObject, true);
            }
        }

        private void SetRenderersEnabled(GameObject target, bool isEnabled)
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
            
            if (showDebug)
            {
                Debug.Log($"[Render] Found {renderers.Length} renderers on {target.name}, setting enabled={isEnabled}");
                foreach (Renderer rend in renderers)
                {
                    Debug.Log($"[Render]   - {rend.gameObject.name}: {rend.GetType().Name}");
                }
            }
            
            foreach (Renderer rend in renderers)
            {
                rend.enabled = isEnabled;
            }
        }
    }
}
