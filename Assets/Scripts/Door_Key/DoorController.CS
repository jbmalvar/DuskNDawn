using UnityEngine;
using System.Collections;
using TMPro;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float smoothSpeed = 2f;
    
    [Header("Security Settings")]
    [Tooltip("This must match the KeyID on the KeyItem exactly.")]
    [SerializeField] private string requiredKeyID = "GeneralKey"; 

    [Header("Locked Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip lockedSound; 
    [SerializeField] private AudioClip openSound;   
    [SerializeField] private float rattleStrength = 3f; 

    [Header("UI Feedback")]
    [SerializeField] private TextMeshProUGUI messageText; 
    [SerializeField] private string lockedMessage = "Locked"; 
    [SerializeField] private string wrongKeyMessage = "Wrong Key. Find a different key"; // New message
    [SerializeField] private float messageDuration = 2f;

    private bool isOpen = false;
    private bool isRattling = false; 
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine messageCoroutine; 

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
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

        // If already open, just close it
        if (isOpen)
        {
            isOpen = false;
            return;
        }

        // Check Inventory
        if (playerInv != null)
        {
            // 1. Player has the CORRECT key
            if (playerInv.HasKey(requiredKeyID))
            {
                OpenDoor();
            }
            // 2. Player has keys, but NOT the right one
            else if (playerInv.HasAnyKeys()) 
            {
                // Wrong Key Feedback
                StartCoroutine(RattleRoutine());
                ShowMessage(wrongKeyMessage);
            }
            // 3. Player has NO keys
            else 
            {
                // Standard Locked Feedback
                StartCoroutine(RattleRoutine());
                ShowMessage(lockedMessage);
            }
        }
    }

    private void OpenDoor()
    {
        isOpen = true;
        if(audioSource && openSound) audioSource.PlayOneShot(openSound);
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    // Consolidated the message logic to accept different strings
    private void ShowMessage(string msg)
    {
        if (messageText != null)
        {
            if (messageCoroutine != null) StopCoroutine(messageCoroutine);
            messageCoroutine = StartCoroutine(MessageRoutine(msg));
        }
    }

    private IEnumerator MessageRoutine(string msg)
    {
        messageText.text = msg;
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