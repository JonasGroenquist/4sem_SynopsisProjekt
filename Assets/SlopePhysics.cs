using UnityEngine;
using StarterAssets; // Add this line to reference the correct namespace

public class SlopePhysics : MonoBehaviour
{
    [Header("Slope Settings")]
    public float maxWalkableSlope = 40f;    // Maximum angle the player can walk on
    public float slidingSpeed = 10f;        // Base speed for sliding
    public float slidingAcceleration = 5f;  // How quickly sliding accelerates

    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private bool isSliding = false;
    private float currentSlideSpeed = 0f;

    // Reference to your main character script - adjust name as needed
    private ThirdPersonController mainController;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainController = GetComponent<ThirdPersonController>();
    }

    void Update()
    {
        HandleSlopes();
    }

    void HandleSlopes()
    {
        if (controller.isGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
            {
                // Calculate slope angle
                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);

                // Debug slope info
                Debug.DrawRay(hit.point, hit.normal * 2f, Color.yellow);

                // Check if slope is too steep to walk on
                if (slopeAngle > maxWalkableSlope)
                {
                    // We're on a steep slope - start sliding
                    isSliding = true;

                    // Calculate slide direction (down the slope)
                    Vector3 slopeDirection = Vector3.Cross(hit.normal, Vector3.Cross(Vector3.up, hit.normal));
                    slopeDirection.Normalize();

                    // Increase sliding speed over time
                    currentSlideSpeed = Mathf.MoveTowards(currentSlideSpeed, slidingSpeed, slidingAcceleration * Time.deltaTime);

                    // Set movement direction down the slope
                    moveDirection = slopeDirection * currentSlideSpeed;

                    // Apply some player control during sliding (optional)
                    Vector3 playerInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                    playerInput = Camera.main.transform.TransformDirection(playerInput);
                    playerInput.y = 0; // Keep input horizontal
                    playerInput.Normalize();

                    // Add slight player influence to the slide direction
                    moveDirection += playerInput * 2f;

                    // Apply movement
                    controller.Move(moveDirection * Time.deltaTime);

                    // Alert the main controller that we're handling movement
                    if (mainController != null)
                    {
                        // Call any method in your main controller to disable its movement
                        // Example: mainController.SetControlEnabled(false);
                    }
                }
                else
                {
                    // Reset sliding state
                    isSliding = false;
                    currentSlideSpeed = 0f;

                    // Return control to main controller
                    if (mainController != null)
                    {
                        // Example: mainController.SetControlEnabled(true);
                    }
                }
            }
        }
        else
        {
            // Not grounded, continue sliding if we were sliding
            if (isSliding)
            {
                controller.Move(moveDirection * Time.deltaTime);
            }
        }
    }
}