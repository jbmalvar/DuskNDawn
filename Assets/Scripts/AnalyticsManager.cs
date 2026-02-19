using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class AnalyticsPayload
{
    public string player_uid;
    public string version_number;
    public string ip_address;
    public string session_id;
    public string timestamp;
    public int dormant_tabs; 
    
    public int level_index;
    public int obelisk_travels;
    public int keys_obtained;
    public List<string> unlocked_doors = new List<string>();
    public int deaths;
}

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance;

    public string backendEndpoint = "https://game-analytics-latest.onrender.com";
    // public string backendEndpoint = "http://34.172.200.224:3000/analytics";
    
    // 1. Lowered the interval to 30 seconds
    public float sendIntervalSeconds = 30f; 

    private string sessionId;
    private int dormantTabCount = 0;
    
    private int currentLevelIndex;
    private int obeliskTravels;
    private int keysObtained;
    private List<string> unlockedDoors = new List<string>();
    private int deaths;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            sessionId = Guid.NewGuid().ToString();
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendLevelAnalytics();
            Debug.Log("Spacebar pressed: Attempting to send analytics...");
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) dormantTabCount++;
    }

    private IEnumerator AnalyticsHeartbeat()
    {
        while (true)
        {
            yield return new WaitForSeconds(sendIntervalSeconds);
            // 2. Updated the log message to reflect the new 30-second timer
            Debug.Log("30 seconds elapsed: Attempting to send analytics...");
            SendLevelAnalytics();
        }
    }

    // --- Tracking Methods ---
    public void SetCurrentLevel(int level) => currentLevelIndex = level;
    public void TrackObeliskTravel() => obeliskTravels++;
    public void TrackKeyObtained() => keysObtained++; 
    public void TrackDoorUnlocked(string doorId) => unlockedDoors.Add(doorId);
    public void TrackDeath() => deaths++;

    // --- Send (NO RESET) ---
    public void SendLevelAnalytics()
    {
        AnalyticsPayload payload = new AnalyticsPayload
        {
            player_uid = SystemInfo.deviceUniqueIdentifier,
            version_number = Application.version,
            ip_address = "handled_by_backend",
            session_id = sessionId,
            timestamp = DateTimeOffset.UtcNow.ToString("o"),
            dormant_tabs = dormantTabCount,
            
            level_index = currentLevelIndex,
            obelisk_travels = obeliskTravels,
            keys_obtained = keysObtained,
            unlocked_doors = new List<string>(unlockedDoors), 
            deaths = deaths
        };

        StartCoroutine(PostData(JsonUtility.ToJson(payload)));
        
        // 3. The reset lines that were here are completely gone!
        // The numbers will now keep climbing until the player quits or the scene restarts.
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