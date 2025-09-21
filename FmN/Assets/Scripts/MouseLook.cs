using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 150f;   // чувствительность мыши
    public float smoothTime = 0.05f;        // сглаживание мыши
    public Transform playerBody;            // ссылка на объект Player

    [Header("Head Bobbing")]
    public bool enableHeadBob = true;
    public float bobFrequency = 2f;               // скорость "шагов"
    public float bobHorizontalAmplitude = 0.04f;  // смещение в стороны
    public float bobVerticalAmplitude = 0.025f;   // высота дуги
    public float bobMoveSmoothTime = 0.1f;        // сглаживание камеры
    public float speedForFullBob = 5f;            // скорость для полной амплитуды

    public CharacterController controller;        // ссылка на CharacterController

    private float xRotation = 0f;
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    private Vector3 initialCameraLocalPos;
    private float bobTimer = 0f;
    private float bobWeight = 0f;
    private float bobWeightVel = 0f;
    private Vector3 bobVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        initialCameraLocalPos = transform.localPosition;
    }

    void Update()
    {
        HandleMouseLook();
        if (enableHeadBob) HandleHeadBob();
    }

    void HandleMouseLook()
    {
        // получаем движение мыши
        Vector2 targetMouseDelta = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );

        // сглаживаем движение мыши
        currentMouseDelta = Vector2.SmoothDamp(
            currentMouseDelta,
            targetMouseDelta,
            ref currentMouseDeltaVelocity,
            smoothTime
        );

        // вращение камеры по X (вверх/вниз)
        xRotation -= currentMouseDelta.y * mouseSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // вращение тела игрока по Y (влево/вправо)
        playerBody.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity * Time.deltaTime);
    }

    void HandleHeadBob()
    {
        // измеряем скорость игрока по горизонтали
        float inputMag = 0f;
        if (controller != null)
        {
            Vector3 vel = controller.velocity;
            vel.y = 0f; // убираем вертикальную скорость
            inputMag = vel.magnitude;
        }

        // переводим скорость в коэффициент от 0 до 1
        float targetWeight = Mathf.Clamp01(inputMag / Mathf.Max(0.01f, speedForFullBob));
        bobWeight = Mathf.SmoothDamp(bobWeight, targetWeight, ref bobWeightVel, 0.1f);

        if (bobWeight > 0.01f) // если персонаж реально движется
        {
            // шаг таймера
            bobTimer += Time.deltaTime * bobFrequency * bobWeight;
            if (bobTimer > Mathf.PI * 2f) bobTimer -= Mathf.PI * 2f;

            float phase = bobTimer;

            // боковое смещение (влево-вправо)
            float hor = Mathf.Sin(phase) * bobHorizontalAmplitude * bobWeight;

            // вертикальное смещение (по дуге вверх)
            float vert = Mathf.Abs(Mathf.Cos(phase)) * bobVerticalAmplitude * bobWeight;

            // итоговая позиция камеры
            Vector3 targetLocal = initialCameraLocalPos + new Vector3(hor, vert, 0);

            transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                targetLocal,
                ref bobVelocity,
                bobMoveSmoothTime
            );
        }
        else
        {
            // плавный возврат камеры в исходное положение
            transform.localPosition = Vector3.SmoothDamp(
                transform.localPosition,
                initialCameraLocalPos,
                ref bobVelocity,
                bobMoveSmoothTime
            );
        }
    }
}
