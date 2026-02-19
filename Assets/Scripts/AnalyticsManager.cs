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
    public bool gotten_the_key;
    public List<string> unlocked_doors = new List<string>();
    public int deaths;
}

[Serializable]
public class AnalyticsPayload
{
    public string player_uid;
    public string version_number;
    public string ip_address;
    public string session_id;
    public string timestamp;
    public LevelEvents level_data;
    public int dormant_tabs; 
}

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance;

    // Points to your local Docker container
    public string backendEndpoint = "http://localhost:3000/analytics"; 

    private string sessionId;
    private int dormantTabCount = 0;
    private LevelEvents currentLevelEvents;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            sessionId = Guid.NewGuid().ToString();
            currentLevelEvents = new LevelEvents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // TEMPORARY TEST: Press Spacebar to track a death and send the data
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TrackDeath();
            SendLevelAnalytics();
            Debug.Log("Spacebar pressed: Attempting to send analytics...");
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) dormantTabCount++;
    }

    // --- Call these from your player/game logic ---
    public void TrackObeliskTravel() => currentLevelEvents.obelisk_travels++;
    public void TrackKeyObtained() => currentLevelEvents.gotten_the_key = true;
    public void TrackDoorUnlocked(string doorId) => currentLevelEvents.unlocked_doors.Add(doorId);
    public void TrackDeath() => currentLevelEvents.deaths++;

    // --- Call this when the level ends ---
    public void SendLevelAnalytics()
    {
        AnalyticsPayload payload = new AnalyticsPayload
        {
            player_uid = SystemInfo.deviceUniqueIdentifier,
            version_number = Application.version,
            ip_address = "handled_by_backend",
            session_id = sessionId,
            timestamp = DateTimeOffset.UtcNow.ToString("o"),
            level_data = currentLevelEvents,
            dormant_tabs = dormantTabCount
        };

        StartCoroutine(PostData(JsonUtility.ToJson(payload)));
        currentLevelEvents = new LevelEvents(); // Reset for next level
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