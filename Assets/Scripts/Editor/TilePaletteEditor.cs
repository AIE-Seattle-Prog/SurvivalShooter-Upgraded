#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TileGeneration
{
    [CustomEditor(typeof(TilePalette))]
    public class TilePaletteEditor : Editor
    {
        TilePalette palette;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (palette == null)
            {
                palette = (TilePalette)target;
            }

            EditorGUI.BeginChangeCheck();

            // Make sure the objects field below are interactable
            // Hopefully there is a better way to do this,
            // but right now this is okay
            GUILayout.Space(500);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(palette);
            }
        }
    }
}
#endif