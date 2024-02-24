using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection.Emit;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileGeneration
{
    [ExecuteInEditMode]
    public class TileGenerator : MonoBehaviour, ISerializationCallbackReceiver
    {
        public static readonly Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.right,
            Vector3Int.up,
            Vector3Int.forward,
            Vector3Int.left,
            Vector3Int.down,
            Vector3Int.back,
            new Vector3Int(1, 1, 0),
            new Vector3Int(1, -1, 0),
            new Vector3Int(1, 0, 1),
            new Vector3Int(1, 0, -1),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(-1, -1, 0),
            new Vector3Int(-1, 0, 1),
            new Vector3Int(-1, 0, -1),
            new Vector3Int(0, 1, 1),
            new Vector3Int(0, 1, -1),
            new Vector3Int(0, -1, 1),
            new Vector3Int(0, -1, -1),
        };

        [SerializeField] Vector3Int tileCount = Vector3Int.one;

        public Vector3Int TileCount => tileCount;

        [SerializeField] List<Vector3Int> tileKeys = new List<Vector3Int>();
        [SerializeField] List<Tile> tileValues = new List<Tile>();
        public bool setSelectedTile = true;

        Dictionary<Vector3Int, Tile> tiles = new Dictionary<Vector3Int, Tile>();

        public List<Tile> tilesInRadius = new List<Tile>();

        public List<TilePalette> palettes = new List<TilePalette>();

        [SerializeReference] public TilePalette selectedPalette;
        [SerializeReference] public RuleTile selectedRule;
        public Vector3Int selectedTileIndex = -Vector3Int.one;

        [SerializeField] Vector3 gridPivotPoint = new Vector3(0.5f, 0f, 0.5f);

        [Tooltip("Show what rule number is being used on each tile? (if any)")]
        [SerializeField] bool debugRuleUsage = true;

        [Tooltip("Show meshes that will be spawned on each tile? (if any)")]
        [SerializeField] bool showMeshes = true;

        public enum PaintMode
        {
            Draw,
            Fill,
            ViewRules,
            ToggleRuleUsage,
        }

        [SerializeField] int paintRadius = 5;

        public int PaintRadius
        {
            get
            {
                return paintRadius;
            }
            set
            {
                paintRadius = Mathf.Clamp(value, 1, 10);
            }
        }

        public PaintMode paintMode = PaintMode.Draw;

        public bool shouldPaint = true;
        [SerializeField] bool showAllYLevels = true;

        public Vector2 mousePosition;

        public Tile SelectedTile
        {
            get
            {
                if (tiles.TryGetValue(selectedTileIndex, out Tile tile))
                {
                    if (selectedTileIndex.x < 0 || selectedTileIndex.y < 0 || selectedTileIndex.z < 0)
                    {
                        tiles.Remove(selectedTileIndex);
                        return null;
                    }

                    return tile;
                }
                return null;
            }
            set
            {
                selectedTileIndex = value == null ? new Vector3Int(-1, selectedTileIndex.y, -1) : value.indexPosition;
            }
        }

        int debugCount;
        Queue<Tile> pendingTilesInRadius;

        // When the user holds down the mouse button, all tiles that they select will
        // be added to this list, and when they release it, this list will reset
        public List<Tile> tilesBeingEdited = new List<Tile>();

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                foreach (var tile in tiles.Values)
                {
                    tile.parent = this;
                    if (tile.obj != null)
                    {
                        tile.obj.SetActive(true);
                    }
                }
            }
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                enabled = false;
            }
            else
            {
                OnValidate();
            }
        }

        void OnValidate()
        {
            // Grid count can never go below 1
            tileCount.x = System.Math.Max(tileCount.x, 1);
            tileCount.y = System.Math.Max(tileCount.y, 1);
            tileCount.z = System.Math.Max(tileCount.z, 1);

            for (int x = 0; x < tileCount.x; x++)
            {
                for (int y = 0; y < tileCount.y; y++)
                {
                    for (int z = 0; z < tileCount.z; z++)
                    {
                        ValidateTile(x, y, z);
                    }
                }
            }
        }

        public void ClearUnusedTiles()
        {
            // Remove unused tiles
            List<Vector3Int> keysToRemove = new List<Vector3Int>();

            foreach (var tileKey in tiles.Keys)
            {
                if (tileKey.x >= tileCount.x ||
                    tileKey.y >= tileCount.y ||
                    tileKey.z >= tileCount.z)
                {
                    keysToRemove.Add(tileKey);
                }
            }

            foreach (var key in keysToRemove)
            {
                Tile tile = tiles[key];
                if (tile.obj != null)
                {
#if UNITY_EDITOR
                    EditorApplication.delayCall += () => DestroyImmediate(tile.obj);
#endif
                }

                tiles.Remove(key);
            }
        }

        public void ClearUnusedObjects()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                bool trackedByTile = false;
                foreach (var tile in tiles.Values)
                {
                    if (tile.obj == child.gameObject)
                    {
                        trackedByTile = true;
                        break;
                    }
                }
                
                if (!trackedByTile)
                {
                    DestroyImmediate(child);
                }
            }
        }

        void ValidateTile(Vector3Int vectorIndex)
        {
            if (!tiles.TryGetValue(vectorIndex, out Tile tile))
            {
                tile = new Tile(this, vectorIndex);
                tiles.Add(vectorIndex, tile);
            }

            tile.UpdatePrefab();
            tile.parent = this;
            tile.indexPosition = vectorIndex;

            if (tile.obj != null)
            {
                tile.SetTransform();

                if (showAllYLevels)
                {
                    tile.obj.SetActive(true);
                }
                else
                {
                    tile.obj.SetActive(vectorIndex.y == selectedTileIndex.y);
                }
            }
            ManageAdjacentTiles(tile);
        }

        void ValidateTile(int x, int y, int z)
        {
            ValidateTile(new Vector3Int(x, y, z));
        }

        void ManageAdjacentTiles(Tile targetTile)
        {
            ManageSingleAdjacentTile(targetTile, Vector3Int.right);
            ManageSingleAdjacentTile(targetTile, Vector3Int.left);
            ManageSingleAdjacentTile(targetTile, Vector3Int.up);
            ManageSingleAdjacentTile(targetTile, Vector3Int.down);
            ManageSingleAdjacentTile(targetTile, Vector3Int.forward);
            ManageSingleAdjacentTile(targetTile, Vector3Int.back);

            ManageSingleAdjacentTile(targetTile, new Vector3Int(1, 1, 0));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(1, -1, 0));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(1, 0, 1));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(1, 0, -1));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(-1, 1, 0));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(-1, -1, 0));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(-1, 0, 1));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(-1, 0, -1));

            ManageSingleAdjacentTile(targetTile, new Vector3Int(0, 1, 1));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(0, 1, -1));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(0, -1, 1));
            ManageSingleAdjacentTile(targetTile, new Vector3Int(0, -1, -1));
        }

        void ManageSingleAdjacentTile(Tile targetTile, Vector3Int direction)
        {
            int x = targetTile.indexPosition.x;
            int y = targetTile.indexPosition.y;
            int z = targetTile.indexPosition.z;

            Vector3Int adjacentVectorIndex = new Vector3Int(x + direction.x, y + direction.y, z + direction.z);
            if (tiles.ContainsKey(adjacentVectorIndex))
            {
                targetTile.SetAdjacentTile(direction, tiles[adjacentVectorIndex]);
            }
        }

