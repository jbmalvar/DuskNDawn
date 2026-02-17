using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; 

public class TimeTravel : MonoBehaviour
{
    public static System.Action<bool> OnTimeSwapped;

    [Header("World Objects")]
    [SerializeField] GameObject present;
    [SerializeField] GameObject past;

    [Header("Atmosphere (Post-Processing)")]
    [SerializeField] Volume presentVolume;
    [SerializeField] Volume pastVolume;
    [SerializeField] float transitionDuration = 1.0f; 

    [Header("Audio")]
    [SerializeField] AudioSource ambienceSource; 
    [SerializeField] AudioClip presentAmbience;
    [SerializeField] AudioClip pastAmbience;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioClip warpSound; 

    private bool isPresentActive;
    private bool isSwitching = false; // Prevents spamming 'E'

    void Start()
    {
        // 1. SMART START
        // Instead of forcing it to true, we check what the scene looks like right now.
        // This allows you to place multiple obelisks without breaking them.
        if (present != null)
        {
            isPresentActive = present.activeSelf;
        }

        // Set volumes based on the detected state
        if(presentVolume) presentVolume.weight = isPresentActive ? 1 : 0;
        if(pastVolume) pastVolume.weight = isPresentActive ? 0 : 1;

        // Set Audio based on the detected state
        if(ambienceSource)
        {
            ambienceSource.clip = isPresentActive ? presentAmbience : pastAmbience;
            ambienceSource.Play();
        }
    }

    public void Interact()
    {
        // SPAM CHECK: If we are already mid-warp, ignore the click
        if (isSwitching) return;

        // SYNC CHECK (The Fix for "Press Twice"):
        // Before we do anything, update our memory to match reality.
        // If Obelisk A changed the world, Obelisk B updates itself right here.
        isPresentActive = present.activeSelf;

        // Play the warp sound
        if (sfxSource && warpSound) sfxSource.PlayOneShot(warpSound);

        // Flip the state
        isPresentActive = !isPresentActive;

        // Lock the script and start transition
        isSwitching = true; 
        StartCoroutine(TransitionRoutine());
    }

    private void UpdateWorldState()
    {
        if (present) present.SetActive(isPresentActive);
        if (past) past.SetActive(!isPresentActive);

        OnTimeSwapped?.Invoke(isPresentActive);
    }

    private IEnumerator TransitionRoutine()
    {
        float timer = 0f;
        float startVolPresent = presentVolume.weight;
        float startVolPast = pastVolume.weight;
        float startMusicVolume = ambienceSource.volume;

        // Target weights based on where we are going
        float targetVolPresent = isPresentActive ? 1f : 0f;
        float targetVolPast = isPresentActive ? 0f : 1f;

        // Pick the correct audio tape
        AudioClip targetClip = isPresentActive ? presentAmbience : pastAmbience;

        // --- PHASE 1: FADE OUT ---
        while (timer < transitionDuration / 2)
        {
            timer += Time.deltaTime;
            float t = timer / (transitionDuration / 2); 

            presentVolume.weight = Mathf.Lerp(startVolPresent, targetVolPresent, t);
            pastVolume.weight = Mathf.Lerp(startVolPast, targetVolPast, t);
            ambienceSource.volume = Mathf.Lerp(startMusicVolume, 0f, t);

            yield return null;
        }

        // --- THE SWAP ---
        UpdateWorldState(); // Switch the walls/floors
        
        ambienceSource.clip = targetClip;
        ambienceSource.Play();

        // --- PHASE 2: FADE IN ---
        timer = 0f;
        while (timer < transitionDuration / 2)
        {
            timer += Time.deltaTime;
            float t = timer / (transitionDuration / 2);

            presentVolume.weight = Mathf.Lerp(presentVolume.weight, targetVolPresent, t);
            pastVolume.weight = Mathf.Lerp(pastVolume.weight, targetVolPast, t);
            ambienceSource.volume = Mathf.Lerp(0f, startMusicVolume, t);

            yield return null;
        }

        // Clean up exact values
        presentVolume.weight = targetVolPresent;
        pastVolume.weight = targetVolPast;
        ambienceSource.volume = startMusicVolume;

        // UNLOCK the button
        isSwitching = false;
    }
}