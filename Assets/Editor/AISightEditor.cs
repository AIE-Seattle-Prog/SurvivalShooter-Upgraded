using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AISight))]
public class AISightEditor : Editor
{
    private void OnSceneGUI()
    {
        AISight Sight = (AISight)target;
        for(int i = 0; i < Sight.Cones.Length; i++)
        {
            Handles.color = Sight.Cones[i].RadiusColor;
            Handles.DrawWireArc(Sight.transform.position, Vector3.up, Vector3.forward, 360, Sight.Cones[i].ViewRadius);

            Vector3 AngleA = Sight.DirFromAngle(-Sight.Cones[i].ViewAngle / 2, false);
            Vector3 AngleB = Sight.DirFromAngle(Sight.Cones[i].ViewAngle / 2, false);

            Handles.color = Sight.Cones[i].ConeColor;

            Handles.DrawLine(Sight.transform.position, Sight.transform.position + AngleA * Sight.Cones[i].ViewRadius);
            Handles.DrawLine(Sight.transform.position, Sight.transform.position + AngleB * Sight.Cones[i].ViewRadius);

        
            Handles.color = Color.red;

            foreach (GameObject VisibleTarget in Sight.VisibleTargets)
            {
                Handles.DrawLine(Sight.transform.position, VisibleTarget.transform.position);
            }
        }

    }
}
