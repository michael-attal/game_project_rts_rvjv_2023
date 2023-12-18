using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public float speed;
    public float scrollSpeed;
    public float maxYPosition;
    public float minYPosition;
    public float maxAngle;
    public float minAngle;

    private Vector3 movementInput;
    private float scrollInput;
    
    void Update()
    {
        // Get basic movement inputs
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.z = Input.GetAxisRaw("Vertical");

        scrollInput = Input.mouseScrollDelta.y;
    }

    private void FixedUpdate()
    {
        Transform thisTransform = transform;

        Vector3 up = transform.worldToLocalMatrix.MultiplyVector(Vector3.up);
        Vector3 movement = scrollInput * scrollSpeed * up + Vector3.ProjectOnPlane(movementInput, up).normalized * speed;
        
        thisTransform.Translate(movement * Time.fixedDeltaTime);

        // Clamp the y position
        var position = transform.position;
        float yPos = Mathf.Clamp(position.y , minYPosition, maxYPosition);
        transform.position = new Vector3(position.x, yPos, position.z);
        
        transform.rotation = Quaternion.Euler(GetCurrentAngle(), 0, thisTransform.rotation.z);
    }

    private float GetCurrentAngle()
    {
        float coef = (maxAngle - minAngle) / (maxYPosition - minYPosition);
        float yPos = transform.position.y;
        
        return yPos * coef + minAngle - coef;
    }
}
