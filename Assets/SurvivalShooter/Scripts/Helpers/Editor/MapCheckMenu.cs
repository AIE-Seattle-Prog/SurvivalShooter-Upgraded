using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public static class MapCheckMenu
{
    [MenuItem("GameObject/Run Map Check")]
    public static void RunMapCheckOnCurrentScene()
    {
        var checkable = Object.FindObjectsOfType<MonoBehaviour>().OfType<IMapCheck>();

        foreach(var obj in checkable)
        {
            if(!obj.Check())
            {
                var unityObject = (MonoBehaviour)obj;
                Debug.LogError($"Map check failed on {unityObject.name}", unityObject);
            }
        }
    }
}