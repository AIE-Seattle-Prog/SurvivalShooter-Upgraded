#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor;

namespace TileGeneration
{
    [EditorTool("Paint Tool", typeof(TileGenerator))]
    public class TileGeneratorPaintTool : EditorTool
    {
        bool isHoldingMouseButton = false;
        bool mouseUp = false;

        TileGenerator tileGenerator;

        void BeforeSceneGUI(SceneView sceneView)
        {
            if (!ToolManager.IsActiveTool(this)) return;

            isHoldingMouseButton = false;
            mouseUp = false;

            if (tileGenerator != null && Event.current != null)
            {
                tileGenerator.mousePosition = Event.current.mousePosition;
                tileGenerator.mousePosition.y = Screen.height - tileGenerator.mousePosition.y - 50;
            }

            if (Selection.activeGameObject == null || !Selection.activeGameObject.TryGetComponent<TileGenerator>(out _)) return;

            if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                if (Event.current.button == 0)
                {
                    isHoldingMouseButton = true;
                }
                Event.current.Use();
            }

            if (Event.current.type == EventType.ScrollWheel)
            {
                int scrollDir = System.Math.Sign(Event.current.delta.y);
                if (scrollDir == 1)
                {
                    if (tileGenerator.paintMode != TileGenerator.PaintMode.ViewRules)
                    {
                        tileGenerator.PaintRadius--;
                    }
                    else
                    {
                        RuleTileVisualizer.hoverSlot?.PreviousCondition();
                    }
                }
                else if (scrollDir == -1)
                {
                    if (tileGenerator.paintMode != TileGenerator.PaintMode.ViewRules)
                    {
                        tileGenerator.PaintRadius++;
                    }
                    else
                    {
                        RuleTileVisualizer.hoverSlot?.NextCondition();
                    }
                }



                Event.current.Use();
            }

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                mouseUp = true;
                Event.current.Use();
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            if (!(window is SceneView)) return;

            if (!ToolManager.IsActiveTool(this)) return;

            if (Selection.activeGameObject == null || !Selection.activeGameObject.TryGetComponent<TileGenerator>(out _)) return;

            Camera currentCamera = Camera.current;
            if (currentCamera != null)
            {
                GUIContent labelContent = new GUIContent(tileGenerator.GetLabelDescription());
                GUIStyle labelStyle = new GUIStyle();
                labelStyle.alignment = TextAnchor.UpperCenter;
                labelStyle.fontSize = 18;
                Handles.Label(currentCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.975f, 1)), labelContent, labelStyle);
            }

            if (tileGenerator.GetSelectedPoint(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out Vector3 point, out Tile tile))
            {
                Handles.matrix = Matrix4x4.TRS(point,
                    tileGenerator.transform.rotation,
                    tileGenerator.transform.localScale * (tileGenerator.paintMode == TileGenerator.PaintMode.ViewRules ? 1 : tileGenerator.PaintRadius));

                if (RuleTileVisualizer.hoverSlot == null)
                {
                    if (tileGenerator.setSelectedTile || mouseUp)
                    {
                        tileGenerator.SelectedTile = tile;
                    }
                    else
                    {
                        // just add the tile to the extra tiles being edited
                        tileGenerator.tilesInRadius.Clear();
                        tileGenerator.tilesInRadius.Add(tile);
                    }
                }
                else
                {
                    tileGenerator.tilesInRadius.Clear();
                }

                Handles.DrawWireDisc(Vector3.zero, Vector3.up, 1);
            }
            else
            {
                if (tileGenerator.setSelectedTile || mouseUp)
                {
                    tileGenerator.SelectedTile = null;
                }

                Handles.DrawWireDisc(GetCurrentMousePositionInScene(), Vector3.up, 0.5f);
            }

            Handles.matrix = Matrix4x4.identity;

            TileGenerator.PaintMode paintMode = tileGenerator.paintMode;

            // Determine whether we should switch the tile's status of whether it is ignoring the rule or not
            Event currentEvent = Event.current;
            do
            {
                if (currentEvent.shift && tileGenerator.paintMode != TileGenerator.PaintMode.ViewRules)
                {
                    tileGenerator.paintMode = TileGenerator.PaintMode.ToggleRuleUsage;
                    break;
                }
            }
            while (Event.PopEvent(currentEvent));

            if (isHoldingMouseButton)
            {
                tileGenerator.ChangeTile();
            }

            if (mouseUp)
            {
                tileGenerator.tilesBeingEdited.Clear();
            }

            tileGenerator.paintMode = paintMode;

            base.OnToolGUI(window);

            window.Repaint();
        }

        public override void OnActivated()
        {
            base.OnActivated();
            if (tileGenerator == null)
            {
                tileGenerator = (TileGenerator)target;
            }

            RuleTileVisualizer.checkForHover = true;

            SceneView.beforeSceneGui += BeforeSceneGUI;
        }

        public override void OnWillBeDeactivated()
        {
            base.OnWillBeDeactivated();

            if (tileGenerator.paintMode != TileGenerator.PaintMode.ViewRules)
            {
                tileGenerator.selectedTileIndex.x = -1;
                tileGenerator.selectedTileIndex.z = -1;
            }
            tileGenerator.tilesInRadius.Clear();

            RuleTileVisualizer.checkForHover = false;

            SceneView.beforeSceneGui -= BeforeSceneGUI;
        }

        Vector3 GetCurrentMousePositionInScene()
        {
            Vector3 mousePosition = Event.current.mousePosition;
            var placeObject = HandleUtility.PlaceObject(mousePosition, out var newPosition, out var normal);

            return placeObject ? newPosition : HandleUtility.GUIPointToWorldRay(mousePosition).GetPoint(10);
        }
    }
}
#endif