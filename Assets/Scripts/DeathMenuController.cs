using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Collections; 
using TMPro; // 1. WE ADDED THIS LINE to tell Unity we are using TextMeshPro

public class DeathMenuController : MonoBehaviour
{
    [Header("UI & Timing")]
    // 2. WE CHANGED "Text" to "TextMeshProUGUI" HERE
    [SerializeField] private TextMeshProUGUI deathText; 
    
    [SerializeField] private string deathMessage = "He feels pity for you, try again.";
    [SerializeField] private float restartDelay = 3f; 

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip respawnSound;

    private void OnEnable()
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // Show the pity text
        if (deathText != null)
        {
            deathText.text = deathMessage;
            deathText.gameObject.SetActive(true);
        }

        // Play the respawn/death audio
        if (audioSource != null && respawnSound != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }

        // Wait for the specified amount of seconds in REAL time 
        yield return new WaitForSecondsRealtime(restartDelay);

        // Register the death to adjust the pity stats before reloading
        if (PityManager.Instance != null)
        {
            PityManager.Instance.RegisterDeath();
        }

        // Reload the current level and unpause time
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}