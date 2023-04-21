using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(EnemyRoundConfig))]
public class EnemyRoundConfigEditor : Editor
{
    SerializedProperty propEnemyCount;
    SerializedProperty propEnemyChances;
    SerializedProperty propEnemyOverrides;

    private void OnEnable()
    {
        propEnemyCount = serializedObject.FindProperty("numberOfEnemies");
        propEnemyChances = serializedObject.FindProperty("enemyChances");
        propEnemyOverrides = serializedObject.FindProperty("enemyOverrides");
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(propEnemyCount);

        EditorGUILayout.PropertyField(propEnemyChances);
        // reset all chances to 1 when pressed
        if(GUILayout.Button("Reset Chances"))
        {
            for(int i = 0; i < propEnemyChances.arraySize; ++i)
            {
                propEnemyChances.GetArrayElementAtIndex(i).FindPropertyRelative("weight").intValue = 1;
            }

            // make sure to save changes!
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.PropertyField(propEnemyOverrides);
        bool shouldWarnUnusedOverride = false;
        for (int i = 0; i < propEnemyOverrides.arraySize; ++i)
        {
            int overrideIndex = propEnemyOverrides.GetArrayElementAtIndex(i).FindPropertyRelative("position").intValue;
            if(overrideIndex > propEnemyCount.intValue)
            {
                shouldWarnUnusedOverride = true;
                break;
            }
        }

        if (shouldWarnUnusedOverride)
        {
            EditorGUILayout.HelpBox("One or more enemy overrides will not be used.\n\nThe number of enemies spawned will not extend to the spawn that will be overridden.", MessageType.Warning);
        }
    }
}
