using UnityEngine;
using System.Collections;

public class PeekEvent : MonoBehaviour
{
    [Header("Settings")]
    public Animator monsterAnimator; // Drag your Monster here
    public bool isStartTrigger = true; // CHECK for Trigger A, UNCHECK for Trigger B
    public float hideDelay = 2.0f; // How long to wait before disabling the monster (Trigger B only)

    private void Start()
    {
        // Safety: If this is a Start Trigger, make sure the monster 
        // is actually hidden when the game starts so he doesn't play early.
        if (isStartTrigger && monsterAnimator != null)
        {
            monsterAnimator.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (monsterAnimator == null) return;

            if (isStartTrigger)
            {
                // 1. Enable the monster
                monsterAnimator.gameObject.SetActive(true);
                // 2. Wait a frame then trigger the animation
                StartCoroutine(TriggerShowNextFrame());
            }
            else
            {
                // Trigger B: End the scare
                monsterAnimator.SetTrigger("Hide");
                StartCoroutine(DisableMonster());
            }

            // Disable this trigger so it only happens once
            GetComponent<Collider>().enabled = false;
        }
    }

    IEnumerator TriggerShowNextFrame()
    {
        yield return null; 
        
        if (monsterAnimator != null)
        {
            // Reset the Hide trigger just in case it was fired early
            monsterAnimator.ResetTrigger("Hide"); 
            
            monsterAnimator.SetTrigger("Show");
            Debug.Log("Trigger A: Monster shown. Waiting in Idle...");
        }
    }

    IEnumerator DisableMonster()
    {
        yield return new WaitForSeconds(hideDelay);
        
        if (monsterAnimator != null)
        {
            monsterAnimator.gameObject.SetActive(false);
            Debug.Log("Trigger B: Monster hidden.");
        }
    }
}