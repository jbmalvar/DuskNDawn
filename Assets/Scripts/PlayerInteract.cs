using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    public float range = 3f;
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;
    
    // Set this to "Player" or "Ignore Raycast" in the Inspector to avoid detecting yourself
    [SerializeField] private LayerMask ignoreLayer; 

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

            // --- NEW DOOR INTERACTION ---
            if (hit.collider.TryGetComponent(out DoorInteraction doorScript))
            {
                ShowPrompt("Press E to Open Door");
                if (Input.GetKeyDown(KeyCode.E)) doorScript.Interact();
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