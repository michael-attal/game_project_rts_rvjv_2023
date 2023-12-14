using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public Camera camera;
    public float speed;

    private float angleAroundFocusPoint;
    private Vector3 movementInput;
    private Vector2 mouseInput;
    private float scrollInput;
    private bool lookingAround;

    private float lookX = 0;
    private float lookY = 0;
    
    void Update()
    {
        // Get basic movement inputs
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.z = Input.GetAxisRaw("Vertical");
        movementInput.Normalize();

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
        
        // TODO: This isn't properly moving with rotations. Have to find a way to make movement inputs local somehow
        Vector3 movement = movementInput * speed;
        movement += thisTransform.forward * (scrollInput * speed);

        thisTransform.Translate(movement * Time.fixedDeltaTime, Space.World);
        transform.rotation = Quaternion.Euler(lookX, lookY, thisTransform.rotation.z);
    }
}
