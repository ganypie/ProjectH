using UnityEngine;

public class SimpleDoor : MonoBehaviour
{
    public Transform player;
    public float openAngle = 90f;       // угол открытия
    public float openSpeed = 180f;      // скорость открытия
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    private Quaternion closedRot;
    private Quaternion targetRot;
    private bool isOpen = false;

    void Start()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        closedRot = transform.rotation;
        targetRot = closedRot;
    }

    void Update()
    {
        if (player == null) return;

        if (Input.GetKeyDown(interactKey))
        {
            float dist = Vector3.Distance(player.position, transform.position);
            if (dist <= interactDistance)
            {
                ToggleDoor();
            }
        }

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            openSpeed * Time.deltaTime
        );
    }

    void ToggleDoor()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            if (player.position.x < transform.position.x)
                targetRot = closedRot * Quaternion.Euler(0f, 0f, -openAngle);
            else
                targetRot = closedRot * Quaternion.Euler(0f, 0f, openAngle);
        }
        else
        {
            targetRot = closedRot;
        }
    }
}
