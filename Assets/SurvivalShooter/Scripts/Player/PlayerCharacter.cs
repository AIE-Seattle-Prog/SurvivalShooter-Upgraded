using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using static UnityEngine.InputSystem.InputAction;

public class PlayerCharacter : MonoBehaviour
{
    public PlayerHealth health;
    public PlayerMovement movement;
    public PlayerShooting shoot;
    public PlayerController Controller { get; private set; }
    private InputActionMap lastBoundActionMap;
    
    [Header("Settings")]
    public float shootPlaneOffset = 0.0f;
    public Transform gunOffset;

    [Header("Mouse Settings")]
    public LayerMask floorMask;                      // A layer mask so that a ray can be cast just at gameobjects on the floor layer.
    public float camRayLength = 100f;          // The length of the ray from the camera into the scene.

    private bool didBindInputs = false;

    //
    // Character Binding
    //

    public void BindToController(PlayerController player)
    {
        Debug.Assert(false == didBindInputs, "Failed to unbind inputs from previous controller before binding to new one!", this);
        Controller = player;

        Debug.Log("Using current control scheme: " + Controller.Input.currentControlScheme, this);
        Debug.Log("Using current action map: " + Controller.Input.currentActionMap, this);

        didBindInputs = true;

        Controller.Input.currentActionMap["Fire"].performed += HandleFire;
        Controller.Input.currentActionMap["Fire"].canceled += HandleFire;
        
        Controller.Input.currentActionMap["MoveHorizontal"].performed += HandleMoveHorizontal;
        Controller.Input.currentActionMap["MoveHorizontal"].canceled += HandleMoveHorizontal;
        
        Controller.Input.currentActionMap["MoveVertical"].performed += HandleMoveVertical;
        Controller.Input.currentActionMap["MoveVertical"].canceled += HandleMoveVertical;
        
        Controller.Input.currentActionMap["Pause"].performed += HandlePause;
        
        Controller.Input.currentActionMap["Aim"].performed += HandleAim;
        Controller.Input.currentActionMap["Turn"].performed += HandleTurn;

        lastBoundActionMap = Controller.Input.currentActionMap;
    }

    public void UnbindFromController()
    {
        bool wasBoundInputs = didBindInputs;

        if (lastBoundActionMap != null)
        {
            lastBoundActionMap["Fire"].performed -= HandleFire;
            lastBoundActionMap["Fire"].canceled -= HandleFire;

            lastBoundActionMap["MoveHorizontal"].performed -= HandleMoveHorizontal;
            lastBoundActionMap["MoveHorizontal"].canceled -= HandleMoveHorizontal;

            lastBoundActionMap["MoveVertical"].performed -= HandleMoveVertical;
            lastBoundActionMap["MoveVertical"].canceled -= HandleMoveVertical;

            lastBoundActionMap["Pause"].performed -= HandlePause;

            lastBoundActionMap["Aim"].performed -= HandleAim;
            lastBoundActionMap["Turn"].performed -= HandleTurn;

            lastBoundActionMap = null;

            didBindInputs = false;
        }

        Debug.Assert(false == (wasBoundInputs && didBindInputs), "Failed to unbind inputs! Erroneous input events may occur.", this);

        Controller = null;
    }

    //
    // Input Handlers
    //

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

    private void HandleAim(CallbackContext callbackContext)
    {
        //
        // MOUSE LOGIC for AIMING
        //

        // Create a ray from the mouse cursor on screen in the direction of the camera.
        Ray camRay = Camera.main.ScreenPointToRay(callbackContext.ReadValue<Vector2>());

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
            if (groundPlane.Raycast(camRay, out var enter))
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
        if (float.IsNaN(turnOffset)) { turnOffset = 0f; }

        // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
        Quaternion newRotation = Quaternion.LookRotation(playerToMouse) * Quaternion.AngleAxis(-turnOffset, Vector3.up);

        // Pass to movement component
        movement.SetMoveRotation(newRotation);
    }

    private void HandleTurn(CallbackContext obj)
    {
        Vector2 raw = obj.ReadValue<Vector2>();

        Vector3 rawWorld = new Vector3(raw.x, 0.0f, raw.y);

        if (rawWorld.sqrMagnitude > 0.0f)
        {
            // Create a quaternion (rotation) based on looking down the vector from the player to the mouse.
            Quaternion newRotation = Quaternion.LookRotation(rawWorld);

            // Pass to movement component
            movement.SetMoveRotation(newRotation);
        }
    }

    //
    // MonoBehaviour Magic
    //

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;

        Vector3 debugOrigin = transform.position;
        debugOrigin.y = shootPlaneOffset;

        Gizmos.DrawRay(debugOrigin, Vector3.forward * 2.0f);
        Gizmos.DrawRay(debugOrigin, Vector3.right * 2.0f);
    }

    private void OnDestroy()
    {
        PlayerController currentController = Controller;
        UnbindFromController();
        currentController.Unpossess();
    }
}
