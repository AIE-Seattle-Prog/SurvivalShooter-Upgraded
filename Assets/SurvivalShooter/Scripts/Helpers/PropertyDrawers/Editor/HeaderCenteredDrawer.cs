using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HeaderCenteredAttribute))]
public class HeaderCenteredDrawer : DecoratorDrawer
{
    // the actual drawing logic (that appears in the inspector)
    public override void OnGUI(Rect position)
    {
        position.yMin += EditorGUIUtility.singleLineHeight * 0.5f;
        position = EditorGUI.IndentedRect(position);
        GUI.Label(position, (attribute as HeaderCenteredAttribute).data, EditorStyles.centeredGreyMiniLabel);
    }

    // provide a quick measurement for how much vertical space is required
    public override float GetHeight()
    {
        return EditorGUIUtility.singleLineHeight * 1.5f;
    }
}
