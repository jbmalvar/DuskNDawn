using UnityEngine;
using System.Collections;
using TMPro; // REQUIRED for TextMeshPro

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float smoothSpeed = 2f;

    [Header("Locked Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip lockedSound; 
    [SerializeField] private AudioClip openSound;   
    [SerializeField] private float rattleStrength = 3f; 

    [Header("UI Feedback")]
    [SerializeField] private TextMeshProUGUI messageText; // Drag your new text here
    [SerializeField] private string lockedMessage = "Locked"; 
    [SerializeField] private float messageDuration = 2f;

    private bool isOpen = false;
    private bool isRattling = false; 
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine messageCoroutine; // We store this so we can restart it if clicked twice

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

        // Ensure text is hidden at start
        if(messageText != null) messageText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isRattling) return;

        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
    }

    public void AttemptOpen(PlayerInventory playerInv)
    {
        if (isRattling) return;

        if (isOpen)
        {
            isOpen = false;
            return;
        }

        if (playerInv != null && playerInv.hasDoorKey)
        {
            isOpen = true;
            if(audioSource && openSound) audioSource.PlayOneShot(openSound);
            
            // Optional: Hide the locked message if it was showing
            if (messageText != null) messageText.gameObject.SetActive(false);
        }
        else
        {
            // FAIL: Rattle and Show Message
            StartCoroutine(RattleRoutine());
            ShowLockedMessage(); // Trigger the UI
        }
    }

    private void ShowLockedMessage()
    {
        if (messageText != null)
        {
            // If a message is already fading out, stop it and start fresh
            if (messageCoroutine != null) StopCoroutine(messageCoroutine);
            messageCoroutine = StartCoroutine(MessageRoutine());
        }
    }

    private IEnumerator MessageRoutine()
    {
        messageText.text = lockedMessage;
        messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(messageDuration);

        messageText.gameObject.SetActive(false);
    }

    private IEnumerator RattleRoutine()
    {
        isRattling = true; 
        if (audioSource && lockedSound) audioSource.PlayOneShot(lockedSound);

        float timer = 0f;
        float duration = 0.5f; 
        Quaternion startRot = transform.rotation;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float zShake = Mathf.Sin(timer * 100f) * rattleStrength * (1f - (timer / duration));
            transform.rotation = startRot * Quaternion.Euler(0, zShake, 0);
            yield return null;
        }

        transform.rotation = startRot;
        isRattling = false; 
    }
}