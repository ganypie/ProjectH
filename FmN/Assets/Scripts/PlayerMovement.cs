using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    private float currentSpeed;

    [Header("Jumping & Gravity")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    private float verticalVelocity;
    private bool isGrounded;

    [Header("Head Bobbing")]
    public Camera playerCamera;
    public float bobFrequency = 3f;  // частота покачивания
    public float bobAmplitude = 0.05f; // амплитуда покачивания
    private float bobTimer = 0f;
    private Vector3 camStartPos;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        camStartPos = playerCamera.transform.localPosition;
    }

    void Update()
    {
        // Проверка на землю
        isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // фикс для стабильного приземления
        }

        // Бег
        bool isRunning = UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed;
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Движение по осям
        float moveX = UnityEngine.InputSystem.Keyboard.current.aKey.isPressed ? -1 :
                      UnityEngine.InputSystem.Keyboard.current.dKey.isPressed ? 1 : 0;
        float moveZ = UnityEngine.InputSystem.Keyboard.current.sKey.isPressed ? -1 :
                      UnityEngine.InputSystem.Keyboard.current.wKey.isPressed ? 1 : 0;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Прыжок
        if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Гравитация
        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);

        // Покачивание головы
        HandleHeadBob(move.magnitude > 0 && isGrounded && controller.velocity.magnitude > 0.1f);
    }

    void HandleHeadBob(bool isMoving)
    {
        if (isMoving)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;
            playerCamera.transform.localPosition = camStartPos + new Vector3(0, bobOffset, 0);
        }
        else
        {
            // плавно возвращаем камеру на место
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, camStartPos, Time.deltaTime * 5f);
            bobTimer = 0f;
        }
    }
}
