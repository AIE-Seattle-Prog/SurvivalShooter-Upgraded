using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(EnemyRoundConfig.EnemyChance))]
public class EnemyChanceDrawer : PropertyDrawer
{

    // UI Toolkit (formerly known as UI Elements)
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        var container = new VisualElement();

        // Create property fields.
        var goField = new PropertyField(property.FindPropertyRelative("enemy"));
        var posField = new PropertyField(property.FindPropertyRelative("position"));

        // Add fields to the container.
        container.Add(goField);
        container.Add(posField);

        return container;
    }

    // Immediate Mode GUI (aka IMGUI)
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var enemyRect = new Rect(position.x, position.y, position.width * 0.60f, position.height);
        var weightRect = new Rect(position.x + position.width * 0.65f, position.y, position.width * 0.35f, position.height);

        // Draw fields - passing "none" omits the label
        EditorGUI.PropertyField(enemyRect, property.FindPropertyRelative("enemyPrefab"), GUIContent.none);
        // Set width for "Weight" label
        EditorGUIUtility.labelWidth = 44;
        EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("weight"));
        EditorGUIUtility.labelWidth = 0;

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }
}
