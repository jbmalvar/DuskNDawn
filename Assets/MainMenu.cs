using UnityEngine;
using UnityEngine.SceneManagement; // This is required to switch scenes!

public class MainMenu : MonoBehaviour
{
    // This function will be linked to your Play Button
    public void PlayGame()
    {
        // This loads the scene at Index 1 (your game scene)
        // You can also use SceneManager.LoadScene("YourSceneName");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // This function will be linked to your Quit Button
    public void QuitGame()
    {
        // Application.Quit doesn't do anything in the Unity Editor, 
        // so we log a message to the console to verify it works.
        Debug.Log("Game is quitting!");
        Application.Quit();
    }
}