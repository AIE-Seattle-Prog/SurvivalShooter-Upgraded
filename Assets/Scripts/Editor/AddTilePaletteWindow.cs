#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace TileGeneration
{
    public class AddTilePaletteWindow : EditorWindow
    {
        public TileGenerator tileGenerator;
        [SerializeField] TilePalette targetPalette;

        void OnGUI()
        {
            targetPalette = (TilePalette)EditorGUILayout.ObjectField("Target Palette", targetPalette, typeof(TilePalette), false);

            if (targetPalette != null)
            {
                if (GUILayout.Button("Add Palette") && !tileGenerator.palettes.Contains(targetPalette))
                {
                    tileGenerator.palettes.Add(targetPalette);
                }
                if (GUILayout.Button("Remove Palette") && tileGenerator.palettes.Contains(targetPalette))
                {
                    tileGenerator.palettes.Remove(targetPalette);
                }
            }
        }

    }
}
#endif