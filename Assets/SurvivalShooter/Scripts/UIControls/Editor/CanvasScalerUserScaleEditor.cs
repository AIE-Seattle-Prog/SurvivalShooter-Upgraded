using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(CanvasScalerUserScale), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom Editor for the CanvasScalerUserScale component.
    ///   Extended from the custom editor for a component from CanvasScaler.
    /// </summary>
    public class CanvasScalerUserScaleUserScaleEditor : CanvasScalerEditor
    {
        SerializedProperty m_fUserScale;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_fUserScale = serializedObject.FindProperty("userScaleFactor");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(m_fUserScale);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
