using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    public float range = 3f;
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;

    void Start()
    {
        // Hide the text when the game starts
        interactionPrompt.SetActive(false);
    }

    void Update()
    {

        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            // Check for TimeTravel (Existing logic)
            TimeTravel timeScript = hit.collider.GetComponent<TimeTravel>();
            // Check for Torch (New logic)
            TorchPickup torchScript = hit.collider.GetComponent<TorchPickup>();

            if (timeScript != null)
            {
                ShowPrompt("Press E to interact");
                if (Input.GetKeyDown(KeyCode.E)) timeScript.Interact();
                return;
            }
            else if (torchScript != null)
            {
                ShowPrompt("Press E to pick up Torch");
                if (Input.GetKeyDown(KeyCode.E)) torchScript.PickUp();
                return;
            }
        }

        interactionPrompt.SetActive(false);
    }

    // Small helper to keep things clean
    void ShowPrompt(string message)
    {
        interactionPrompt.SetActive(true);
        promptText.text = message;
    }
}