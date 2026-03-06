using UnityEngine;

namespace Script
{
    public class BillboardCanvas : MonoBehaviour
    {
        void LateUpdate()
        {
            Camera activeCamera = GetActiveCamera();
            
            if (activeCamera == null)
            {
                return;
            }

            transform.LookAt(transform.position + activeCamera.transform.forward);
        }
        
        private Camera GetActiveCamera()
        {
            Camera[] allCameras = Camera.allCameras;
            
            foreach (Camera cam in allCameras)
            {
                if (cam.enabled && cam.gameObject.activeInHierarchy)
                {
                    return cam;
                }
            }
            
            return Camera.main;
        }
    }
}