#if UNITY_EDITOR
        public void ResetTiles()
        {
            tiles.Clear();
            while (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }
            }
            EditorUtility.SetDirty(this);
        }

        void OnDrawGizmos()
        {
            if (debugRuleUsage)
            {
                // Display the status of each rule next to each tile
                foreach (var tile in tiles.Values)
                {
                    GUIStyle style = new GUIStyle();
                    style.alignment = TextAnchor.MiddleCenter;

                    Vector3 tileRelativeToCameraPos = Camera.current.transform.InverseTransformPoint(tile.GetTargetPosition());

                    style.fontSize = (int)(32f / tileRelativeToCameraPos.z);

                    // Drawing text too far away from the camera won't look good
                    if (style.fontSize == 0) return;

                    if (tile.IgnoreRule)
                    {
                        // No rules are to be used for this tile
                        Handles.Label(tile.GetTargetPosition(), "Using default object", style);
                    }
                    else if (tile.Rule != null)
                    {
                        int ruleCount = tile.Rule.rules.Count;

                        for (int i = 0; i < ruleCount; i++)
                        {
                            var rule = tile.Rule.rules[i];

                            float yOffset = (i - (ruleCount * 0.5f)) * 10f * style.fontSize / ruleCount;
                            style.contentOffset = Vector2.up * yOffset;

                            if (rule.Evaluate(tile))
                            {
                                style.normal.textColor = Color.green;
                                Handles.Label(tile.GetTargetPosition(), "Rule " + (i + 1).ToString() + " was successful", style);
                            }
                            else
                            {
                                style.normal.textColor = Color.red;
                                Handles.Label(tile.GetTargetPosition(), "Rule " + (i + 1).ToString() + " was unsuccessful", style);
                            }
                        }
                    }
                }
            }

            ValidateTile(selectedTileIndex);

            switch (paintMode)
            {
                case PaintMode.Draw:
                    tilesInRadius = GetTilesInRadius(SelectedTile);
                    break;
                case PaintMode.Fill:
                    tilesInRadius = GetTilesToFill(SelectedTile);
                    break;
                default:
                    break;
            }

            Gizmos.color = Color.gray;
            for (int x = 0; x < tileCount.x; x++)
            {
                for (int z = 0; z < tileCount.z; z++)
                {
                    if (showAllYLevels)
                    {
                        for (int y = 0; y < tileCount.y; y++)
                        {
                            DrawTile(x, y, z);
                        }
                    }
                    else
                    {
                        DrawTile(x, selectedTileIndex.y, z);
                    }
                }
            }

            Gizmos.matrix = Matrix4x4.identity;

            Tile selectedTile = SelectedTile;
            if (paintMode == PaintMode.ViewRules)
            {
                if (selectedTile != null)
                {
                    RuleTileVisualizer.Display(mousePosition, selectedTile);
                }
            }
            else
            {
                setSelectedTile = true;
            }
        }

        void DrawTile(int x, int y, int z)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(GetGridScalePoint(x, y, z)), transform.rotation, transform.localScale);

            Vector3Int indexPosition = new Vector3Int(x, y, z);

            tiles.TryAdd(indexPosition, new Tile(this, indexPosition));

            Tile tile = tiles[indexPosition];

            if (SelectedTile == tile || tilesInRadius.Contains(tile) || tilesBeingEdited.Contains(tile))
            {
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
            }
            else
            {
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
            if (showMeshes)
            {
                tile.DrawMesh();
            }
        }
