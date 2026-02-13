using UnityEngine;
using UnityEngine.UI; // REQUIRED for the Stamina Bar

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    [Header("UI (Drag Slider Here)")]
    public Slider staminaBar;
    public CanvasGroup staminaCanvasGroup; // Optional: To fade bar out

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float runDrainSpeed = 20f; // How fast energy goes down
    public float regenSpeed = 15f;    // How fast energy comes back
    private float currentStamina;
    private bool isExhausted = false; // Tracks if we pushed too hard

    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float gravity = 20f;
    
    [Header("Camera & Look")]
    public Camera playerCamera;
    public float lookSpeed = 2f;
    public float lookXLimit = 60f;

    [Header("Head Bob & Audio")]
    public bool enableHeadBob = true;
    public float bobSpeed = 10f;
    public float bobAmount = 0.05f;
    public AudioClip[] footstepClips;
    public float baseStepInterval = 0.5f;

    // Private internals
    private CharacterController characterController;
    private AudioSource audioSource;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private float defaultCameraY;
    private float bobTimer = 0;
    private float footstepTimer = 0;
    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        if (playerCamera != null) defaultCameraY = playerCamera.transform.localPosition.y;

        // Initialize Stamina
        currentStamina = maxStamina;
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- 1. MOVEMENT CALCULATION ---
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Check input
        bool isTryingToRun = Input.GetKey(KeyCode.LeftShift);
        
        // STAMINA LOGIC
        // We can only run if: We are holding Shift + We have Stamina + We are NOT exhausted + We are moving
        bool isMoving = Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0;
        bool canRun = isTryingToRun && currentStamina > 0 && !isExhausted && isMoving;

        // Set speed based on logic
        float curSpeedX = canMove ? (canRun ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (canRun ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;

        // Handle Stamina Drain/Regen
        if (canRun)
        {
            currentStamina -= runDrainSpeed * Time.deltaTime;
            
            // Check for Exhaustion
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true; // Player must wait to recharge
            }
        }
        else
        {
            // Regen stamina
            if (currentStamina < maxStamina)
            {
                currentStamina += regenSpeed * Time.deltaTime;
            }

            // Recovery from Exhaustion
            // (e.g., must recover to 25% before running again)
            if (isExhausted && currentStamina > (maxStamina * 0.25f))
            {
                isExhausted = false;
            }
        }

        // Update UI
        UpdateStaminaUI();

        // --- 2. PHYSICS & GRAVITY ---
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (!characterController.isGrounded)
            moveDirection.y = movementDirectionY - (gravity * Time.deltaTime);
        else
            moveDirection.y = -2f;

        characterController.Move(moveDirection * Time.deltaTime);

        // --- 3. CAMERA ---
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        // --- 4. EXTRAS ---
        if (canMove && characterController.isGrounded)
        {
            HandleHeadBob(curSpeedX, curSpeedY);
            HandleFootsteps(curSpeedX, curSpeedY);
        }
    }

    void UpdateStaminaUI()
    {
        if (staminaBar == null) return;

        staminaBar.value = currentStamina;

        // Optional: Hide bar if full (Classic Horror Style)
        if (staminaCanvasGroup != null)
        {
            if (currentStamina >= maxStamina - 1f)
                staminaCanvasGroup.alpha = Mathf.MoveTowards(staminaCanvasGroup.alpha, 0f, Time.deltaTime * 2f);
            else
                staminaCanvasGroup.alpha = Mathf.MoveTowards(staminaCanvasGroup.alpha, 1f, Time.deltaTime * 5f);
        }
    }

    void HandleHeadBob(float speedX, float speedY)
    {
        if (!enableHeadBob || playerCamera == null) return;

        if (Mathf.Abs(speedX) > 0.1f || Mathf.Abs(speedY) > 0.1f)
        {
            // Bob faster if running
            float speedMult = (Mathf.Abs(speedX) > walkSpeed + 1f) ? 1.5f : 1f;
            bobTimer += Time.deltaTime * (bobSpeed * speedMult);
            
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultCameraY + Mathf.Sin(bobTimer) * bobAmount,
                playerCamera.transform.localPosition.z
            );
        }
        else
        {
            bobTimer = 0;
            Vector3 newPos = playerCamera.transform.localPosition;
            newPos.y = Mathf.Lerp(newPos.y, defaultCameraY, Time.deltaTime * 5f);
            playerCamera.transform.localPosition = newPos;
        }
    }

    void HandleFootsteps(float speedX, float speedY)
    {
        if (Mathf.Abs(speedX) > 0.1f || Mathf.Abs(speedY) > 0.1f)
        {
            // Normalize speed to prevent machine gun audio
            float speedRatio = (new Vector2(speedX, speedY).magnitude) / walkSpeed;
            
            footstepTimer -= Time.deltaTime * speedRatio;

            if (footstepTimer <= 0)
            {
                if (footstepClips.Length > 0)
                    audioSource.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)]);
                
                footstepTimer = baseStepInterval;
            }
        }
    }
}