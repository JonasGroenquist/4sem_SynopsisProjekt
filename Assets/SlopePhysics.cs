using UnityEngine;

public class SlopePhysics : MonoBehaviour
{
    private CharacterController controller;
    public float slopeForce = 15f;
    public float slopeForceRayLength = 1.5f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        // Only check when grounded
        if (!controller.isGrounded) return;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, slopeForceRayLength))
        {
            // Calculate slope angle
            float angle = Vector3.Angle(hit.normal, Vector3.up);

            // Only apply force on steep slopes
            if (angle > controller.slopeLimit)
            {
                // Calculate direction down the slope
                Vector3 hitNormal = hit.normal;
                Vector3 slopeDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);

                Vector3 moveDirection = Vector3.ClampMagnitude(slopeDirection * slopeForce, slopeForce);
                controller.Move(moveDirection * Time.fixedDeltaTime);

                // Debug visual in Scene view
                Debug.DrawRay(hit.point, slopeDirection * 2, Color.yellow);
            }
        }
    }
}