#endif

        public Vector3 GetGridScalePoint(Vector3Int index)
        {
            return GetGridScalePoint(index.x, index.y, index.z);
        }

        public Vector3 GetGridScalePoint(float x, float y, float z)
        {
            float xPoint = x + (0.5f - gridPivotPoint.x);
            float yPoint = y + (0.5f - gridPivotPoint.y);
            float zPoint = z + (0.5f - gridPivotPoint.z);

            return new Vector3(xPoint, yPoint, zPoint) - Vector3.Scale(tileCount - Vector3.one, gridPivotPoint);
        }

        public bool GetSelectedPoint(Ray ray, out Vector3 point)
        {
            point = Vector3.zero;

            Plane hPlane = new Plane(transform.up, transform.TransformPoint(GetGridScalePoint(0, selectedTileIndex.y, 0)));
            hPlane.distance -= transform.position.y;

            if (hPlane.Raycast(ray, out float distance))
            {
                point = ray.GetPoint(distance);

                Vector3 selectedPoint = transform.InverseTransformPoint(ray.GetPoint(distance));

                // Trying to convert xPoint, yPoint, and zPoint into what the indexes will be (excluding decimals)
                float xPoint = selectedPoint.x + tileCount.x * 0.5f;
                float yPoint = selectedPoint.y + tileCount.y * 0.5f;
                float zPoint = selectedPoint.z + tileCount.z * 0.5f;

                int xIndex = (int)xPoint;
                int yIndex = (int)yPoint;
                int zIndex = (int)zPoint;

                if (TileInRange(xIndex, yIndex, zIndex))
                {
                    if (setSelectedTile)
                    {
                        selectedTileIndex.x = xIndex;
                        selectedTileIndex.z = zIndex;
                    }
                    return true;
                }
            }
            if (setSelectedTile)
            {
                selectedTileIndex.x = -1;
                selectedTileIndex.z = -1;
            }
            return false;
        }

        public bool GetSelectedPoint(Ray ray, out Vector3 point, out Tile tile)
        {
            point = Vector3.zero;
            tile = null;

            Plane hPlane = new Plane(transform.up, transform.TransformPoint(GetGridScalePoint(0, selectedTileIndex.y, 0)));
            hPlane.distance -= transform.position.y;

            if (hPlane.Raycast(ray, out float distance))
            {
                point = ray.GetPoint(distance);

                Vector3 selectedPoint = transform.InverseTransformPoint(ray.GetPoint(distance));

                // Trying to convert xPoint and zPoint into what the indexes will be (excluding decimals)
                float xPoint = selectedPoint.x + tileCount.x * 0.5f;
                float zPoint = selectedPoint.z + tileCount.z * 0.5f;

                int xIndex = (int)xPoint;
                int yIndex = selectedTileIndex.y;
                int zIndex = (int)zPoint;

                if (TileInRange(xIndex, yIndex, zIndex))
                {
                    tile = tiles[new Vector3Int(xIndex, selectedTileIndex.y, zIndex)];
                    return true;
                }
            }
            return false;
        }

