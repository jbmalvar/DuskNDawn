using UnityEngine;

public class GroupTimeMaterialSwapper : MonoBehaviour
{
    [Header("Time Travel Materials")]
    [SerializeField] private Material presentMaterial;
    [SerializeField] private Material pastMaterial;

    [Header("Exclusions")]
    [Tooltip("Any object with this tag will NOT change textures.")]
    [SerializeField] private string ignoreTag = "IgnoreTimeTravel";

    private MeshRenderer[] childRenderers;

    private void Awake()
    {
        childRenderers = GetComponentsInChildren<MeshRenderer>(true);
    }

    private void OnEnable()
    {
        TimeTravel.OnTimeSwapped += UpdateMaterials;
    }

    private void OnDisable()
    {
        TimeTravel.OnTimeSwapped -= UpdateMaterials;
    }

    private void Start()
    {
        if (presentMaterial != null && pastMaterial != null)
        {
            UpdateMaterials(true); 
        }
    }

    private void UpdateMaterials(bool isPresentActive)
    {
        Material targetMaterial = isPresentActive ? presentMaterial : pastMaterial;

        foreach (MeshRenderer rend in childRenderers)
        {
            if (rend != null)
            {
                // SAFETY CHECK: If the object has our special tag, skip it!
                if (rend.gameObject.CompareTag(ignoreTag))
                {
                    continue; 
                }

                rend.material = targetMaterial;
            }
        }
    }
}