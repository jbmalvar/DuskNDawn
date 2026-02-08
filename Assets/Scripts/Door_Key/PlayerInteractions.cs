using UnityEngine;
using TMPro; // Standard Unity Text Mesh Pro

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactRange = 3f;
    public LayerMask interactLayer; // Set this to "Default" or your specific layer

    [Header("References")]
    public TextMeshProUGUI promptText; // Drag your UI Text here
    public PlayerInventory playerInventory; // Drag your Player object here

    void Update()
    {
        // 1. Create the Ray (shoot laser from center of screen)
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // 2. Reset UI text hidden by default every frame
        bool hitSomething = false;
        
        // 3. Perform Raycast
        if (Physics.Raycast(ray, out hit, interactRange, interactLayer))
        {
            // CASE A: We hit a Door
            DoorController door = hit.collider.GetComponent<DoorController>();
            if (door != null)
            {
                hitSomething = true;
                promptText.text = "Press F to Open";
                
                if (Input.GetKeyDown(KeyCode.F))
                {
                    door.AttemptOpen(playerInventory);
                }
            }

            // CASE B: We hit a Key
            KeyItem key = hit.collider.GetComponent<KeyItem>();
            if (key != null)
            {
                hitSomething = true;
                promptText.text = "Press F to Pickup";

                if (Input.GetKeyDown(KeyCode.F))
                {
                    playerInventory.CollectKey();
                    key.Pickup();
                }
            }
        }

        // 4. Show/Hide the UI text based on if we hit something valid
        if (hitSomething)
        {
            promptText.gameObject.SetActive(true);
        }
        else
        {
            promptText.gameObject.SetActive(false);
        }
    }
}