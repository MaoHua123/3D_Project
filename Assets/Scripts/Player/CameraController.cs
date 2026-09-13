using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform followTarget;

    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float distance = 5f;

    [SerializeField] private float minVerticalAngle = -10;
    [SerializeField] private float maxVerticalAngle = 45;

    [SerializeField] private Vector2 framingOffset;

    [SerializeField] private bool invertX = false;
    [SerializeField] private bool invertY = true;

    [SerializeField] private LayerMask obstaclelayer;
    [SerializeField] private float cameraRadius = 0.2f;
    [SerializeField] private float wallOffset = 0.15f;
    private float currentDistance;
    
    private float rotationX;
    private float rotationY;

    private float invertXVal;
    private float invertYVal;
    

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        currentDistance = distance;
    }

    private void Update()
    {
        invertXVal = invertX ? -1 : 1;
        invertYVal = invertY ? -1 : 1;
        
        rotationX += Input.GetAxis("Mouse Y") * invertYVal * rotationSpeed;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

        rotationY += Input.GetAxis("Mouse X") * invertXVal * rotationSpeed;
    }

    private void LateUpdate()
    {
        var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);
        var focusPosition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y);

        Vector3 desiredCamPos = focusPosition - targetRotation * new Vector3(0, 0, distance);
        Vector3 dirToCam = desiredCamPos - focusPosition;
        float desiredLen = dirToCam.magnitude;
       if(Physics.SphereCast(focusPosition, cameraRadius, dirToCam, out RaycastHit hit, desiredLen, obstaclelayer))
        {
            currentDistance = hit.distance - wallOffset;
        }
        else
        {
            currentDistance = distance;
        }
       
        transform.position = focusPosition - targetRotation * new Vector3(0, 0, currentDistance);
        transform.rotation = targetRotation;

        Debug.DrawLine(focusPosition, focusPosition + dirToCam.normalized * desiredLen, Color.red, Time.deltaTime);
    }

    public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);
}
