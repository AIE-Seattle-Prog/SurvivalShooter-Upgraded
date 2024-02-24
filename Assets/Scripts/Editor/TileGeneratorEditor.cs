#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace TileGeneration
{
    [CustomEditor(typeof(TileGenerator))]
    public class TileGeneratorEditor : Editor
    {
        TileGenerator tileGenerator;

        public override void OnInspectorGUI()
        {
            if (tileGenerator == null)
            {
                tileGenerator = (TileGenerator)target;
            }

            Undo.RecordObject(tileGenerator, "Tile Generator Change");

            Color oldColor = GUI.backgroundColor;

            EditorGUI.BeginChangeCheck();

            SerializedProperty tileCount = serializedObject.FindProperty("tileCount");
            EditorGUILayout.PropertyField(tileCount);

            SerializedProperty selectedTileIndexProperty = serializedObject.FindProperty("selectedTileIndex");
            Vector3Int selectedTileIndex = selectedTileIndexProperty.vector3IntValue;
            selectedTileIndex.y = EditorGUILayout.IntField("Y Level", selectedTileIndex.y);
            selectedTileIndex.y = Mathf.Clamp(selectedTileIndex.y, 0, tileGenerator.TileCount.y - 1);

            selectedTileIndexProperty.vector3IntValue = selectedTileIndex;

            SerializedProperty showAllYLevels = serializedObject.FindProperty("showAllYLevels");
            EditorGUILayout.PropertyField(showAllYLevels);

            SerializedProperty debugRuleUsage = serializedObject.FindProperty("debugRuleUsage");
            EditorGUILayout.PropertyField(debugRuleUsage);

            SerializedProperty showMeshes = serializedObject.FindProperty("showMeshes");
            EditorGUILayout.PropertyField(showMeshes);

            SerializedProperty gridPivotPoint = serializedObject.FindProperty("gridPivotPoint");

            Vector3 gridPivotValue = gridPivotPoint.vector3Value;
            gridPivotValue.x = Mathf.Clamp01(gridPivotValue.x);
            gridPivotValue.y = Mathf.Clamp01(gridPivotValue.y);
            gridPivotValue.z = Mathf.Clamp01(gridPivotValue.z);

            gridPivotPoint.vector3Value = gridPivotValue;

            EditorGUILayout.PropertyField(gridPivotPoint);

            serializedObject.ApplyModifiedProperties();

            Rect currentRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(25);
            currentRect.y += 25;

            foreach (TilePalette palette in tileGenerator.palettes)
            {
                if (palette != null)
                {
                    GUI.backgroundColor = tileGenerator.selectedPalette == palette ? Color.blue : oldColor;

                    if (GUI.Button(currentRect, palette.name))
                    {
                        tileGenerator.selectedPalette = tileGenerator.selectedPalette == palette ? null : palette;
                    }
                    GUILayout.Space(25);
                    currentRect.y += 25;
                }
            }

            GUILayout.Space(25);
            currentRect.y += 25;

            if (tileGenerator.selectedPalette != null)
            {
                // Display selected palette's prefabs
                foreach (var ruleTile in tileGenerator.selectedPalette.ruleTiles)
                {
                    if (ruleTile == null) continue;

                    GameObject targetObj = ruleTile.GetObject(tileGenerator.SelectedTile);
                    if (targetObj != null)
                    {
                        GUI.backgroundColor = tileGenerator.selectedRule == ruleTile ? Color.green : oldColor;

                        GUIContent asset = new GUIContent(AssetPreview.GetAssetPreview(ruleTile.defaultGameObject),
                            "This is the " + ruleTile.defaultGameObject.name + " prefab");

                        if (GUI.Button(new Rect(currentRect.position, Vector2.one * 100), asset))
                        {
                            tileGenerator.selectedRule = ruleTile;
                        }
                    }
                    GUILayout.Space(100);
                    currentRect.y += 100;
                }
            }
            GUILayout.Space(25);
            currentRect.y += 25;

            GUI.backgroundColor = tileGenerator.paintMode == TileGenerator.PaintMode.Draw ? Color.red : oldColor;

            // Pencil icon
            GUIContent draw = new GUIContent(TextureLibrary.GetTexture("Pencil"));
            if (GUI.Button(new Rect(currentRect.x, currentRect.y, 100, 100), draw))
            {
                tileGenerator.paintMode = TileGenerator.PaintMode.Draw;
            }

            GUI.backgroundColor = tileGenerator.paintMode == TileGenerator.PaintMode.Fill ? Color.red : oldColor;

            // Paint bucket icon
            GUIContent fill = new GUIContent(TextureLibrary.GetTexture("Bucket"));
            if (GUI.Button(new Rect(currentRect.x + 100, currentRect.y, 100, 100), fill))
            {
                tileGenerator.paintMode = TileGenerator.PaintMode.Fill;
            }

            GUI.backgroundColor = tileGenerator.paintMode == TileGenerator.PaintMode.ViewRules ? Color.red : oldColor;

            // Button to view rules
            if (GUI.Button(new Rect(currentRect.x + 200, currentRect.y, 100, 100), "View Rules"))
            {
                tileGenerator.paintMode = TileGenerator.PaintMode.ViewRules;
            }

            GUI.backgroundColor = !tileGenerator.shouldPaint ? Color.red : oldColor;

            GUILayout.Space(100);
            currentRect.y += 100;

            // Eraser icon
            GUIContent erase = new GUIContent(TextureLibrary.GetTexture("Eraser"));
            if (GUI.Button(new Rect(currentRect.position, Vector2.one * 100), erase))
            {
                tileGenerator.shouldPaint = !tileGenerator.shouldPaint;
            }

            GUI.backgroundColor = oldColor;

            GUILayout.Space(100);
            currentRect.y += 100;

            if (GUI.Button(currentRect, "Generate Tiles"))
            {
                tileGenerator.GenerateTiles();
            }

            GUILayout.Space(25);
            currentRect.y += 25;

            if (GUI.Button(currentRect, "Clear Tiles"))
            {
                tileGenerator.ClearTiles();
            }

            GUILayout.Space(25);
            currentRect.y += 25;

            if (GUI.Button(currentRect, "Add/Remove Palettes"))
            {
                AddTilePaletteWindow window = (AddTilePaletteWindow)EditorWindow.GetWindow(typeof(AddTilePaletteWindow), false, "Add Palette");
                window.tileGenerator = tileGenerator;
            }

            GUILayout.Space(25);
            currentRect.y += 25;

            if (GUI.Button(currentRect, new GUIContent("Clear Unused Tiles", "Deletes tiles that are out of the current grid bounds")))
            {
                tileGenerator.ClearUnusedTiles();
            }

            GUILayout.Space(25);
            currentRect.y += 25;

            if (GUI.Button(currentRect, new GUIContent("Clear Unused Objects", "Deletes children objects that aren't being tracked by a tile")))
            {
                tileGenerator.ClearUnusedObjects();
            }

            GUILayout.Space(25);
            currentRect.y += 25;

            if (GUI.Button(currentRect, new GUIContent("Reset Tiles", "Deletes the data in all tiles")))
            {
                tileGenerator.ResetTiles();
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(tileGenerator);
            }
        }
    }
}
#endif