#if UNITY_EDITOR
        public void ChangeTile()
        {
            Tile selectedTile = SelectedTile;

            if (selectedTile == null) return;
            if (tilesBeingEdited.Contains(selectedTile)) return;

            Undo.RecordObject(this, "Tile Generator Change");

            switch (paintMode)
            {
                case PaintMode.ViewRules:
                    if (RuleTileVisualizer.hoverSlot == null)
                    {
                        setSelectedTile = false;
                    }
                    break;
                case PaintMode.ToggleRuleUsage:
                    selectedTile.IgnoreRule = !selectedTile.IgnoreRule;
                    foreach (var tile in tilesInRadius)
                    {
                        tile.IgnoreRule = !tile.IgnoreRule;
                    }
                    break;
                default:
                    if (shouldPaint)
                    {
                        // Paint tile
                        PaintTile();
                    }
                    else
                    {
                        // Erase tile
                        EraseTile();
                    }
                    break;
            }

            tilesBeingEdited.Add(selectedTile);
        }

        void PaintTile()
        {
            // Cache selected tile
            Tile selectedTile = SelectedTile;

            if (selectedTile != null)
            {
                selectedTile.Rule = selectedRule;
            }

            foreach (var tile in tilesInRadius)
            {
                if (!tilesBeingEdited.Contains(tile))
                {
                    PaintTile(tile);
                    tilesBeingEdited.Add(tile);
                }
            }

            // Fix objects in a separate loop because
            // we only want to do that after all tiles in radius
            // have had their objects created
            foreach (var tileIndex in tiles.Keys)
            {
                if (TileInRange(tileIndex))
                {
                    ValidateTile(tileIndex);
                }
            }

            EditorUtility.SetDirty(this);
        }

        void PaintTile(Tile tile)
        {
            if (tile != null && selectedRule != tile.Rule)
            {
                tile.Rule = selectedRule;
            }
        }

        void EraseTile()
        {
            // Cache selected tile
            Tile selectedTile = SelectedTile;
            if (selectedTile != null)
            {
                selectedTile.Rule = null;
            }
            foreach (var tile in tilesInRadius)
            {
                if (!tilesBeingEdited.Contains(tile))
                {
                    EraseTile(tile);
                    tilesBeingEdited.Add(tile);
                }
            }
            EditorUtility.SetDirty(this);
        }

        void EraseTile(Tile tile)
        {
            if (tile != null)
            {
                tile.Rule = null;
            }
            EditorUtility.SetDirty(this);
        }

        public void GenerateTiles()
        {
            foreach (var tile in tiles.Values)
            {
                tile.SpawnObject();
            }
            foreach (var tile in tiles.Values)
            {
                tile.FixObject();
            }
        }
        public void ClearTiles()
        {
            foreach (var tile in tiles.Values)
            {
                tile.DestroyObject();
            }
        }
