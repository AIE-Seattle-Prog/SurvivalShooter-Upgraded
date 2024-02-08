using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AINoisemaker))]
public class AINoisemakerEditor : Editor
{
    private void OnSceneGUI()
    {
        AINoisemaker Noisemaker = (AINoisemaker)target;

        

        for(int i = 0; i < Noisemaker.Noises.Length; i++)
        {
            Handles.color = Noisemaker.Noises[i].NoiseRadiusColor;
            Handles.DrawWireArc(Noisemaker.transform.position, Vector3.up, Vector3.forward, 360, Noisemaker.Noises[i].Radius);
        }
    }
}
