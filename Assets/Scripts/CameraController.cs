using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    // Editable Data
    public float speed;
    public float scrollSpeed;
    public float maxYPosition = 5;
    public float minYPosition = 1;
    public float maxAngle = 75f;
    public float minAngle = 5f;

    // Inputs
    private Vector3 movementInput;
    private float scrollInput;
    
    // Internal variables
    private float targetY;
    private float yVelocity;

    private void Start()
    {
        targetY = transform.position.y;
    }

    void Update()
    {
        // Get basic movement inputs
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.z = Input.GetAxisRaw("Vertical");

        scrollInput = Input.mouseScrollDelta.y;

        targetY += scrollInput * scrollSpeed * Time.fixedDeltaTime;
        targetY = Mathf.Clamp(targetY, minYPosition, maxYPosition);

        var position = transform.position;
        transform.position = new Vector3(position.x, Mathf.SmoothDamp(position.y, targetY, ref yVelocity, 0.1f), position.z);
        transform.rotation = Quaternion.Euler(GetCurrentAngle(), 0, transform.rotation.z);
    }

    private void FixedUpdate()
    {
        Transform thisTransform = transform;

        Vector3 up = transform.worldToLocalMatrix.MultiplyVector(Vector3.up);
        Vector3 movement = Vector3.ProjectOnPlane(movementInput, up).normalized * speed;

        thisTransform.Translate(movement * Time.fixedDeltaTime);
    }

    private float GetCurrentAngle()
    {
        float coef = (maxAngle - minAngle) / (maxYPosition - minYPosition);
        float yPos = transform.position.y;
        
        return yPos * coef + minAngle - coef;
    }
}
