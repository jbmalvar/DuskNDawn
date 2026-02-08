using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool hasDoorKey = false;

    public void CollectKey()
    {
        hasDoorKey = true;
        Debug.Log("Inventory: Key added.");
    }
}