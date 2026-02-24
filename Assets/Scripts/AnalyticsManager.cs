using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class LevelEvents
{
    public int obelisk_travels;
    public List<string> keys_gotten = new List<string>();
    public List<string> unlocked_doors = new List<string>();
    public int deaths;
    public float time_spent; // Tracks seconds spent in this specific level
}

[Serializable]
public class AnalyticsPayload
{
    public string player_uid;
    public string version_number;
    public string ip_address;
    public string session_id;
    public string timestamp;
    public LevelEvents[] levels; // Fixed array of 4 levels
    public bool winner;
    public int dormant_tabs;
}

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance;

    public string backendEndpoint = "https://dndgameanalytics.duckdns.org/analytics";
    public float sendIntervalSeconds = 30f;

    private AnalyticsPayload sessionData;
    private int currentLevelIndex = 0; // 0 = Level 1, 3 = Level 4
    private bool isTrackingTime = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSessionData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartCoroutine(AnalyticsHeartbeat());
    }

    private void Update()
    {
        // Add delta time to the current level's time_spent
        if (isTrackingTime && currentLevelIndex >= 0 && currentLevelIndex < 4 && !sessionData.winner)
        {
            sessionData.levels[currentLevelIndex].time_spent += Time.deltaTime;
        }

        // // TEMPORARY TEST: Press Spacebar to track a death and send the data
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     TrackDeath();
        //     SendLevelAnalytics();
        //     Debug.Log("Spacebar pressed: Attempting to send analytics...");
        // }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) sessionData.dormant_tabs++;
    }

    private void OnApplicationQuit()
    {
        SendLevelAnalytics();
    }

    private void InitializeSessionData()
    {
        sessionData = new AnalyticsPayload
        {
            player_uid = SystemInfo.deviceUniqueIdentifier,
            version_number = Application.version, // Change this to your assigned version if needed
            ip_address = "handled_by_backend",
            session_id = Guid.NewGuid().ToString(),
            levels = new LevelEvents[5],
            winner = false,
            dormant_tabs = 0
        };

        // Initialize the 4 levels so they aren't null
        for (int i = 0; i < 4; i++)
        {
            sessionData.levels[i] = new LevelEvents();
        }
    }

    private IEnumerator AnalyticsHeartbeat()
    {
        while (true)
        {
            yield return new WaitForSeconds(sendIntervalSeconds);
            SendLevelAnalytics();
        }
    }

    // --- Call these from your player/game logic ---

    // Sets the active level (0 to 3). Time will automatically start accumulating for the new level.
    public void SetCurrentLevelIndex(int zeroBasedLevelIndex)
    {
        if (zeroBasedLevelIndex >= 0 && zeroBasedLevelIndex < 5)
        {
            currentLevelIndex = zeroBasedLevelIndex;
            isTrackingTime = true;
        }
        else
        {
            Debug.LogWarning("Level index out of bounds (must be 0-3).");
        }
    }

    // Pause time tracking (e.g., if the game is paused or transitioning)
    public void PauseTimeTracking(bool pause) => isTrackingTime = !pause;

    public void TrackObeliskTravel() => sessionData.levels[currentLevelIndex].obelisk_travels++;
    
    // Now takes a string so you can track WHICH key was gotten
    // Mimics a Set by checking if the list already contains the key/door
    public void TrackKeyObtained(string keyId) 
    {
        if (!sessionData.levels[currentLevelIndex].keys_gotten.Contains(keyId))
        {
            sessionData.levels[currentLevelIndex].keys_gotten.Add(keyId);
        }
    }
    
    public void TrackDoorUnlocked(string doorId) 
    {
        if (!sessionData.levels[currentLevelIndex].unlocked_doors.Contains(doorId))
        {
            sessionData.levels[currentLevelIndex].unlocked_doors.Add(doorId);
        }
    }
    
    public void TrackDeath() => sessionData.levels[currentLevelIndex].deaths++;

    // Call this when the player beats the final level
    public void SetWinner()
    {
        sessionData.winner = true;
        isTrackingTime = false; // Stop the clock
        SendLevelAnalytics();
    }

    // --- Network Logic ---
    public void SendLevelAnalytics()
    {
        // Update the timestamp to the exact moment of sending
        sessionData.timestamp = DateTimeOffset.UtcNow.ToString("o");

        string jsonPayload = JsonUtility.ToJson(sessionData);
        StartCoroutine(PostData(jsonPayload));
    }

    private IEnumerator PostData(string json)
    {
        using (UnityWebRequest request = new UnityWebRequest(backendEndpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log("Analytics Sent!");
            else
                Debug.LogError($"Analytics Error: {request.error}");
        }
    }
}