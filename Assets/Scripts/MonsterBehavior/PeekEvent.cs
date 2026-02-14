using UnityEngine;
using System.Collections;

public class PeekEvent : MonoBehaviour
{
    [Header("Settings")]
    public Animator monsterAnimator; 
    public bool isStartTrigger = true; 
    public float hideDelay = 2.0f; 

    [Header("Audio")] // <--- NEW SECTION
    public AudioSource audioSource; 
    public AudioClip triggerSound;

    private void Start()
    {
        if (isStartTrigger && monsterAnimator != null)
        {
            monsterAnimator.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // --- 1. PLAY SOUND HERE ---
            if (audioSource != null && triggerSound != null)
            {
                audioSource.PlayOneShot(triggerSound);
            }

            // --- 2. MONSTER LOGIC ---
            if (monsterAnimator != null)
            {
                if (isStartTrigger)
                {
                    monsterAnimator.gameObject.SetActive(true);
                    StartCoroutine(TriggerShowNextFrame());
                }
                else
                {
                    monsterAnimator.SetTrigger("Hide");
                    StartCoroutine(DisableMonster());
                }
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
            monsterAnimator.ResetTrigger("Hide"); 
            monsterAnimator.SetTrigger("Show");
        }
    }

    IEnumerator DisableMonster()
    {
        // Wait for state transition
        yield return new WaitForSeconds(0.2f);

        // Get length of "Hide" animation
        AnimatorStateInfo stateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
        float remainingTime = stateInfo.length - 0.2f;
        if (remainingTime < 0) remainingTime = 0;

        // Wait for animation to finish
        yield return new WaitForSeconds(remainingTime);
        
        if (monsterAnimator != null)
        {
            monsterAnimator.gameObject.SetActive(false);
        }
    }
}