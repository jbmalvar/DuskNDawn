using UnityEngine;


public class KeyItem : MonoBehaviour
{
    [Header("Key Settings")]
    [Tooltip("The name of this key. Must match the Door's Key ID.")]
    public string keyID = "GeneralKey"; // Default name

    public void Pickup()
    {
        Debug.Log($"KeyItem: You picked up the {keyID}.");
        AnalyticsManager.Instance.TrackKeyObtained(keyID);
        Destroy(gameObject); 
    }
}