using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerWidget : MonoBehaviour
{
    public PlayerController OwningPlayer { get; private set; }

    public void BindToPlayer(PlayerController player)
    {
        OwningPlayer = player;
        OnBindToPlayer(player);
    }

    public void UnbindFromPlayer()
    {
        OnUnbindFromPlayer();
        OwningPlayer = null;
    }

    protected virtual void OnBindToPlayer(PlayerController player)
    {
        OwningPlayer = player;
    }

    protected virtual void OnUnbindFromPlayer()
    {
        OwningPlayer = null;
    }

}
