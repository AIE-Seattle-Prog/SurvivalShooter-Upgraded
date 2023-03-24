using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerController : MonoBehaviour
{
    public PlayerHealth health;
    public PlayerMovement movement;
    public PlayerShooting shoot;
    [Space]
    public PlayerInput input;

    public float shootPlaneOffset = 0.0f;
    public Transform gunOffset;

    [Header("Mouse Settings")]
    public LayerMask floorMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
    public float camRayLength = 100f;          // The length of the ray from the camera into the scene.

    private void OnEnable()
    {
        input.currentActionMap["Fire"].performed += HandleFire;
        input.currentActionMap["Fire"].canceled += HandleFire;

        input.currentActionMap["MoveHorizontal"].performed += HandleMoveHorizontal;
        input.currentActionMap["MoveHorizontal"].canceled += HandleMoveHorizontal;

        input.currentActionMap["MoveVertical"].performed += HandleMoveVertical;
        input.currentActionMap["MoveVertical"].canceled += HandleMoveVertical;

        input.currentActionMap["Pause"].performed += HandlePause;
    }

    private void OnDisable()
    {
        if (gameObject.scene.isLoaded)
        {
            input.currentActionMap["Fire"].performed -= HandleFire;
            input.currentActionMap["Fire"].canceled -= HandleFire;

            input.currentActionMap["MoveHorizontal"].performed -= HandleMoveHorizontal;
            input.currentActionMap["MoveHorizontal"].canceled -= HandleMoveHorizontal;

            input.currentActionMap["MoveVertical"].performed -= HandleMoveVertical;
            input.currentActionMap["MoveVertical"].canceled -= HandleMoveVertical;

            input.currentActionMap["Pause"].performed -= HandlePause;
        }
    }

    private void Update()
    {
        //
        // MOUSE LOGIC for AIMING
        //

        // Create a ray from the mouse cursor on screen in the direction of the camera.
        Ray camRay = Camera.main.ScreenPointToRay(input.currentActionMap["Aim"].ReadValue<Vector2>());

        Vector3 hitPoint = Vector3.zero;

        // Perform the raycast and if it hits something on the floor layer...
        if (Physics.Raycast(camRay, out var floorHit, camRayLength, floorMask))
        {
            // Determine hitpoint from RaycastHit
            hitPoint = floorHit.point;
        }
        else
        {
            // Construct an endless plane that mimics the floor plane
            Plane groundPlane = new Plane(Vector3.up, 0.0f);

            // NOTE: 'enter' is the distance that the ray travelled to hit the plane
            if(groundPlane.Raycast(camRay, out var enter))
            {
                // pass 'enter' to 'GetPoint' on Ray to determine hit point
                hitPoint = camRay.GetPoint(enter);
            }
        }

        // Create a vector from the player to the point on the floor the raycast from the mouse hit.
        Vector3 playerToMouse = hitPoint - transform.position;

        // Ensure the vector is entirely along the floor plane.
        playerToMouse.y = 0f;

        // Offset rotation to account for distance between body and gun
        float aimOffset = (gunOffset.position - transform.position).magnitude;
        float distToShoot = playerToMouse.magnitude;
        float turnOffset = Mathf.Rad2Deg * Mathf.Asin(aimOffset / distToShoot);

        // fallback to excluding offset if invalid
        if(float.IsNaN(turnOffset)) { turnOffset = 0f; }

        // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
        Quaternion newRotation = Quaternion.LookRotation(playerToMouse) * Quaternion.AngleAxis(-turnOffset, Vector3.up);

        // Pass to movement component
        movement.SetMoveRotation(newRotation);
    }

    private void HandlePause(CallbackContext obj)
    {
        PauseManager.Instance.SetPause(!PauseManager.Instance.IsPaused);
    }

    private void HandleFire(CallbackContext callbackContext)
    {
        if (callbackContext.phase == InputActionPhase.Performed) { shoot.isFiring = true; }
        else if (callbackContext.phase == InputActionPhase.Canceled) { shoot.isFiring = false; }
    }

    private void HandleMoveHorizontal(CallbackContext callbackContext)
    {
        movement.SetMoveHorizontal(callbackContext.ReadValue<float>());
    }

    public void HandleMoveVertical(CallbackContext callbackContext)
    {
        movement.SetMoveForward(callbackContext.ReadValue<float>());
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;

        Vector3 debugOrigin = transform.position;
        debugOrigin.y = shootPlaneOffset;

        Gizmos.DrawRay(debugOrigin, Vector3.forward * 2.0f);
        Gizmos.DrawRay(debugOrigin, Vector3.right * 2.0f);
    }
}
