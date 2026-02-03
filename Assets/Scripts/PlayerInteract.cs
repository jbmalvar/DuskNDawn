using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float range = 3f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            AttemptInteract();
        }
    }

    void AttemptInteract()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            TimeTravel timeScript = hit.collider.GetComponent<TimeTravel>();

            if (timeScript != null)
            {
                timeScript.Interact();
            }
        }
    }

}
