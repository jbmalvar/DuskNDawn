using UnityEngine;

public class LevelTracker : MonoBehaviour
{
    [Tooltip("Set this to 0 for Level 1, 1 for Level 2, etc.")]
    public int levelIndex;

    private void Start()
    {
        // As soon as this scene loads, tell the Analytics Manager which level we are in
        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.SetCurrentLevelIndex(levelIndex);
            
            // Optional: You can trigger an immediate send here if you want 
            // the database to know the second they start a new level
            // AnalyticsManager.Instance.SendLevelAnalytics(); 
            
            Debug.Log($"Analytics: Now tracking Level {levelIndex + 1}");
        }
        else
        {
            Debug.LogWarning("AnalyticsManager missing! Make sure it loads in your first scene.");
        }
    }
}