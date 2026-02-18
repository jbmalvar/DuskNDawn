using UnityEngine;
using UnityEngine.SceneManagement; // REQUIRED for loading scenes

public class DeathMenuController : MonoBehaviour
{
    // Function to reload the current level
    public void RetryGame()
    {
        // Get the name of the active scene and reload it
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        // Ensure time is running again (just in case it was paused)
        Time.timeScale = 1f; 
    }

    // Function to go to the Main Menu
    public void QuitToMenu()
    {
        // Make sure your menu scene is named exactly "MainMenu" (case sensitive)
        SceneManager.LoadScene("MainMenu");
        
        Time.timeScale = 1f;
    }
}