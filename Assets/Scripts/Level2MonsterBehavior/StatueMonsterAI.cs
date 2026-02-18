using UnityEngine;
using UnityEngine.AI;
using System.Collections; // Required for Coroutines

public class StatueMonsterAI : MonoBehaviour
{
    [Header("Settings")]
    public Transform player;
    public float chaseSpeed = 10f; 
    
    [Header("Detection")]
    public Renderer monsterRenderer; 
    public Camera mainCam;
    public AudioSource movementAudio;

    [Header("Jumpscare & UI")]
    public AudioSource jumpscareAudio; // Drag the SCREAM audio here
    public GameObject deathCanvas;     // Drag your UI Canvas here
    public float jumpScareDuration = 2.0f; // How long to wait before Game Over screen
    
    // Internal variables
    private NavMeshAgent agent;
    private Animator animator;
    private bool isDead = false; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Auto-find components on Child
        animator = GetComponentInChildren<Animator>();
        if (monsterRenderer == null) monsterRenderer = GetComponentInChildren<Renderer>();
        
        // Auto-find Player/Camera
        if (mainCam == null) mainCam = Camera.main;
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Force Agent settings
    if (agent != null)
            {
                // FORCE SNAP TO NEAREST BLUE SPOT
                NavMeshHit hit;
                // Look for a spot within 5 meters
                if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out hit, 5.0f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    agent.Warp(hit.position); // Teleport to the valid spot
                }
            }
    }

    void Update()
    {
        // 1. STOP if game over or player missing
        if (isDead || player == null) return;

        // 2. Main Logic
        if (IsVisibleToCamera())
        {
            Freeze();
        }
        else
        {
            Chase();
        }
    }

    // --- TRIGGER: WHEN PLAYER TOUCHES MONSTER ---
    void OnTriggerEnter(Collider other)
    {
        // Only trigger if alive and touching Player
        if (!isDead && other.CompareTag("Player"))
        {
            // --- PEEK PROTECTION ---
            // If the monster is currently playing a "Peek" animation, DO NOT KILL.
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // Add any other peek state names here if you have them
            if (stateInfo.IsName("Peek") || stateInfo.IsName("peekIdle") || stateInfo.IsName("peek_finish"))
            {
                return; // Ignore the player
            }

            // Otherwise, start the kill sequence
            StartCoroutine(JumpscareRoutine());
        }
    }

    // --- SEQUENCE: ANIMATION -> WAIT -> UI ---

    IEnumerator JumpscareRoutine()
    {
        isDead = true; 

        // 1. DISABLE PLAYER CONTROL
        if (player != null)
        {
            var movementScript = player.GetComponent<PlayerMovement>();
            if (movementScript != null) movementScript.enabled = false;
        }

        // 2. Freeze Monster Base
        if (agent != null) 
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        if (movementAudio != null) movementAudio.Stop(); 

        // 3. Force Animation 
        if (animator != null) 
        {
            animator.speed = 1; 
            animator.SetBool("isChasing", false); 
            animator.Play("Jumpscare"); 
        }

        // 4. Play Scream
        if (jumpscareAudio != null) jumpscareAudio.Play();

        // 5. THE DEATH STARE LOOP
        // Save the original clip plane so we can restore it later if needed
        float originalClip = mainCam.nearClipPlane;
        mainCam.nearClipPlane = 0.01f; // <--- MAGIC FIX: Allows rendering SUPER close

        float timer = 0;
        while (timer < jumpScareDuration)
        {
            // Keep the monster body facing the camera
            Vector3 targetPosition = new Vector3(mainCam.transform.position.x, transform.position.y, mainCam.transform.position.z);
            transform.LookAt(targetPosition);

            if (mainCam != null && monsterRenderer != null)
            {
                // A. Find the center of the head (Eyes)
                Vector3 eyePos = monsterRenderer.bounds.center;
                eyePos.y = monsterRenderer.bounds.max.y - 0.25f; // Adjust this if looking too high/low

                // B. Lock Camera slightly in front of the face
                // Change '0.75f' to move closer/further. 
                // 0.4f is "Nose touching lens", 0.8f is "Personal space invasion"
                float faceDistance = 0.75f; 
                
                // This math places the camera directly in front of the monster's face
                // regardless of how the animation leans forward
                Vector3 intenseSpot = eyePos + (transform.forward * faceDistance);

                mainCam.transform.position = intenseSpot; 
                mainCam.transform.LookAt(eyePos);        
            }

            timer += Time.deltaTime;
            yield return null; 
        }

        // Restore camera setting (good practice)
        mainCam.nearClipPlane = originalClip;

        // 6. Show UI
        if (deathCanvas != null)
        {
            deathCanvas.SetActive(true);
            Cursor.lockState = CursorLockMode.None; 
            Cursor.visible = true;
        }
    }

    // --- EXISTING HELPER FUNCTIONS ---
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
            
            // Only play audio if moving
            if (movementAudio != null && !movementAudio.isPlaying) movementAudio.Play();
        }
        
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
        if (movementAudio != null) movementAudio.Stop();
        if (animator != null) animator.speed = 0; 
    }
}