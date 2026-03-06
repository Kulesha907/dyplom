using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchCamera : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;
    
    private void Start()
    {
        if (camera1 != null)
            camera1.gameObject.SetActive(true);
        if (camera2 != null)
            camera2.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            if (camera1 != null && camera2 != null)
            {
                camera1.gameObject.SetActive(!camera1.gameObject.activeSelf);
                camera2.gameObject.SetActive(!camera2.gameObject.activeSelf);
            }
        }
    }
}
