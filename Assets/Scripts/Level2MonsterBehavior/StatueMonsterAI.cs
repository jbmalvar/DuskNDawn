using UnityEngine;
using UnityEngine.AI;

public class StatueMonsterAI : MonoBehaviour
{
    [Header("Settings")]
    public Transform player;
    public float chaseSpeed = 10f; 
    public string runAnimationBool = "isRunning"; // The name of your bool parameter in Animator
    
    [Header("Detection")]
    public float lookBuffer = 0.1f; // Small delay to prevent jittering at edge of screen

    private NavMeshAgent agent;
    private Animator animator;
    private Renderer monsterRenderer;
    private Camera mainCam;
    private bool isMoving = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        monsterRenderer = GetComponent<Renderer>();
        mainCam = Camera.main;

        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        agent.speed = chaseSpeed;
    }

    void Update()
    {
        if (player == null) return;

        // Check if the monster is visible to the main camera
        if (IsVisibleToCamera())
        {
            Freeze();
        }
        else
        {
            Chase();
        }
    }

    bool IsVisibleToCamera()
    {
        // 1. Calculate the camera's view frustum (the pyramid of vision)
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCam);

        // 2. Check if the monster's collider bounds are within those planes
        return GeometryUtility.TestPlanesAABB(planes, monsterRenderer.bounds);
    }

    void Freeze()
    {
        if (!isMoving) return; // Already stopped

        isMoving = false;
        agent.isStopped = true;
        agent.ResetPath(); // Clears path so it stops immediately
        
        // Stop Animation
        if (animator != null) animator.SetBool(runAnimationBool, false);
    }

    void Chase()
    {
        isMoving = true;
        agent.isStopped = false;
        agent.SetDestination(player.position);

        // Play Run Animation
        if (animator != null) animator.SetBool(runAnimationBool, true);
    }
}