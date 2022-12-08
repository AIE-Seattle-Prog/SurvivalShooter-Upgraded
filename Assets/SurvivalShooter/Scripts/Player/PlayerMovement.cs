using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;            // The speed that the player will move at.
    Vector3 movement;                   // The vector to store the direction of the player's movement.
    
    public Animator anim;                      // Reference to the animator component.
    public Rigidbody playerRigidbody;          // Reference to the player's rigidbody.

    private float moveHorizontal = 0.0f;
    private float moveVertical = 0.0f;
    private Quaternion moveRotation = Quaternion.identity;

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

    void Move (float h, float v)
    {
        // Set the movement vector based on the axis input.
        movement.Set (h, 0f, v);
        
        // Normalise the movement vector and make it proportional to the speed per second.
        movement = movement.normalized * speed * Time.deltaTime;

        // Move the player to it's current position plus the movement.
        playerRigidbody.MovePosition (transform.position + movement);
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