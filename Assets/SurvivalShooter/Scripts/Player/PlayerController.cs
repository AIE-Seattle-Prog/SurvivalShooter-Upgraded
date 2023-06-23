using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerCharacter Character { get; private set; }
    [Space]
    public PlayerInput Input;

    public void Possess(PlayerCharacter newBody)
    {
        Debug.Log("Possess!", this);
        Character = newBody;
        Character.BindToController(this);   
    }

    public void Unpossess()
    {
        Debug.Log("Unpossess!", this);
        if (Character != null)
        {
            Character.UnbindFromController();
        }

        Character = null;
    }
}
