using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    [Header("Footstep Settings")]
    public AudioClip[] footstepClips;
    public float baseStepInterval = 0.5f; // Adjust this for timing between steps
    private float footstepTimer = 0;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private AudioSource audioSource;

    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Movement Speed logic
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;

        // Preserve vertical velocity (gravity) but recalculate horizontal movement
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Apply Gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y = movementDirectionY - (gravity * Time.deltaTime);
        }
        else
        {
            moveDirection.y = -0.5f;
        }

        // Crouch Logic
        if (Input.GetKey(KeyCode.R) && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 6f;
            runSpeed = 12f;
        }

        // Execute Move
        characterController.Move(moveDirection * Time.deltaTime);

        // Footstep Logic
        HandleFootsteps();

        // Camera Rotation
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }

    private void HandleFootsteps()
    {
        // Get horizontal velocity magnitude
        float horizontalVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z).magnitude;

        if (characterController.isGrounded && horizontalVelocity > 0.1f)
        {
            // The timer decreases based on how fast we are moving
            footstepTimer -= Time.deltaTime * horizontalVelocity;

            if (footstepTimer <= 0)
            {
                if (footstepClips.Length > 0)
                {
                    audioSource.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)]);
                }
                footstepTimer = baseStepInterval;
            }
        }
    }
}