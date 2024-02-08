using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AISight : MonoBehaviour
{

    [Header("Vision Cone Stats and Settings")]
    [Tooltip("Vision Cone Array with custom stats.")]
    public
    Cone[] Cones;

    [Tooltip("Used to determine the cone's distance")]float ViewRadius;
   [Tooltip("The cone's width in degrees")][Range(0, 360)]float ViewAngle;

    [Space]

    [Tooltip("The layer mask for anything you want to catch the attention of your AI")]public LayerMask TargetMask;

    public LayerMask ObstructionMask;

    [Space]
    
    [Tooltip("The amount of time to wait before checking for targets within the radius.")]
    [Range(0f, 1f)][SerializeField] float TargetCheckDelay;

    float delayTimer = 0f;
    [HideInInspector]
    public List<GameObject> VisibleTargets = new List<GameObject>();

    [Space]

    [Header("Detection Events")]
    [SerializeField] UnityEvent<GameObject> OnDetectBegin;
    [SerializeField] UnityEvent<GameObject> OnDetectEnd;



    private void Update()
    {

        delayTimer += Time.deltaTime;

        if(delayTimer >= TargetCheckDelay)
        {
            FindVisibleTargets();

            delayTimer = 0f;
        }
    }

    //Checks for targets in the view radius
    void FindVisibleTargets()
    {

        for(int i = 0; i < Cones.Length; i++)
        {
            List<Collider> TargetsInRad = new List<Collider>();
            TargetsInRad.AddRange(Physics.OverlapSphere(transform.position, Cones[i].ViewRadius, TargetMask));

            List<GameObject> radObjects = new List<GameObject>();


            for(int j = 0; j < TargetsInRad.Count; j++)
            {
                Collider Target = TargetsInRad[j];

                radObjects.Add(Target.gameObject);

                Vector3 TargetDir = (Target.transform.position - transform.position).normalized;

                if (Vector3.Angle(transform.forward, TargetDir) < Cones[i].ViewAngle / 2)
                {
                    float TargetDist = Vector3.Distance(transform.position, Target.transform.position);

                    if(!Physics.Raycast(transform.position, TargetDir, TargetDist, ObstructionMask) && !VisibleTargets.Contains(Target.gameObject))
                    {
                        VisibleTargets.Add(Target.gameObject);

                        OnDetectBegin.Invoke(Target.gameObject);
                    }
                }        
            



            }
         
            //Removes any objects no longer in sight
            for(int j = 0; j < VisibleTargets.Count; j++)
            {
                Vector3 ObDir = (VisibleTargets[j].transform.position - transform.position).normalized;

                if (!radObjects.Contains(VisibleTargets[j]) || Vector3.Angle(transform.forward, ObDir) > Cones[i].ViewAngle / 2 || Physics.Raycast(transform.position, ObDir, Cones[i].ViewRadius, ObstructionMask))
                {   
                    OnDetectEnd.Invoke(VisibleTargets[j]);
                    VisibleTargets.Remove(VisibleTargets[j]); 
                }
            }

        }


    }
    //Returns a direction from an angle
    public Vector3 DirFromAngle(float angleInDeg, bool AngleIsGlobal)
    {
        if(!AngleIsGlobal)
        {
            angleInDeg += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDeg * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDeg * Mathf.Deg2Rad));
    }

    // TODO: Expose a function that lets you query which cones are currently
    //       detecting a target

    //public Cone? GetConeByIndex(int Index)
    //{
    //    if(Index > Cones.Length || Index < 0)
    //    {
    //        Debug.LogError("Index " + Index + " is out of bounds of the cone array!");

    //        return null;
    //    }

    //    return Cones[Index];
    //}

    //public Cone? GetConeByName(string ConeName)
    //{
    //    int ConeIndex;

    //    for(int i = 0; i < Cones.Length; i++)
    //    {
    //        if(Cones[i].Name == ConeName)
    //        {
    //            return Cones[i];
    //        }
    //    }


    //    Debug.LogError("Cone of name " + ConeName + " does not exist!");

    //    return null;
    //}

    //Vision Cone Struct
    [System.Serializable]
    public struct Cone
    {
        public string Name;

        [Tooltip("Used to determine the cone's distance")] public float ViewRadius;
        [Tooltip("The cone's width in degrees")][Range(0, 360)] public float ViewAngle;

        [Space]

        [Header("Editor Tools")]
        public Color ConeColor;
        public Color RadiusColor;
    }

}
