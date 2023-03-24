using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;            // The speed that the player will move at.
    public float airSpeed = 1f;         // The speed that the player will move at mid-air.
    Vector3 movement;                   // The vector to store the direction of the player's movement.
    
    public Animator anim;                      // Reference to the animator component.
    public Rigidbody playerRigidbody;          // Reference to the player's rigidbody.
    public CapsuleCollider playerCollider;     // Reference to the player's collider.

    private float moveHorizontal = 0.0f;
    private float moveVertical = 0.0f;
    private Quaternion moveRotation = Quaternion.identity;
    [SerializeField]
    private LayerMask groundLayer;
    [FormerlySerializedAs("groundCheckDistance")] [SerializeField]
    private float bonusGroundCheckDistance = 1.0f;
    [SerializeField]
    private float maxGroundAngle = 75.0f;

    private bool wasGrounded;

    void FixedUpdate ()
    {
        // Move the player around the scene.
        Move (moveHorizontal, moveVertical);

        // Turn the player to face the mouse cursor.
        Turning ();

        // Animate the player.
        Animating (moveHorizontal, moveVertical);
    }

    public void SetMoveHorizontal(float horizontal)
    {
        moveHorizontal = horizontal;
    }

    public void SetMoveForward(float forward)
    {
        moveVertical = forward;
    }

    public void SetMoveRotation(Quaternion rotation)
    {
        moveRotation = rotation;

        // TODO: remove any rotation on the euler X and Z axes
    }

    bool CheckGrounded(out Vector3 groundPose)
    {
        Vector3 colliderCenter = transform.TransformPoint(playerCollider.center);
        Vector3 colliderTop = transform.TransformPoint(playerCollider.center + new Vector3(0,playerCollider.height / 2 - playerCollider.radius,0));
        float distance = (playerCollider.height - playerCollider.radius * 2) + bonusGroundCheckDistance;
        var hits = Physics.SphereCastAll(colliderTop, playerCollider.radius, Vector3.down, distance, groundLayer);
        
        Debug.DrawRay(colliderTop, Vector3.down * distance, Color.red);
        
        Array.Sort(hits, (hit1, hit2) => hit2.distance.CompareTo(hit1.distance));
        
        foreach(var hit in hits)
        {
            if (Vector3.Angle(hit.normal, Vector3.up) < maxGroundAngle)
            {
                groundPose = colliderTop + new Vector3(0, -(hit.distance + playerCollider.radius), 0);
                return true;
            }
        }

        groundPose = Vector3.zero;
        return false;
    }
    
    void Move (float h, float v)
    {
        // Set the movement vector based on the axis input.
        movement.Set (h, movement.y, v);
        
        bool isGrounded = CheckGrounded(out Vector3 groundPose);
        
        float effectiveSpeed = isGrounded ? speed : airSpeed;
        
        // Remove y-velocity for ground movement calculations
        Vector3 movementXZ = new Vector3(movement.x, 0.0f, movement.z);
        movementXZ = Vector3.ClampMagnitude(movementXZ, 1.0f);
        
        // Normalise the movement vector and make it proportional to the speed per second.
        movementXZ = movementXZ * (effectiveSpeed * Time.deltaTime);

        movement.x = movementXZ.x;
        // y is retained
        movement.z = movementXZ.z;

        // Update y-velocity based on grounded/not-grounded state
        Vector3 groundAlignment = Vector3.zero;
        if (isGrounded)
        {
            movement.y = 0.0f;
            groundAlignment.y += groundPose.y - transform.position.y;
        }
        else
        {
            movement.y += Physics.gravity.y * Time.deltaTime * Time.deltaTime;
        }
        
        // Move the player to it's current position plus the movement.
        playerRigidbody.MovePosition (transform.position + movement + groundAlignment);

        wasGrounded = isGrounded;
    }


    void Turning ()
    {
        // Set the player's rotation to this new rotation.
        playerRigidbody.MoveRotation(moveRotation);
    }
    
    void Animating (float h, float v)
    {
        // Create a boolean that is true if either of the input axes is non-zero.
        bool walking = h != 0f || v != 0f;

        // Tell the animator whether or not the player is walking.
        anim.SetBool ("IsWalking", walking);
    }

    private void Reset()
    {
        anim = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
    }
}