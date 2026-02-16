using UnityEngine;
using UnityEngine.AI;

public class StatueMonsterAI : MonoBehaviour
{
    [Header("Settings")]
    public Transform player;
    public float chaseSpeed = 10f; 
    
    [Header("Detection")]
    public Renderer monsterRenderer; 
    public Camera mainCam;
    public AudioSource movementAudio;
    
    // Internal variables
    private NavMeshAgent agent;
    private Animator animator;

    void Start()
    {
        // 1. Get the Agent (It is on THIS object, the Driver)
        agent = GetComponent<NavMeshAgent>();
        
        // 2. Get the Animator (It is on the CHILD object, the Passenger)
        // We use GetComponentInChildren to find it automatically.
        animator = GetComponentInChildren<Animator>();

        // 3. Get the Renderer (It is on the CHILD)
        if (monsterRenderer == null) monsterRenderer = GetComponentInChildren<Renderer>();
        
        // 4. Setup Player/Camera
        if (mainCam == null) mainCam = Camera.main;
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // 5. Force the Agent settings
        if (agent != null)
        {
            agent.speed = chaseSpeed;
            agent.acceleration = 100f;
            agent.angularSpeed = 2000f; 
            agent.autoBraking = false;
        }
    }

    void Update()
    {
        if (player == null) return;

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
        if (monsterRenderer == null) return false;
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCam);
        return GeometryUtility.TestPlanesAABB(planes, monsterRenderer.bounds);
    }

    void Chase()
    {
        if (agent != null)
        {
            agent.isStopped = false; 
            agent.SetDestination(player.position);

            // AUDIO ON
            if (movementAudio != null && !movementAudio.isPlaying) movementAudio.Play();
        }
        
        // ANIMATION RUN
        if (animator != null)
        {
            animator.speed = 1; 
            animator.SetBool("isChasing", true); 
        }
    }

    void Freeze()
    {
        if (agent != null) 
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // AUDIO OFF
        if (movementAudio != null) movementAudio.Stop();

        // ANIMATION FREEZE
        if (animator != null) 
        {
            animator.speed = 0; 
        }
    }
}