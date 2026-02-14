using UnityEngine;
using System.Collections;

public class PeekEvent : MonoBehaviour
{
    [Header("Settings")]
    public Animator monsterAnimator;
    public bool isStartTrigger = true; // Check this TRUE for the trigger that starts the peek
    public float hideDelay = 1.5f;     // How long to wait before disabling the monster (when hiding)

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip triggerSound;

    private void Start()
    {
        // If this is the trigger that STARTS the scare, hide the monster initially
        if (isStartTrigger && monsterAnimator != null)
        {
            monsterAnimator.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // --- 1. PLAY SOUND ---
            if (audioSource != null && triggerSound != null)
            {
                audioSource.PlayOneShot(triggerSound);
            }

            // --- 2. MONSTER LOGIC ---
            if (monsterAnimator != null)
            {
                if (isStartTrigger)
                {
                    // START THE PEEK
                    monsterAnimator.gameObject.SetActive(true);
                    
                    // This matches your Animator Condition: isPeeking = true
                    monsterAnimator.SetBool("isPeeking", true);
                }
                else
                {
                    // END THE PEEK
                    // This matches your return logic: isPeeking = false
                    monsterAnimator.SetBool("isPeeking", false);
                    
                    // Start timer to make the GameObject vanish
                    StartCoroutine(DisableMonster());
                }
            }

            // Disable this trigger so it only happens once
            GetComponent<Collider>().enabled = false;
        }
    }

    IEnumerator DisableMonster()
    {
        // 1. Wait a tiny bit for the Animator to switch from "Idle" to "PeekFinish"
        yield return new WaitForSeconds(0.2f);

        // 2. Get the length of the animation currently playing (PeekFinish)
        if (monsterAnimator != null)
        {
            AnimatorStateInfo stateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
            
            // 3. Wait for the rest of the animation to finish
            // (We subtract 0.2f because we already waited that long above)
            float waitTime = stateInfo.length - 0.2f;
            
            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
            }
        }

        // 4. NOW disable it immediately
        if (monsterAnimator != null)
        {
            monsterAnimator.gameObject.SetActive(false);
        }
    }
}