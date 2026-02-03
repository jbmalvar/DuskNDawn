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

        // 1. SHOOT THE LASER
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            TimeTravel timeScript = hit.collider.GetComponent<TimeTravel>();

            if (timeScript != null)
            {

                interactionPrompt.SetActive(true);

                // Update what the text says
                if (hit.collider.CompareTag("TutorialObelisk"))
                {
                    promptText.text = "Press E to interact";
                }
                else
                {
                    promptText.text = "E";
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    timeScript.Interact();
                }

                return; 
            }
        }

        interactionPrompt.SetActive(false);
    }
}