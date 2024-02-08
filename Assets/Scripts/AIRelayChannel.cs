using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AIRelayChannel : ScriptableObject
{
    [NonSerialized]
    List<AIHearing> Listeners = new();

    public void AddListener(AIHearing ai)
    {
        Listeners.Add(ai);
    }

    // TODO : Remove a listener when it destroyed

    public void BroadcastNoise(Vector3 NoisePos, int Priority)
    {
        for (int i = 0; i < Listeners.Count; i++)
        {
            Listeners[i].DetectNoise(NoisePos, Priority);
        }
    }
}
