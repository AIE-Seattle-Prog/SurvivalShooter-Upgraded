using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Noise", menuName = "AISenses/Noise", order = 1)]
public class Noise : ScriptableObject
{
    public AudioClip Clip;

    public float Radius;

    public int PriorityLevel;

    public bool IsGlobal;

    public LayerMask AIMasks;

    public Color NoiseRadiusColor;
}
