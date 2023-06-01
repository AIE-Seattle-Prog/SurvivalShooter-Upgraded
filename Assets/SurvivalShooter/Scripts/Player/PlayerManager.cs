using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [field: SerializeField]
    public PlayerInputManager PlayerInputs { get; private set; }
}
