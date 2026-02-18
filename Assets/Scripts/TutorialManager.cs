using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI tutorialText;
    public CanvasGroup textCanvasGroup; // Add a CanvasGroup component to your Text object!

    [Header("Settings")]
    public float fadeDuration = 1.0f;
    public float timeBetweenMessages = 0.5f;

    [Header("Messages")]
    [TextArea] public string moveMessage = "Use WASD to move";
    [TextArea] public string interactMessage = "Press E to interact with objects";
    [TextArea] public string goalMessage = "Find the Obelisk.\nTraverse between two worlds to solve the puzzle.";

    // Private tracking
    private Vector3 startPos;
    private Transform playerTransform;

    void Start()
    {
        // Find player automatically if tagged, otherwise assume Camera root
        if(GameObject.FindGameObjectWithTag("Player"))
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        else
            playerTransform = Camera.main.transform.root;

        startPos = playerTransform.position;
        
        // Ensure alpha is 0 to start, then begin the sequence
        textCanvasGroup.alpha = 0;
        StartCoroutine(TutorialSequence());
    }

    IEnumerator TutorialSequence()
    {
        // --- STEP 1: MOVEMENT ---
        tutorialText.text = moveMessage;
        yield return StartCoroutine(FadeText(1)); // Fade In

        // Wait until player moves 2 units away from start
        while (Vector3.Distance(playerTransform.position, startPos) < 2.0f)
        {
            yield return null;
        }

        yield return StartCoroutine(FadeText(0)); // Fade Out
        yield return new WaitForSeconds(timeBetweenMessages);


        // --- STEP 2: INTERACTION ---
        tutorialText.text = interactMessage;
        yield return StartCoroutine(FadeText(1));

        // Wait until player presses E
        while (!Input.GetKeyDown(KeyCode.E))
        {
            yield return null;
        }

        yield return StartCoroutine(FadeText(0));
        yield return new WaitForSeconds(timeBetweenMessages);


        // --- STEP 3: THE GOAL ---
        tutorialText.text = goalMessage;
        yield return StartCoroutine(FadeText(1));

        yield return new WaitForSeconds(6.0f); // Read time

        yield return StartCoroutine(FadeText(0));
        
        // Tutorial Complete - Disable text object to save resources
        tutorialText.gameObject.SetActive(false);
    }

    // Helper to fade text seamlessly
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