using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor; // Needed for auto-selecting the object
#endif

public class PlayerInteract : MonoBehaviour
{
    [Header("Settings")]
    public float range = 5f; // Increased slightly to help find walls easier
    [SerializeField] private LayerMask ignoreLayer;

    [Header("Debug")]
    public bool enableColliderFinder = true; // Uncheck this when you are done fixing!

    [Header("References")]
    public GameObject interactionPrompt;
    public TextMeshProUGUI promptText;
    public PlayerInventory playerInventory;

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

        // Draw the ray so you can see where you are looking
        Debug.DrawRay(ray.origin, ray.direction * range, Color.green);

        if (Physics.Raycast(ray, out RaycastHit hit, range, ~ignoreLayer))
        {
            // =========================================================
            //  THE "FINDER TOOL"
            // =========================================================
            if (enableColliderFinder && hit.collider is BoxCollider)
            {
                // 1. Get the parent name to help locate it in hierarchy
                string parentName = hit.transform.parent != null ? hit.transform.parent.name : "No Parent";
                
                // 2. Log useful info with color for visibility
                Debug.Log($"<color=red>BLOCKED BY:</color> {hit.collider.name} \n<color=yellow>PARENT:</color> {parentName}");

                // 3. Draw a box around the hit object in the Scene view so you can see the invisible wall
                // (Only works if you have Gizmos enabled in Game view or look at Scene view)
                Debug.DrawRay(hit.point, Vector3.up, Color.red, 1f); 

                // 4. EDITOR ONLY: Auto-select the object in the Hierarchy
                #if UNITY_EDITOR
                // This makes Unity highlight the object as if you clicked it
                if (Selection.activeGameObject != hit.collider.gameObject) 
                {
                    Selection.activeGameObject = hit.collider.gameObject;
                    EditorGUIUtility.PingObject(hit.collider.gameObject);
                }
                #endif
            }
            // =========================================================

            // ... Your existing logic continues below ...

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

            if (hit.collider.TryGetComponent(out DoorInteraction doorScript))
            {
                ShowPrompt("Press E to Open Door");
                if (Input.GetKeyDown(KeyCode.E)) doorScript.Interact();
                return;
            }

            // Key Item
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
                }
                return;
            }

            // Door Controller
            if (hit.collider.TryGetComponent(out DoorController doorController))
            {
                ShowPrompt("Press E to Open");
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (playerInventory != null) doorController.AttemptOpen(playerInventory);
                }
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