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
    public float time_spent; 
}

[Serializable]
public class AnalyticsPayload
{
    public string player_uid;
    public string version_number;
    public string ip_address;
    public string session_id;
    public string timestamp;
    public LevelEvents[] levels; 
    public bool winner;
    public int dormant_tabs;
}

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance;

    public string backendEndpoint = "https://dndgameanalytics.duckdns.org/analytics";
    public float sendIntervalSeconds = 30f;

    private AnalyticsPayload sessionData;
    private int currentLevelIndex = 0; 
    private bool isTrackingTime = true;
    private bool isDormant = false; // Added to prevent double-counting in WebGL

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
        if (isTrackingTime && currentLevelIndex >= 0 && currentLevelIndex < sessionData.levels.Length && !sessionData.winner)
        {
            sessionData.levels[currentLevelIndex].time_spent += Time.deltaTime;
        }
    }

    // --- WebGL Dormant Tab Fixes ---
    private void OnApplicationFocus(bool hasFocus)
    {
        // Triggers when clicking on/off the WebGL Canvas on the webpage
        HandleDormancy(!hasFocus);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // Triggers when actually switching browser tabs
        HandleDormancy(pauseStatus);
    }

    private void HandleDormancy(bool isNowDormant)
    {
        if (isNowDormant && !isDormant)
        {
            isDormant = true;
            sessionData.dormant_tabs++;
        }
        else if (!isNowDormant && isDormant)
        {
            isDormant = false;
        }
    }

    private void OnApplicationQuit()
    {
        // NOTE: This rarely fires in WebGL, but we keep it just in case.
        SendLevelAnalytics();
    }

    private void InitializeSessionData()
    {
        // --- WebGL Player UID Fix ---
        string savedUID = PlayerPrefs.GetString("Analytics_PlayerUID", "");
        if (string.IsNullOrEmpty(savedUID))
        {
            // First time playing: Generate a new ID and save it to the browser
            savedUID = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("Analytics_PlayerUID", savedUID);
            PlayerPrefs.Save();
        }

        sessionData = new AnalyticsPayload
        {
            player_uid = savedUID, // Now pulls from browser LocalStorage
            version_number = "1.1", 
            ip_address = "handled_by_backend",
            session_id = Guid.NewGuid().ToString(), // Perfectly fine for WebGL
            levels = new LevelEvents[5], // Explicitly 5 levels
            winner = false,
            dormant_tabs = 0
        };

        // Dynamically initialize based on the array length so it never crashes
        for (int i = 0; i < sessionData.levels.Length; i++)
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

    // --- Game Logic Trackers ---
    public void SetCurrentLevelIndex(int zeroBasedLevelIndex)
    {
        if (zeroBasedLevelIndex >= 0 && zeroBasedLevelIndex < sessionData.levels.Length)
        {
            currentLevelIndex = zeroBasedLevelIndex;
            isTrackingTime = true;
        }
        else
        {
            Debug.LogWarning($"Level index {zeroBasedLevelIndex} out of bounds.");
        }
    }

    public void PauseTimeTracking(bool pause) => isTrackingTime = !pause;

    public void TrackObeliskTravel() => sessionData.levels[currentLevelIndex].obelisk_travels++;
    
    public void TrackKeyObtained(string keyId) 
    {
        if (!sessionData.levels[currentLevelIndex].keys_gotten.Contains(keyId))
            sessionData.levels[currentLevelIndex].keys_gotten.Add(keyId);
    }
    
    public void TrackDoorUnlocked(string doorId) 
    {
        if (!sessionData.levels[currentLevelIndex].unlocked_doors.Contains(doorId))
            sessionData.levels[currentLevelIndex].unlocked_doors.Add(doorId);
    }
    
    public void TrackDeath() => sessionData.levels[currentLevelIndex].deaths++;

    public void SetWinner()
    {
        sessionData.winner = true;
        isTrackingTime = false; 
        SendLevelAnalytics();
    }

    // --- Network Logic ---
    public void SendLevelAnalytics()
    {
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