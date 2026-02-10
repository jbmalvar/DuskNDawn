using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    [Header("Settings")]
    public float range = 3f;
    // Set this to "Player" or "Ignore Raycast" in the Inspector to avoid detecting yourself
    [SerializeField] private LayerMask ignoreLayer;

    [Header("References")]
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;
    public PlayerInventory playerInventory; // Drag your Player object/inventory here

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        interactionPrompt.SetActive(false);

        if (cam == null) Debug.LogError("No Main Camera found in scene!");
    }

    void Update()
    {
        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // Debug draw to see the ray in Scene view
        Debug.DrawRay(ray.origin, ray.direction * range, Color.green);

        // ~ignoreLayer inverts the mask (collides with everything EXCEPT the ignoreLayer)
        if (Physics.Raycast(ray, out RaycastHit hit, range, ~ignoreLayer))
        {
            // --- EXISTING INTERACTIONS ---

            if (hit.collider.TryGetComponent(out TimeTravel timeScript))
            {
                ShowPrompt("Press E to interact");
                if (Input.GetKeyDown(KeyCode.E)) timeScript.Interact();
                return;
            }

            if (hit.collider.TryGetComponent(out TorchPickup torchScript))
            {
                ShowPrompt("Press E to pick up Torch");
                if (Input.GetKeyDown(KeyCode.E)) torchScript.PickUp();
                return;
            }

            // --- OLD DOOR SCRIPT ---
            if (hit.collider.TryGetComponent(out DoorInteraction doorScript))
            {
                ShowPrompt("Press E to Open Door");
                if (Input.GetKeyDown(KeyCode.E)) doorScript.Interact();
                return;
            }

            // --- MERGED INTERACTIONS (FROM PlayerInteractions.cs) ---

            // 1. Check for the Key Item
            if (hit.collider.TryGetComponent(out KeyItem key))
            {
                ShowPrompt("Press E to Pickup Key");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (playerInventory != null)
                    {
                        playerInventory.CollectKey();
                        key.Pickup();
                    }
                    else
                    {
                        Debug.LogError("PlayerInventory not assigned in Inspector on PlayerInteract!");
                    }
                }
                return;
            }

            // 2. Check for the Door Controller (The one requiring a key)
            if (hit.collider.TryGetComponent(out DoorController doorController))
            {
                ShowPrompt("Press E to Open");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (playerInventory != null)
                    {
                        doorController.AttemptOpen(playerInventory);
                    }
                    else
                    {
                        Debug.LogError("PlayerInventory not assigned in Inspector on PlayerInteract!");
                    }
                }
                return;
            }
        }

        // If nothing was hit, or the thing hit wasn't interactable
        interactionPrompt.SetActive(false);
    }

    void ShowPrompt(string message)
    {
        interactionPrompt.SetActive(true);
        promptText.text = message;
    }
}