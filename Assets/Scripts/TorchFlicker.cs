using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
    private Light fireLight;
    
    [Header("Flicker Settings")]
    public float minIntensity = 200f; // The dimmest the fire gets
    public float maxIntensity = 500f; // The brightest the fire gets
    public float flickerSpeed = 3.5f; // Lower = lazy fire, Higher = windy fire
    
    [Header("Movement")]
    public bool wobblePosition = true; // Moves light slightly to simulate flame dancing
    public float wobbleAmount = 0.05f; 
    
    private Vector3 originalPos;
    private float timeSeed;

    void Start()
    {
        fireLight = GetComponent<Light>();
        originalPos = transform.localPosition;
        // Random start point so multiple torches don't pulse in sync
        timeSeed = Random.Range(0f, 100f); 
    }

    void Update()
    {
        if (fireLight == null) return;

        // 1. Calculate Intensity (Brightness)
        // We use Perlin Noise for smooth, organic transitions
        float noiseVal = Mathf.PerlinNoise(timeSeed + Time.time * flickerSpeed, 0);
        fireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noiseVal);

        // 2. Wobble Position (Optional realism)
        // This moves the light tiny amounts to simulate the flame moving on the stick
        if (wobblePosition)
        {
            float x = (Mathf.PerlinNoise(timeSeed + Time.time * flickerSpeed, 1) - 0.5f) * wobbleAmount;
            float y = (Mathf.PerlinNoise(timeSeed + Time.time * flickerSpeed, 2) - 0.5f) * wobbleAmount;
            float z = (Mathf.PerlinNoise(timeSeed + Time.time * flickerSpeed, 3) - 0.5f) * wobbleAmount;
            transform.localPosition = originalPos + new Vector3(x, y, z);
        }
    }
}