#endif

        public void OnBeforeSerialize()
        {
            tileKeys.Clear();
            tileValues.Clear();

            foreach (var kvp in tiles)
            {
                tileKeys.Add(kvp.Key);
                tileValues.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            tiles = new Dictionary<Vector3Int, Tile>();
            for (int i = 0; i < Mathf.Min(tileKeys.Count, tileValues.Count); i++)
            {
                tiles.Add(tileKeys[i], tileValues[i]);
            }
        }

        List<Tile> GetTilesInRadius(Tile startingTile)
        {
            debugCount = 0;
            pendingTilesInRadius = new Queue<Tile>();
            List<Tile> tilesInRadius = new List<Tile>();

            if (startingTile != null)
            {
                pendingTilesInRadius.Enqueue(startingTile);
                while (pendingTilesInRadius.Count > 0)
                {
                    GetTilesInRadiusRecursive(tilesInRadius, startingTile, pendingTilesInRadius.Dequeue());
                }
            }

            return tilesInRadius;
        }

        void GetTilesInRadiusRecursive(List<Tile> tilesInRadius, Tile startingTile, Tile currentTile)
        {
            if (startingTile != null && currentTile != null)
            {
                ManageAdjacentTiles(currentTile);
                foreach (var pair in currentTile.adjacentTiles)
                {
                    Vector3Int key = pair.Key;
                    Tile value = pair.Value;

                    if (Vector3.Distance(startingTile.indexPosition, value.indexPosition) < paintRadius &&
                        !tilesInRadius.Contains(value) && key.y == 0)
                    {
                        debugCount++;
                        tilesInRadius.Add(value);
                        pendingTilesInRadius.Enqueue(value);
                    }
                }
            }
        }

        List<Tile> GetTilesToFill(Tile startingTile)
        {
            debugCount = 0;
            pendingTilesInRadius = new Queue<Tile>();
            List<Tile> tilesToFill = new List<Tile>();

            if (startingTile != null)
            {
                pendingTilesInRadius.Enqueue(startingTile);
                while (pendingTilesInRadius.Count > 0)
                {
                    GetTilesToFillRecursive(startingTile.Rule, tilesToFill, pendingTilesInRadius.Dequeue());
                }
            }

            return tilesToFill;
        }

        void GetTilesToFillRecursive(RuleTile startingRule, List<Tile> tilesToFill, Tile currentTile)
        {
            if (currentTile != null)
            {
                ManageAdjacentTiles(currentTile);
                foreach (var pair in currentTile.adjacentTiles)
                {
                    Vector3Int key = pair.Key;
                    Tile value = pair.Value;

                    if (key.magnitude != 1 || key.y != 0) continue;

                    if (value.Rule == startingRule && !tilesToFill.Contains(value))
                    {
                        debugCount++;
                        tilesToFill.Add(value);
                        pendingTilesInRadius.Enqueue(value);
                    }
                }
            }
        }

        bool TileInRange(Tile tile)
        {
            if (tile == null) return false;

            return tile.indexPosition.x >= 0 && tile.indexPosition.x < tileCount.x &&
            tile.indexPosition.y >= 0 && tile.indexPosition.y < tileCount.y &&
            tile.indexPosition.z >= 0 && tile.indexPosition.z < tileCount.z;
        }

        bool TileInRange(Vector3Int indexPosition)
        {
            return indexPosition.x >= 0 && indexPosition.x < tileCount.x &&
            indexPosition.y >= 0 && indexPosition.y < tileCount.y &&
            indexPosition.z >= 0 && indexPosition.z < tileCount.z;
        }

        bool TileInRange(int x, int y, int z)
        {
            return x >= 0 && x < tileCount.x &&
            y >= 0 && y < tileCount.y &&
            z >= 0 && z < tileCount.z;
        }

        public string GetLabelDescription()
        {
            switch (paintMode)
            {
                case PaintMode.Draw:
                    break;
                case PaintMode.Fill:
                    return $"Click to fill a tile \n " +
                $"Shift click to toggle whether a tile will evaluate their rule or just use their default game object.";
                case PaintMode.ViewRules:
                    if (setSelectedTile)
                    {
                        return $"Click to select a tile.";
                    }
                    return $"Hover over the labeled squares and scroll to change their requirements.";
                case PaintMode.ToggleRuleUsage:
                    break;
                default:
                    return $"Click to {(shouldPaint ? "draw" : "erase")} a tile\n " +
                $"Shift click to toggle whether a tile will evaluate their rule or just use their default game object.";
            }

            return $"Click to {(shouldPaint ? "draw" : "erase")} a tile\n " +
                $"Shift click to toggle whether a tile will evaluate their rule or just use their default game object.";
        }
    }
}