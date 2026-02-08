using UnityEngine;

public class DoorController : MonoBehaviour
{
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    public float openAngle = 90f;
    public float smoothSpeed = 2f;

    void Start()
    {
        // Store the starting rotation
        closedRotation = transform.rotation;
        // Calculate the open rotation
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
    }

    void Update()
    {
        // Smoothly rotate the door based on state
        if (isOpen)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, openRotation, Time.deltaTime * smoothSpeed);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, closedRotation, Time.deltaTime * smoothSpeed);
        }
    }

    public void AttemptOpen(PlayerInventory playerInv)
    {
        if (isOpen)
        {
            // Optional: Close it if already open
            Debug.Log("Door: Closing.");
            isOpen = false;
            return;
        }

        if (playerInv.hasDoorKey)
        {
            Debug.Log("Door: Access Granted.");
            isOpen = true;
        }
        else
        {
            Debug.Log("Door: Locked. You need the key.");
        }
    }
}