using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISoundRelay : MonoBehaviour
{
    [SerializeField] AIHearing[] AIHearingTargets;

    //public void BroadcastNoise(Vector3 NoisePos)
    //{
    //    for(int i = 0; i < AIHearingTargets.Length; i++)
    //    {
    //        AIHearingTargets[i].DetectNoise(NoisePos);
    //    }
    //}

    public void BroadcastNoise(Vector3 NoisePos, int Priority)
    {
        for (int i = 0; i < AIHearingTargets.Length; i++)
        {
            AIHearingTargets[i].DetectNoise(NoisePos, Priority);
        }
    }

    
}
