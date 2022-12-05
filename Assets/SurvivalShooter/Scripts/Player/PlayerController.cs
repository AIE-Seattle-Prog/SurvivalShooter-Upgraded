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

    public PlayerInput input;

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

    private void HandlePause(CallbackContext obj)
    {
        PauseManager.Instance.SetPause(!PauseManager.Instance.IsPaused);
    }

    private void OnDisable()
    {
        input.currentActionMap["Fire"].performed -= HandleFire;
        input.currentActionMap["Fire"].canceled -= HandleFire;

        input.currentActionMap["MoveHorizontal"].performed -= HandleMoveHorizontal;
        input.currentActionMap["MoveHorizontal"].canceled -= HandleMoveHorizontal;

        input.currentActionMap["MoveVertical"].performed -= HandleMoveVertical;
        input.currentActionMap["MoveVertical"].canceled -= HandleMoveVertical;

        input.currentActionMap["Pause"].performed -= HandlePause;
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
}
