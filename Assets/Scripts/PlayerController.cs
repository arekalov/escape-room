using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float mouseSensitivity = 2f;

    [Header("Interaction")]
    public float interactRange = 2.5f;

    float _pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float yaw   = Input.GetAxis("Mouse X") * mouseSensitivity;
        float pitch = Input.GetAxis("Mouse Y") * mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch - pitch, -80f, 80f);
        transform.rotation = Quaternion.Euler(_pitch, transform.eulerAngles.y + yaw, 0f);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = transform.right * h + transform.forward * v;
        dir.y = 0f;
        transform.position += dir * moveSpeed * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.E))
            TryInteract();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void TryInteract()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactRange))
            hit.collider.GetComponentInParent<IInteractable>()?.Interact();
    }

    void OnGUI()
    {
        if (!Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactRange)) return;
        var hint = hit.collider.GetComponentInParent<IInteractable>()?.GetHintText();
        if (string.IsNullOrEmpty(hint)) return;

        var style = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(Screen.width / 2f - 200, Screen.height * 0.6f, 400, 36), hint, style);
    }
}
