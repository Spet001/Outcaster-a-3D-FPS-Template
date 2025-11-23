using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ClassicFpsController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private bool allowJump = true;

    [Header("View")]
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private float mouseSensitivity = 2.3f;
    [SerializeField] private bool lockCursorOnStart = true;

    private CharacterController controller;
    private Vector3 planarVelocity;
    private float verticalVelocity;
    private float yaw;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraRoot == null && Camera.main != null)
        {
            cameraRoot = Camera.main.transform;
        }

        yaw = transform.rotation.eulerAngles.y;
    }

    private void OnEnable()
    {
        if (lockCursorOnStart)
        {
            SetCursorLock(true);
        }
    }

    private void OnDisable()
    {
        if (lockCursorOnStart)
        {
            SetCursorLock(false);
        }
    }

    private void Update()
    {
        HandleLook();
        HandleMove();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        yaw += mouseX;
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        if (cameraRoot != null)
        {
            // Classic Doom never tilts the camera up/down, so keep local rotation flat.
            cameraRoot.localRotation = Quaternion.identity;
        }
    }

    private void HandleMove()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input = Vector2.ClampMagnitude(input, 1f);

        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        Vector3 desiredPlanar = (transform.right * input.x + transform.forward * input.y) * targetSpeed;
        planarVelocity = Vector3.Lerp(planarVelocity, desiredPlanar, acceleration * Time.deltaTime);

        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            if (allowJump && Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 motion = new Vector3(planarVelocity.x, verticalVelocity, planarVelocity.z);
        controller.Move(motion * Time.deltaTime);
    }

    public void SetCursorLock(bool shouldLock)
    {
        Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !shouldLock;
    }
}
