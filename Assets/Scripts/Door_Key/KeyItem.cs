using UnityEngine;

public class KeyItem : MonoBehaviour
{
    public void Pickup()
    {
        Debug.Log("KeyItem: You picked up the key.");
        Destroy(gameObject); // Removes the key from the world
    }
}