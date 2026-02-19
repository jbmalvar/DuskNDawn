using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    public int nextLevelIndex = 2; 
    
    [Header("Transition Settings")]
    public bool useTransition = true; 
    public GameObject transitionUI; 
    public float transitionDelay = 3f; 

    private bool isTransitioning = false; 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            isTransitioning = true; 
            
            if (nextLevelIndex > PlayerPrefs.GetInt("LevelReached", 1))
            {
                PlayerPrefs.SetInt("LevelReached", nextLevelIndex);
                AnalyticsManager.Instance.SetCurrentLevel(nextLevelIndex);
                PlayerPrefs.Save(); 
            }
            
            if (useTransition)
            {
                StartCoroutine(ShowTransitionAndLoadAsync());
            }
            else
            {
                SceneManager.LoadScene(nextLevelIndex);
            }
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

        // 5. Ensure the background load is finished (Unity stops progress at 0.9f when ready)
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