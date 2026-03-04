using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the MapScreen parent object here.")]
    public GameObject mapUI;

    [Header("Lighting")]
    [Tooltip("Drag the secret map Directional Light here.")]
    public GameObject mapLight;

    [Header("Player Control")]
    [Tooltip("Drag your player or camera object here, then select your mouse-look script.")]
    public MonoBehaviour mouseLookScript;

    private bool isMapOpen = false;
    private bool originalFogState; 

    void Start()
    {
        if (mapUI != null) mapUI.SetActive(false);
        if (mapLight != null) mapLight.SetActive(false);
    }

    void Update()
    {
        // Toggle the map when pressing Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMap();
        }
    }

    private void ToggleMap()
    {
        isMapOpen = !isMapOpen;
        mapUI.SetActive(isMapOpen);

        if (isMapOpen)
        {
            // --- PAUSE EVERYTHING ---
            originalFogState = RenderSettings.fog; 
            RenderSettings.fog = false;            

            if (mapLight != null) mapLight.SetActive(true);
            if (mouseLookScript != null) mouseLookScript.enabled = false;

            Time.timeScale = 0f;
            
            // Pause all audio globally
            AudioListener.pause = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // --- RESUME EVERYTHING ---
            RenderSettings.fog = originalFogState; 

            if (mapLight != null) mapLight.SetActive(false);
            if (mouseLookScript != null) mouseLookScript.enabled = true;

            Time.timeScale = 1f;
            
            // Resume all audio globally
            AudioListener.pause = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}