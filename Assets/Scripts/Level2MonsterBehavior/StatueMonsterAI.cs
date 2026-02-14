using UnityEngine;
using UnityEngine.AI;

public class StatueMonsterAI : MonoBehaviour
{
    [Header("Settings")]
    public Transform player;
    public float chaseSpeed = 10f; 
    
    [Header("Detection")]
    public Renderer monsterRenderer; // Drag your mesh here!
    public Camera mainCam;
    
    private NavMeshAgent agent;
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        // Auto-assign player and camera if missing
        if (mainCam == null) mainCam = Camera.main;
        if (player == null) 
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        
        agent.speed = chaseSpeed;

        // --- THE "LEVEL 2" TRIGGER ---
        // This tells the Animator: "We are in the chase level now. Stop idling."
        // Since Level 1 doesn't have this script, this will never happen there.
        if (animator != null)
        {
            animator.SetBool("isChasing", true);
        }
    }

    void Update()
    {
        if (player == null) return;

        // Logic: If visible, FREEZE. If hidden, CHASE.
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
        // Check if monster is inside the camera's view
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCam);
        if (GeometryUtility.TestPlanesAABB(planes, monsterRenderer.bounds))
        {
            // Optional: Add Raycast here if you want walls to block line of sight
            return true; 
        }
        return false;
    }

    void Freeze()
    {
        if (agent != null) agent.isStopped = true; // Stop moving
        if (animator != null) animator.speed = 0;  // PAUSE animation (Statue effect)
    }

    void Chase()
    {
        if (agent != null)
        {
            agent.isStopped = false; 
            agent.SetDestination(player.position);
        }
        
        if (animator != null)
        {
            animator.speed = 1; // UNPAUSE animation
            
            // Redundancy: Ensure we stay in the running state
            animator.SetBool("isChasing", true); 
        }
    }
}