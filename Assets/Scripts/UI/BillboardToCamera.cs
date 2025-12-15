using UnityEngine;

/// <summary>
/// Makes the object always face the main camera (billboard effect).
/// Only rotates on Y axis to keep the sign upright.
/// </summary>
public class BillboardToCamera : MonoBehaviour
{
    [SerializeField] private bool lockX = true;
    [SerializeField] private bool lockZ = true;
    
    private Transform cameraTransform;

    private void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
        {
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;
            else
                return;
        }

        // Calculate direction FROM camera (so the front of the object faces the camera)
        Vector3 directionFromCamera = transform.position - cameraTransform.position;
        
        if (lockX) directionFromCamera.y = 0;
        
        if (directionFromCamera != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionFromCamera);
            
            if (lockX || lockZ)
            {
                Vector3 euler = targetRotation.eulerAngles;
                if (lockX) euler.x = 0;
                if (lockZ) euler.z = 0;
                targetRotation = Quaternion.Euler(euler);
            }
            
            transform.rotation = targetRotation;
        }
    }
}
