using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(EnemyRoundConfig.EnemyChance))]
public class EnemyChanceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Set label position for EnemyChance
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        var editorIndent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate the amount of space taken up by each object
        Rect prefabRect = new Rect(position.x, position.y, position.width * 0.60f, position.height);
        Rect weightRect = new Rect(position.x + position.width * 0.65f, position.y, position.width * 0.35f, position.height);

        // Draw the properties
        EditorGUI.PropertyField(prefabRect, property.FindPropertyRelative("enemyPrefab"), GUIContent.none);
        EditorGUIUtility.labelWidth = 44;
        EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("weight"));
        EditorGUIUtility.labelWidth = 0;

        EditorGUI.indentLevel = editorIndent;

        EditorGUI.EndProperty();
    }
}