using UnityEngine;
using System.Collections;

public class PeekEvent : MonoBehaviour
{
    [Header("Settings")]
    public Animator monsterAnimator; // Drag your Monster here
    public bool isStartTrigger = true; // CHECK for Trigger A, UNCHECK for Trigger B
    public float hideDelay = 2.0f; // How long to wait before disabling the monster (Trigger B only)

    private void OnTriggerEnter(Collider other)
    {
        // Only the player can trigger this
        if (other.CompareTag("Player"))
        {
            if (isStartTrigger)
            {
                // Trigger A: Start the scare
                monsterAnimator.gameObject.SetActive(true); // Make sure he exists
                monsterAnimator.SetTrigger("Show");
            }
            else
            {
                // Trigger B: End the scare
                monsterAnimator.SetTrigger("Hide");
                StartCoroutine(DisableMonster());
            }

            // Turn off this trigger so it doesn't happen again
            GetComponent<Collider>().enabled = false;
        }
    }

    IEnumerator DisableMonster()
    {
        // Wait for the 'Peek_Exit' animation to finish
        yield return new WaitForSeconds(hideDelay);
        
        // Make the monster vanish
        monsterAnimator.gameObject.SetActive(false);
    }
}