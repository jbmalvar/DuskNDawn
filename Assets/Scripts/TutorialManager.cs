using UnityEngine;
using TMPro;
using System.Collections;
using System; // Required for Func<bool>

public class TutorialManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI tutorialText;
    public CanvasGroup textCanvasGroup; 

    [Header("Settings")]
    public float fadeDuration = 1.0f;
    public float timeBetweenMessages = 0.5f;

    [Header("Messages")]
    [TextArea] public string moveMessage = "Use WASD to move";
    [TextArea] public string sprintMessage = "Hold Shift to sprint";
    [TextArea] public string interactMessage = "Press E to interact with objects";
    [TextArea] public string mapMessage = "Press Tab to look at the map.\nYellow = Key, Grey = Obelisk, Green = You";
    [TextArea] public string pauseMessage = "Press Escape to pause";
    [TextArea] public string goalMessage = "Find the Obelisk.\nTraverse between two worlds to solve the puzzle.";

    private Vector3 startPos;
    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerTransform = player != null ? player.transform : Camera.main.transform.root;
        startPos = playerTransform.position;
        
        textCanvasGroup.alpha = 0;
        StartCoroutine(TutorialSequence());
    }

    IEnumerator TutorialSequence()
    {
        // 1. MOVEMENT
        yield return StartCoroutine(ShowMessageAndWait(moveMessage, () => Vector3.Distance(playerTransform.position, startPos) >= 2.0f));

        // 2. SPRINT
        yield return StartCoroutine(ShowMessageAndWait(sprintMessage, () => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)));

        // 3. INTERACTION
        yield return StartCoroutine(ShowMessageAndWait(interactMessage, () => Input.GetKeyDown(KeyCode.E)));

        // 4. MAP
        yield return StartCoroutine(ShowMessageAndWait(mapMessage, () => Input.GetKeyDown(KeyCode.Tab)));

        // 5. PAUSE (Time-based, displays for 4 seconds)
        yield return StartCoroutine(ShowMessageAndWait(pauseMessage, null, 4.0f));

        // 6. THE GOAL (Time-based, no input condition)
        yield return StartCoroutine(ShowMessageAndWait(goalMessage, null, 6.0f));
        
        // Tutorial Complete
        tutorialText.gameObject.SetActive(false);
    }

    // --- HELPER METHODS ---

    IEnumerator ShowMessageAndWait(string message, Func<bool> exitCondition, float delayIfNoCondition = 0f)
    {
        tutorialText.text = message;
        yield return StartCoroutine(FadeText(1)); // Fade In

        // Wait for either the specific input/action, or a timer if no condition is provided
        if (exitCondition != null)
        {
            yield return new WaitUntil(exitCondition);
        }
        else
        {
            yield return new WaitForSeconds(delayIfNoCondition);
        }

        yield return StartCoroutine(FadeText(0)); // Fade Out
        yield return new WaitForSeconds(timeBetweenMessages);
    }

    IEnumerator FadeText(float targetAlpha)
    {
        float startAlpha = textCanvasGroup.alpha;
        float timer = 0;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }
        textCanvasGroup.alpha = targetAlpha;
    }
}