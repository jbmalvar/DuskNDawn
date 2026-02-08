using UnityEngine;

public class TorchPickup : MonoBehaviour
{
    public GameObject torchInHand;

    // This function will be called by the Player's camera script
    public void PickUp()
    {
        if (torchInHand != null)
        {
            torchInHand.SetActive(true);
            Destroy(gameObject);
        }
    }
}