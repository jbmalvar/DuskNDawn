using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    public int nextLevelIndex = 2; 
    
    [Header("UI Settings")]
    [Tooltip("Drag your 'Press E to advance' UI text or GameObject here.")]
    public GameObject interactPrompt;

    [Header("Transition Settings")]
    public bool useTransition = true; 
    public GameObject transitionUI; 
    public float transitionDelay = 3f; 

    private bool isTransitioning = false; 
    private bool isPlayerInRange = false; 

    void Start()
    {
        // Ensure the prompt is hidden when the level starts
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    void Update()
    {
        // Check if the player is inside the trigger, presses 'E', and we aren't already transitioning
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !isTransitioning)
        {
            StartTransition();
        }
    }

  void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered the trigger: " + other.gameObject.name); // ADD THIS LINE

        // Mark the player as in range and show the UI prompt
        if (other.CompareTag("Player"))
        {
            Debug.Log("It was the player!"); // ADD THIS LINE
            isPlayerInRange = true; 
            
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Mark the player as out of range and hide the UI prompt
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }

    private void StartTransition()
    {
        isTransitioning = true; 
        
        // Hide the prompt immediately once the player triggers the transition
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }

        // Save level progress
        if (nextLevelIndex > PlayerPrefs.GetInt("LevelReached", 1))
        {
            PlayerPrefs.SetInt("LevelReached", nextLevelIndex);
            PlayerPrefs.Save(); 
        }
        
        // Start transition or load immediately
        if (useTransition)
        {
            StartCoroutine(ShowTransitionAndLoadAsync());
        }
        else
        {
            SceneManager.LoadScene(nextLevelIndex);
        }
    }

    IEnumerator ShowTransitionAndLoadAsync()
    {
        // 1. Pause the game immediately
        Time.timeScale = 0f;

        // 2. Show the transition screen
        if (transitionUI != null)
        {
            transitionUI.SetActive(true);
        }

        // 3. Start loading the next level silently in the background
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextLevelIndex);
        
        // Tell Unity NOT to switch to the new scene yet
        asyncLoad.allowSceneActivation = false;

        // 4. Wait for your transition delay (using real-world time)
        yield return new WaitForSecondsRealtime(transitionDelay);

        // 5. Ensure the background load is finished
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // 6. Unpause the game right before the jump
        Time.timeScale = 1f;

        // 7. Flip the switch to activate the loaded scene instantly
        asyncLoad.allowSceneActivation = true;
    }
}