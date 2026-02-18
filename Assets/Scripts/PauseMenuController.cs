using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject pauseCanvas; // Drag your PauseCanvas here

    // Internal state
    private bool isPaused = false;

    void Start()
    {
        // Ensure the menu is hidden at start
        if (pauseCanvas != null) pauseCanvas.SetActive(false);
    }

    void Update()
    {
        // Check if the script is actually running
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // --- BUTTON FUNCTIONS ---

    public void ResumeGame()
    {
        isPaused = false;
        
        // 1. Hide the Menu
        if (pauseCanvas != null) pauseCanvas.SetActive(false);

        // 2. Unfreeze Time
        Time.timeScale = 1f;

        // 3. Lock Cursor back to game (optional, remove if you don't use locked cursor)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void PauseGame()
    {
        isPaused = true;

        // 1. Show the Menu
        if (pauseCanvas != null) pauseCanvas.SetActive(true);

        // 2. Freeze Time (This stops physics and updates)
        Time.timeScale = 0f;

        // 3. Unlock Cursor so you can click buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitToMenu()
    {
        // Unfreeze time before leaving (otherwise the next scene might be stuck!)
        Time.timeScale = 1f;
        
        // Make sure your menu scene name matches exactly
        SceneManager.LoadScene("MainMenu");
    }
}