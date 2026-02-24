using UnityEngine;

namespace Script
{
    public class BillboardCanvas : MonoBehaviour
    {
        private Camera _mainCamera;

        void Start()
        {
            _mainCamera = Camera.main;
            
            if (_mainCamera == null)
            {
                Debug.LogError("BillboardCanvas: Main camera not found!");
            }
        }

        void LateUpdate()
        {
            if (_mainCamera == null)
            {
                return;
            }

            transform.LookAt(transform.position + _mainCamera.transform.forward);
        }
    }
}

