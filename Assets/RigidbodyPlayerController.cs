using UnityEngine;
using UnityEngine.InputSystem;

public class RigidbodyPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float sprintSpeed = 15f;
    public float jumpForce = 12f;
    public float turnSpeed = 15f;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 45f;
    public float groundSnapForce = 15f;
    public float slopeSlideSpeed = 2f;

    [Header("Ground Detection")]
    public LayerMask groundMask;
    public float groundDistance = 0.4f;

    [Header("References")]
    public Transform cameraRoot;

    private Rigidbody rb;
    private Animator animator;
    private StarterAssets.StarterAssetsInputs input;
    private RaycastHit slopeHit;

    private bool isGrounded;
    private bool isOnSlope;
    private Vector3 slopeNormal;
    private float slopeAngle;

    private float targetRotation;
    private float rotationVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        input = GetComponent<StarterAssets.StarterAssetsInputs>();

        // Configure rigidbody
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        CheckGroundStatus();
        HandleJumpInput();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        MovePlayer();
        HandleSlopes();
    }

    bool IsOnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 1.5f, groundMask))
        {
            slopeNormal = slopeHit.normal;
            slopeAngle = Vector3.Angle(slopeNormal, Vector3.up);
            return slopeAngle > 1f && slopeAngle <= maxSlopeAngle;
        }
        return false;
    }

    void CheckGroundStatus()
    {
        // Spherecast for ground detection
        isGrounded = Physics.CheckSphere(transform.position - new Vector3(0, 0.9f, 0), groundDistance, groundMask);

        // Check if we're on a walkable slope
        isOnSlope = IsOnSlope();

        // Debug visuals
        Debug.DrawRay(transform.position, Vector3.down * 1.5f, isGrounded ? Color.green : Color.red);
        if (isOnSlope)
            Debug.DrawRay(transform.position, slopeNormal * 1.5f, Color.blue);
    }

    void HandleJumpInput()
    {
        if (input.jump && isGrounded)
        {
            // Calculate jump force to achieve desired height
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            input.jump = false;

            if (animator)
                animator.SetTrigger("Jump");

            Debug.Log("Jump executed with force: " + jumpForce);
        }
    }

    void MovePlayer()
    {
        // Get input direction and normalize
        Vector3 inputDirection = new Vector3(input.move.x, 0, input.move.y).normalized;

        // Handle rotation based on input
        if (input.move != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                             Camera.main.transform.eulerAngles.y;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation,
                                                  ref rotationVelocity, 0.05f);

            transform.rotation = Quaternion.Euler(0, rotation, 0);
        }

        // Calculate move direction in world space
        Vector3 moveDirection = Quaternion.Euler(0, targetRotation, 0) * Vector3.forward;

        // Set target speed based on sprint input
        float targetSpeed = input.sprint ? sprintSpeed : moveSpeed;
        if (input.move == Vector2.zero) targetSpeed = 0;

        // Handle movement with slope compensation
        if (isGrounded)
        {
            Vector3 movementVector;

            if (isOnSlope)
            {
                // Project movement onto the slope
                movementVector = Vector3.ProjectOnPlane(moveDirection, slopeNormal).normalized * targetSpeed;
            }
            else
            {
                // Normal flat movement
                movementVector = moveDirection * targetSpeed;
            }

            // Override vertical velocity when grounded
            if (rb.linearVelocity.y < 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, -2f, rb.linearVelocity.z);
            }

            // Apply movement
            rb.AddForce(movementVector, ForceMode.Acceleration);

            // Limit horizontal velocity for better control
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > targetSpeed)
            {
                Vector3 limitedVelocity = horizontalVelocity.normalized * targetSpeed;
                rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
            }
        }
        else
        {
            // Air control
            rb.AddForce(moveDirection * targetSpeed * 0.15f, ForceMode.Acceleration);
        }
    }

    void HandleSlopes()
    {
        if (isGrounded)
        {
            if (isOnSlope)
            {
                // Apply downward force to stick to slope
                rb.AddForce(Vector3.down * groundSnapForce, ForceMode.Force);

                // If not moving and on steep slope, slide down
                if (input.move == Vector2.zero && slopeAngle > 20f)
                {
                    // Calculate slide direction (down the slope)
                    Vector3 slideDirection = Vector3.up - slopeNormal * Vector3.Dot(Vector3.up, slopeNormal);
                    rb.AddForce(slideDirection.normalized * slopeSlideSpeed, ForceMode.Force);
                }
            }
            else
            {
                // On flat ground, apply grounding force
                rb.AddForce(Vector3.down * 5f, ForceMode.Force);
            }
        }
    }

    void UpdateAnimator()
    {
        if (animator)
        {
            // Calculate speed for animation
            float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

            // Update animation parameters
            animator.SetFloat("Speed", speed);
            animator.SetBool("Grounded", isGrounded);

            // Update falling animation
            if (!isGrounded && rb.linearVelocity.y < -2f)
            {
                animator.SetBool("FreeFall", true);
            }
            else
            {
                animator.SetBool("FreeFall", false);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw ground check sphere
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position - new Vector3(0, 0.9f, 0), groundDistance);
    }
}