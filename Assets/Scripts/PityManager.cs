using UnityEngine;

public class PityManager : MonoBehaviour
{
    // This makes the manager accessible from anywhere
    public static PityManager Instance { get; private set; }

    [Header("Pity Settings")]
    [Tooltip("Percentage to reduce monster speed per death (e.g., 0.05 = 5% slower)")]
    public float monsterSlowdownPercentage = 0.05f;
    
    [Tooltip("Percentage to increase stamina regen per death (e.g., 0.10 = 10% faster)")]
    public float staminaBoostPercentage = 0.10f;

    // These variables hold the active multipliers
    public float currentMonsterSpeedMultiplier = 1f;
    public float currentStaminaRegenMultiplier = 1f;

    private void Awake()
    {

        // Singleton pattern: Ensure only one instance exists and it survives scene loads
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps this object alive across scenes
        }
        else
        {
            Destroy(gameObject); // Destroys duplicates if you reload the initial scene
        }
    }

    // Call this method every time the player dies
    public void RegisterDeath()
    {
        // Reduce monster speed multiplier
        currentMonsterSpeedMultiplier *= (1f - monsterSlowdownPercentage);
        
        // Increase stamina regen multiplier
        currentStaminaRegenMultiplier *= (1f + staminaBoostPercentage);
        
        Debug.Log($"Player Died! New Monster Speed: {currentMonsterSpeedMultiplier}x | New Stamina Regen: {currentStaminaRegenMultiplier}x");
    }
}