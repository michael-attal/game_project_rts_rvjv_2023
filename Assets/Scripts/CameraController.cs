using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public Camera camera;
    public float speed;

    private Vector3 movementInput;
    private float scrollInput;

    private float lookX = 0;
    private float lookY = 0;
    
    void Update()
    {
        // Get basic movement inputs
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.z = Input.GetAxisRaw("Vertical");

        scrollInput = Input.mouseScrollDelta.y;
        
        // Handle mouse for rotation inputs
        if (Input.GetButton("Fire1"))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            lookX -= Input.GetAxisRaw("Mouse Y");
            lookY += Input.GetAxisRaw("Mouse X");
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void FixedUpdate()
    {
        Transform thisTransform = transform;

        Vector3 up = transform.worldToLocalMatrix.MultiplyVector(Vector3.up);
        Vector3 movement = Vector3.ProjectOnPlane(movementInput, up).normalized * speed;

        thisTransform.Translate(movement * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(lookX, lookY, thisTransform.rotation.z);
    }
}
