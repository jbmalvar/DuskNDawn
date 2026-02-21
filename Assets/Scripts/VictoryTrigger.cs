using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryTrigger : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject victoryCanvas; // Drag your VictoryCanvas here

    // 1. When the player touches the item
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AnalyticsManager.Instance.SetWinner();
            ShowVictoryScreen();
        }
    }

    void ShowVictoryScreen()
    {
        // Turn on the Green Screen
        if (victoryCanvas != null) 
        {
            victoryCanvas.SetActive(true);
        }

        // Unlock the mouse cursor so you can click the button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Stop the game time (optional, keeps monsters from eating you while you celebrate)
        Time.timeScale = 0f;
    }

    // 2. Button Function (Connect this to your Main Menu Button)
    public void ReturnToMenu()
    {
        Time.timeScale = 1f; // Unfreeze time before leaving
        SceneManager.LoadScene("MainMenu"); // Make sure your menu scene is named exactly this
    }
}