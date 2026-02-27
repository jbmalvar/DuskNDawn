using UnityEngine;

[System.Serializable]
public class Objective 
{
    public Transform keyTransform; 
    public GameObject doorToHighlight; 
}

public class ObjectiveManager : MonoBehaviour
{
    public Objective[] objectives;
    
    [Header("UI Compass Settings")]
    public RectTransform uiArrow; 
    public Transform playerCamera; 
    
    private int currentIndex = 0;

    void Update()
    {
        if (uiArrow.gameObject.activeSelf && currentIndex < objectives.Length)
        {
            PointUIArrowAt(objectives[currentIndex].keyTransform.position);
        }
    }

    public void OnKeyCollected()
    {
        HighlightDoor(objectives[currentIndex].doorToHighlight);
        currentIndex++;

        if (currentIndex >= objectives.Length)
        {
            uiArrow.gameObject.SetActive(false); 
        }
    }

    private void PointUIArrowAt(Vector3 targetPosition)
    {
        Vector3 directionToTarget = targetPosition - playerCamera.position;
        directionToTarget.y = 0; 

        Vector3 cameraForward = playerCamera.forward;
        cameraForward.y = 0;

        float angle = Vector3.SignedAngle(cameraForward, directionToTarget, Vector3.up);

        uiArrow.localEulerAngles = new Vector3(0, 0, -angle + 90f);
    }

    private void HighlightDoor(GameObject door)
    {
        if (door != null)
        {
            door.GetComponent<Renderer>().material.color = Color.red; 
        }
    }
}