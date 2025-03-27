using UnityEngine;
using UnityEngine.InputSystem;

public class cameraMovement : MonoBehaviour
{
    public float moveSpeed = 5f;   // Movement speed
    public float lookSpeed = 2f;   // Look sensitivity

    private Vector2 moveInput = Vector2.zero;
    private Vector2 lookInput = Vector2.zero;
    
    private float verticalRotation = 0f; // Clamping for up/down look

    private PlayerControls controls; // Reference to PlayerControls

    private void Awake()
    {
        controls = new PlayerControls(); // Initialize the input system
    }

    private void OnEnable()
    {
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero; // Stop movement when key is released

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero; // Stop looking when input stops

        controls.Enable(); // Enable input
    }

    private void OnDisable()
    {
        controls.Disable(); // Disable input when object is disabled
    }

    private void Update()
    {
        // Move camera with WASD
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // Rotate camera with mouse
        float mouseX = lookInput.x * lookSpeed;
        float mouseY = lookInput.y * lookSpeed;

        // Horizontal rotation
        transform.Rotate(Vector3.up * mouseX, Space.World);

        // Vertical rotation (clamped)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(verticalRotation, transform.localEulerAngles.y, 0);
    }
}