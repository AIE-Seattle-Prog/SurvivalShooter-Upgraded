using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AIHearing : MonoBehaviour
{

    //[SerializeField] float HearingRange;

    [HideInInspector] public int CurrentNoisePriority;

    [HideInInspector] public Vector3 NoisePosition;

    [SerializeField] AIRelayChannel Channel;

    [Space]

    [SerializeField] UnityEvent<Vector3> OnNoiseDetected;

    private void Start()
    {
        if (Channel != null) { Channel.AddListener(this); }
    }

    public void DetectNoise(Vector3 NoisePos, int Priority)
    {
        CurrentNoisePriority = Priority;

        NoisePosition = NoisePos;

        OnNoiseDetected.Invoke(NoisePos);

        Debug.Log("[AI]Noise Detected!!!");
    }
}
