using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnemyRoundConfig))]
public class EnemyRoundConfigEditor : Editor
{
    SerializedProperty propEnemyCount;
    SerializedProperty propEnemyChances;
    SerializedProperty propEnemyOverrides;

    // gets called when editor this object is opened
    private void OnEnable()
    {
        propEnemyCount = serializedObject.FindProperty("numberOfEnemies");
        propEnemyChances = serializedObject.FindProperty("enemyChances");
        propEnemyOverrides = serializedObject.FindProperty("enemyOverrides");
    }

    // gets called when inspector needs to refresh
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(propEnemyCount);
        EditorGUILayout.PropertyField(propEnemyChances);
        if(GUILayout.Button("Reset Weights"))
        {
            for(int i = 0; i < propEnemyChances.arraySize; ++i)
            {
                propEnemyChances.GetArrayElementAtIndex(i).FindPropertyRelative("weight").intValue = 1;
            }

            // MAKE SURE TO SAVE AND APPLY YOUR CHANGES
            serializedObject.ApplyModifiedProperties();
        }
        EditorGUILayout.PropertyField(propEnemyOverrides);

        bool shouldWarnAboutUnusedOverride = false;
        for (int i = 0; i < propEnemyOverrides.arraySize; ++i)
        {
            if(propEnemyOverrides.GetArrayElementAtIndex(i).FindPropertyRelative("position").intValue > propEnemyCount.intValue)
            {
                shouldWarnAboutUnusedOverride = true;
                break;
            }
        }
        if(shouldWarnAboutUnusedOverride)
        {
            EditorGUILayout.HelpBox("One or more enemy overrides will be unused since its position is more than the total number of enemies spawned.", MessageType.Warning);
        }
    }
}