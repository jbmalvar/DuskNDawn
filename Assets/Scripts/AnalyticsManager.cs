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
    // --- The Lazy-Loading Auto-Instantiating Singleton ---
    private static AnalyticsManager _instance;
    public static AnalyticsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // First, check if it exists but we just lost the pointer
                _instance = FindObjectOfType<AnalyticsManager>();
                
                // If it's truly missing, allocate and create it on the fly
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("AnalyticsManager_AutoCreated");
                    _instance = singletonObject.AddComponent<AnalyticsManager>();
                    // AddComponent immediately calls Awake(), handling initialization
                }
            }
            return _instance;
        }
    }

    public string backendEndpoint = "https://dndgameanalytics.duckdns.org/analytics";
    public float sendIntervalSeconds = 30f;

    private AnalyticsPayload sessionData;
    private int currentLevelIndex = 0; 
    private bool isTrackingTime = true;
    private bool isDormant = false; 
    private bool isInitialized = false;

    private void Awake()
    {
        // Enforce the singleton pattern
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSessionData();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Catch-all for components manually dragged into scenes
        if (!isInitialized) 
        {
            InitializeSessionData();
        }
    }

    private void Start()
    {
        StartCoroutine(AnalyticsHeartbeat());
    }

    private void Update()
    {
        if (isTrackingTime && IsLevelIndexValid() && !sessionData.winner)
        {
            sessionData.levels[currentLevelIndex].time_spent += Time.deltaTime;
        }
    }

    // --- WebGL Dormant Tab Logic ---
    private void OnApplicationFocus(bool hasFocus)
    {
        HandleDormancy(!hasFocus);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        HandleDormancy(pauseStatus);
    }

    private void HandleDormancy(bool isNowDormant)
    {
        if (!isInitialized) return;

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
        if (isInitialized) SendLevelAnalytics();
    }

    // --- Data Initialization ---
    private void InitializeSessionData()
    {
        if (isInitialized) return;

        string savedUID = PlayerPrefs.GetString("Analytics_PlayerUID", "");
        if (string.IsNullOrEmpty(savedUID))
        {
            savedUID = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("Analytics_PlayerUID", savedUID);
            PlayerPrefs.Save();
        }

        sessionData = new AnalyticsPayload
        {
            player_uid = savedUID, 
            version_number = "1.1.0B", 
            ip_address = "handled_by_backend",
            session_id = Guid.NewGuid().ToString(), 
            levels = new LevelEvents[5], 
            winner = false,
            dormant_tabs = 0
        };

        for (int i = 0; i < sessionData.levels.Length; i++)
        {
            sessionData.levels[i] = new LevelEvents();
        }

        isInitialized = true;
    }

    private IEnumerator AnalyticsHeartbeat()
    {
        while (true)
        {
            yield return new WaitForSeconds(sendIntervalSeconds);
            if (isInitialized) SendLevelAnalytics();
        }
    }

    // --- Safety Check ---
    private bool IsLevelIndexValid()
    {
        return sessionData != null && 
               sessionData.levels != null && 
               currentLevelIndex >= 0 && 
               currentLevelIndex < sessionData.levels.Length;
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
            Debug.LogWarning($"Analytics: Level index {zeroBasedLevelIndex} out of bounds.");
        }
    }

    public void PauseTimeTracking(bool pause) => isTrackingTime = !pause;

    public void TrackObeliskTravel()
    {
        if (IsLevelIndexValid()) sessionData.levels[currentLevelIndex].obelisk_travels++;
    }
    
    public void TrackKeyObtained(string keyId) 
    {
        if (IsLevelIndexValid() && !sessionData.levels[currentLevelIndex].keys_gotten.Contains(keyId))
            sessionData.levels[currentLevelIndex].keys_gotten.Add(keyId);
    }
    
    public void TrackDoorUnlocked(string doorId) 
    {
        if (IsLevelIndexValid() && !sessionData.levels[currentLevelIndex].unlocked_doors.Contains(doorId))
            sessionData.levels[currentLevelIndex].unlocked_doors.Add(doorId);
    }
    
    public void TrackDeath()
    {
        if (IsLevelIndexValid()) sessionData.levels[currentLevelIndex].deaths++;
    }

    public void SetWinner()
    {
        if (!isInitialized) return;
        sessionData.winner = true;
        isTrackingTime = false; 
        SendLevelAnalytics();
    }

    // --- Network Logic ---
    public void SendLevelAnalytics()
    {
        if (!isInitialized || sessionData == null) return;

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

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Analytics Error: {request.error}");
            }
        }
    }
}