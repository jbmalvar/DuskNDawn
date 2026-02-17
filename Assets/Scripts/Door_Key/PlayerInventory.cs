using UnityEngine;
using System.Collections.Generic; // Required for Lists

public class PlayerInventory : MonoBehaviour
{
    // Replaced single bool with a list of key names
    private List<string> collectedKeys = new List<string>();

    public void CollectKey(string keyID)
    {
        if (!collectedKeys.Contains(keyID))
        {
            collectedKeys.Add(keyID);
            Debug.Log($"Inventory: Picked up {keyID}");
        }
    }

    public bool HasKey(string keyID)
    {
        return collectedKeys.Contains(keyID);
    }
    
    // Helper to check if we have ANY keys (for the "Wrong Key" logic)
    public bool HasAnyKeys()
    {
        return collectedKeys.Count > 0;
    }
}