using UnityEngine;
using System.Collections;

public class SimpleJumpScare : MonoBehaviour
{
    // These create slots in the Inspector for you to drag things into
    public GameObject monsterModel;
    public AudioSource screamSound;
    public float scareDuration = 2.0f; // Stays on screen for 2 seconds

    // This makes sure the scare doesn't happen twice
    private bool hasTriggered = false;

    // This function runs automatically when something enters the "Is Trigger" box
    private void OnTriggerEnter(Collider other)
    {
        // Check if the thing entering is the Player (using the "Player" tag)
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(ScareSequence());
        }
    }

    // This handles the timing
    IEnumerator ScareSequence()
    {
        // 1. Unhide the monster
        monsterModel.SetActive(true);

        // 2. Play the sound (if we have one)
        if (screamSound != null) screamSound.Play();

        // 3. Wait for 2 seconds
        yield return new WaitForSeconds(scareDuration);

        // 4. Hide the monster again
        monsterModel.SetActive(false);
    }
}