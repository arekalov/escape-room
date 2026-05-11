using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float gravity   = -15f;

    [Header("Look")]
    public float sensitivity = 0.2f;
    public float maxPitch    = 80f;

    [Header("References")]
    public Transform cameraRoot;

    CharacterController _cc;
    InputAction _moveAction;
    InputAction _lookAction;
    float _pitch;
    float _yVelocity;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();

        _moveAction = new InputAction("Move", InputActionType.Value);
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/w")
            .With("Down",  "<Keyboard>/s")
            .With("Left",  "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        _moveAction.Enable();

        _lookAction = new InputAction("Look", InputActionType.Value);
        _lookAction.AddBinding("<Mouse>/delta");
        _lookAction.Enable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }

    void HandleLook()
    {
        var delta = _lookAction.ReadValue<Vector2>();
        transform.Rotate(Vector3.up * delta.x * sensitivity);
        _pitch = Mathf.Clamp(_pitch - delta.y * sensitivity, -maxPitch, maxPitch);
        if (cameraRoot != null)
            cameraRoot.localEulerAngles = new Vector3(_pitch, 0f, 0f);
    }

    void HandleMovement()
    {
        var input = _moveAction.ReadValue<Vector2>();
        Vector3 move = (transform.right * input.x + transform.forward * input.y) * moveSpeed;

        if (_cc.isGrounded && _yVelocity < 0f) _yVelocity = -2f;
        _yVelocity += gravity * Time.deltaTime;
        move.y = _yVelocity;

        _cc.Move(move * Time.deltaTime);
        AudioManager.SetFootsteps(input.magnitude > 0.1f && _cc.isGrounded);
    }

    void OnDestroy()
    {
        _moveAction?.Dispose();
        _lookAction?.Dispose();
    }
}
