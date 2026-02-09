using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    public float range = 3f;
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;
    
    // Set this to "Player" or "Ignore Raycast" in the Inspector
    [SerializeField] private LayerMask ignoreLayer; 

    private Camera cam;

    void Start()
    {
        cam = Camera.main; 
        interactionPrompt.SetActive(false);
        
        // Safety check to make sure you have a camera tagged "MainCamera"
        if (cam == null) Debug.LogError("No Main Camera found in scene!");
    }

    void Update()
    {
        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        // This shoots the ray from the EXACT center of your screen
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        // Debug: See the ray in the Scene view while playing
        Debug.DrawRay(ray.origin, ray.direction * range, Color.green);

        // ~ignoreLayer ensures the ray doesn't hit the player/torch in your hand
        if (Physics.Raycast(ray, out RaycastHit hit, range, ~ignoreLayer))
        {
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
        }

        interactionPrompt.SetActive(false);
    }

    void ShowPrompt(string message)
    {
        interactionPrompt.SetActive(true);
        promptText.text = message;
    }
}