using UnityEngine;

public class JumpScareBehavior : MonoBehaviour
{
    public GameObject monster; // Drag your monster here in the Inspector
    public AudioSource scareSound; // Optional: Drag a sound effect here

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the thing walking into the cube is the Player
        if (other.CompareTag("Player") && !hasTriggered)
        {
            ActivateScare();
        }
    }

    void ActivateScare()
    {
        hasTriggered = true;
        monster.SetActive(true); // Make the monster appear
        
        if (scareSound != null) 
            scareSound.Play();

        // Optional: Hide the monster again after 2 seconds
        Invoke("EndScare", 2f);
    }

    void EndScare()
    {
        monster.SetActive(false);
    }
}
