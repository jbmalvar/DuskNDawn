using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; // Required for accessing Volumes

public class TimeTravel : MonoBehaviour
{
    [Header("World Objects")]
    [SerializeField] GameObject present;
    [SerializeField] GameObject past;

    [Header("Atmosphere (Post-Processing)")]
    [SerializeField] Volume presentVolume;
    [SerializeField] Volume pastVolume;
    [SerializeField] float transitionDuration = 1.0f; // How long the shift takes

    [Header("Audio")]
    [SerializeField] AudioSource ambienceSource; 
    [SerializeField] AudioClip presentAmbience;
    [SerializeField] AudioClip pastAmbience;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip warpSound; // This was named 'timeShiftSFX' in previous errors

    private bool isPresentActive = true;
    private Coroutine transitionCoroutine;

    void Start()
    {
        // Initialize State
        isPresentActive = true;
        UpdateWorldState();
        
        // Ensure Volumes are set correctly at start
        if(presentVolume) presentVolume.weight = 1;
        if(pastVolume) pastVolume.weight = 0;

        // Start initial ambience
        if(ambienceSource && presentAmbience)
        {
            ambienceSource.clip = presentAmbience;
            ambienceSource.Play();
        }
    }

    public void Interact()
    {
        // 1. Play sound (Fixed variable name to 'warpSound')
        if (sfxSource && warpSound) sfxSource.PlayOneShot(warpSound);

        // 2. Flip the boolean variable IMMEDIATELY so we know where we are going
        isPresentActive = !isPresentActive;

        // 3. Start the transition
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionRoutine());
    }

    // This is the function that actually swaps the objects
    private void UpdateWorldState()
    {
        // We swap the physical objects immediately so the player doesn't fall through floors
        present.SetActive(isPresentActive);
        past.SetActive(!isPresentActive);
    }

    private IEnumerator TransitionRoutine()
    {
        float timer = 0f;
        float startVolPresent = presentVolume.weight;
        float startVolPast = pastVolume.weight;
        float startMusicVolume = ambienceSource.volume;

        // Target weights
        float targetVolPresent = isPresentActive ? 1f : 0f;
        float targetVolPast = isPresentActive ? 0f : 1f;

        // Select the correct audio clip for the destination
        AudioClip targetClip = isPresentActive ? presentAmbience : pastAmbience;

        // --- PHASE 1: FADE OUT (The "Blink") ---
        while (timer < transitionDuration / 2)
        {
            timer += Time.deltaTime;
            float t = timer / (transitionDuration / 2); // Normalized 0-1 for this half

            // Blend Volumes
            presentVolume.weight = Mathf.Lerp(startVolPresent, targetVolPresent, t);
            pastVolume.weight = Mathf.Lerp(startVolPast, targetVolPast, t);

            // Fade Audio Out
            ambienceSource.volume = Mathf.Lerp(startMusicVolume, 0f, t);

            yield return null;
        }

        // --- THE SWAP (Mid-point) ---
        // Fixed: Now calling the correct function name 'UpdateWorldState'
        UpdateWorldState(); 
        
        // Swap the audio clip while volume is zero
        ambienceSource.clip = targetClip;
        ambienceSource.Play();

        // --- PHASE 2: FADE IN (The "Reveal") ---
        timer = 0f;
        while (timer < transitionDuration / 2)
        {
            timer += Time.deltaTime;
            float t = timer / (transitionDuration / 2);

            // Keep blending volumes to their final strict values
            presentVolume.weight = Mathf.Lerp(presentVolume.weight, targetVolPresent, t);
            pastVolume.weight = Mathf.Lerp(pastVolume.weight, targetVolPast, t);

            // Fade Audio In
            ambienceSource.volume = Mathf.Lerp(0f, startMusicVolume, t);

            yield return null;
        }

        // Hard set final values to ensure no float errors
        presentVolume.weight = targetVolPresent;
        pastVolume.weight = targetVolPast;
        ambienceSource.volume = startMusicVolume;
    }
}