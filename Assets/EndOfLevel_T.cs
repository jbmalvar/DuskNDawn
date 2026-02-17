using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    public int nextLevelIndex = 2; // Set this in the inspector to the level you want to load next

    // This fires when a physics object enters the trigger zone
    void OnTriggerEnter(Collider other)
    {
        // Ensure the object hitting the trigger is actually the player
        if (other.CompareTag("Player"))
        {
            // If the level we are unlocking is higher than what is currently saved, update the save file
            if (nextLevelIndex > PlayerPrefs.GetInt("LevelReached", 1))
            {
                PlayerPrefs.SetInt("LevelReached", nextLevelIndex);
                PlayerPrefs.Save(); // Force write to disk
            }
            
            // Teleport to the next level
            SceneManager.LoadScene(nextLevelIndex);
        }
    }
}
