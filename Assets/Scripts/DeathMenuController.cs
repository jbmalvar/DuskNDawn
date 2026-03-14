using UnityEngine;
using System.Collections; 
using TMPro; 

public class DeathMenuController : MonoBehaviour
{
    [Header("UI & Timing")]
    [SerializeField] private TextMeshProUGUI deathText; 
    [SerializeField] private string deathMessage = "He feels pity for you, try again.";
    [SerializeField] private float restartDelay = 3f; 

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip respawnSound;

    [Header("Respawn System (Drag these in!)")]
    [SerializeField] private Transform respawnPoint;  // Create an Empty GameObject for where they spawn
    [SerializeField] private GameObject player;       // Drag your Player here
    [SerializeField] private StatueMonsterAI monster; // Drag your Monster here

    private void OnEnable()
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // 1. Show the pity text
        if (deathText != null)
        {
            deathText.text = deathMessage;
            deathText.gameObject.SetActive(true);
        }

        // 2. Play the respawn/death audio
        if (audioSource != null && respawnSound != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }

        // 3. Wait for the specified amount of seconds in REAL time 
        yield return new WaitForSecondsRealtime(restartDelay);

        // 4. Register the death with the Pity Manager
        if (PityManager.Instance != null)
        {
            PityManager.Instance.RegisterDeath();
        }

        // --- NEW TELEPORT & RESET LOGIC ---
        
        // 5. Teleport the Player safely
        if (player != null && respawnPoint != null)
        {
            // You MUST disable the CharacterController to teleport, or Unity ignores it
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // Move the player to the checkpoint
            player.transform.position = respawnPoint.position;
            player.transform.rotation = respawnPoint.rotation;

            // Turn the CharacterController back on
            if (cc != null) cc.enabled = true;

            // Re-enable Player Movement so they can walk again
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null) 
            {
                pm.enabled = true;
                
                // Manually apply the stamina pity boost since Start() won't run again
                if (PityManager.Instance != null)
                {
                    pm.regenSpeed *= (1f + PityManager.Instance.staminaBoostPercentage);
                }
            }
        }

        // 6. Reset the Monster
        if (monster != null)
        {
            monster.ResetMonster();
        }

        // 7. Lock and hide the cursor again for first-person gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 7. Hide the Death Canvas so the player can see the game again
        gameObject.SetActive(false);
    }
}