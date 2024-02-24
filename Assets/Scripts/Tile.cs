using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TileGeneration
{
    [System.Serializable]
    public class Tile
    {
        public TileGenerator parent;

        public Vector3Int indexPosition;

        [SerializeField] RuleTile rule;
        public RuleTile Rule
        {
            get
            {
                return rule;
            }
            set
            {
                if (rule != value)
                {
                    if (value != null)
                    {
                        Prefab = value.GetObject(this);
                    }
                    else
                    {
                        Prefab = null;
                    }
                }

                rule = value;
            }
        }

        [SerializeField] bool ignoreRule;
        public bool IgnoreRule
        {
            get
            {
                return ignoreRule;
            }
            set
            {
                if (ignoreRule != value && rule != null)
                {
                    if (value)
                    {
                        Prefab = rule.defaultGameObject;
                    }
                    else
                    {
                        Prefab = rule.GetObject(this);
                    }
                }

                ignoreRule = value;
            }
        }

        [SerializeField] GameObject prefab;
        [SerializeField] MeshFilter[] meshFilters;

        // Prefab for this tile (we will be comparing this when deciding if we need to draw a new one or not)
        public GameObject Prefab
        {
            get
            {
                return prefab;
            }
            private set
            {
                if (prefab != value)
                {
                    if (value != null)
                    {
                        meshFilters = value.GetComponentsInChildren<MeshFilter>();
                    }
                    else
                    {
                        meshFilters = null;
                    }
                }

                prefab = value;
            }
        }

        // Game Object that is occupying this tile slot (if any)
        public GameObject obj;
        public Dictionary<Vector3Int, Tile> adjacentTiles = new Dictionary<Vector3Int, Tile>();

        public void UpdatePrefab()
        {
            if (rule != null)
            {
                Prefab = rule.GetObject(this);
            }
            else
            {
                Prefab = null;
            }
        }

        public void SetAdjacentTile(Vector3Int directionIndex, Tile newTile)
        {
            if (adjacentTiles == null)
            {
                adjacentTiles = new Dictionary<Vector3Int, Tile>();
            }

            if (adjacentTiles.ContainsKey(directionIndex))
            {
                adjacentTiles[directionIndex] = newTile;
            }
            else
            {
                adjacentTiles.Add(directionIndex, newTile);
            }
        }

        public Tile GetAdjacentTile(Vector3Int directionIndex)
        {
            if (adjacentTiles == null)
            {
                adjacentTiles = new Dictionary<Vector3Int, Tile>();
            }

            if (adjacentTiles.ContainsKey(directionIndex))
            {
                return adjacentTiles[directionIndex];
            }
            return null;
        }

#if UNITY_EDITOR
        public void SpawnObject()
        {
            if (Rule == null || Prefab == null || obj != null) return;

            obj = (GameObject)PrefabUtility.InstantiatePrefab(Prefab);
            SetTransform();
        }

        public void DestroyObject()
        {
            if (obj != null)
            {
                Object.DestroyImmediate(obj);
                obj = null;
            }
        }

        public void FixObject()
        {
            if (Rule == null || Prefab == null || obj == null)
            {
                if (obj != null)
                {
                    EditorApplication.delayCall += () => Object.DestroyImmediate(obj);
                    obj = null;
                }

                return;
            }

            GameObject rulePrefab = ignoreRule ? Rule.defaultGameObject : Rule.GetObject(this);
            if (rulePrefab != null && rulePrefab != PrefabUtility.GetCorrespondingObjectFromSource(obj))
            {
                // Destroy obj and re-instantiate the new prefab
                GameObject objToDestroy = obj;
                if (objToDestroy != null)
                {
                    EditorApplication.delayCall += () => Object.DestroyImmediate(objToDestroy);
                }

                obj = (GameObject)PrefabUtility.InstantiatePrefab(rulePrefab);
                obj.transform.parent = parent.transform;
                obj.transform.position = GetTargetPosition();
                obj.transform.localRotation = rulePrefab.transform.localRotation;
                SetTransform();
            }
        }
#endif

        public void DrawMesh()
        {
            if (meshFilters != null)
            {
                foreach (var filter in meshFilters)
                {
                    Mesh mesh = filter.sharedMesh;

                    Gizmos.matrix = Matrix4x4.TRS(GetTargetPosition(), GetTargetRotation(), GetTargetScale());

                    Gizmos.DrawWireMesh(mesh);
                }
                Gizmos.matrix = Matrix4x4.identity;
            }
        }

        public Vector3 GetTargetLocalPosition(bool useOffset = true)
        {
            if (useOffset && Rule != null)
            {
                if (meshFilters == null)
                {
                    meshFilters = prefab.GetComponentsInChildren<MeshFilter>();
                }

                Rule.GetOffsets(this, out Vector3 positionOffset, out _, out _);
                Rule.GetFixBounds(this, out bool fixBoundsPosition, out _);
                if (fixBoundsPosition)
                {
                    Bounds bounds = new Bounds();

                    foreach (var filter in meshFilters)
                    {
                        bounds.Encapsulate(filter.sharedMesh.bounds);
                    }

                    return parent.GetGridScalePoint(indexPosition) + positionOffset - bounds.center;
                }

                return parent.GetGridScalePoint(indexPosition) + positionOffset;
            }
            return parent.GetGridScalePoint(indexPosition);
        }

        public Vector3 GetTargetPosition(bool useOffset = true)
        {
            return parent.transform.TransformPoint(GetTargetLocalPosition(useOffset));
        }

        public Quaternion GetTargetLocalRotation(bool useOffset = true)
        {
            if (useOffset && Rule != null)
            {
                Rule.GetOffsets(this, out _, out Quaternion rotationOffset, out _);
                return (prefab == null ? Quaternion.identity : prefab.transform.rotation) * rotationOffset;
            }
            return prefab == null ? Quaternion.identity : prefab.transform.rotation;
        }
        public Quaternion GetTargetRotation(bool useOffset = true)
        {
            return parent.transform.rotation * GetTargetLocalRotation(useOffset);
        }

        public Vector3 GetTargetLocalScale(bool useOffset = true)
        {
            Vector3 returnValue = prefab == null ? Vector3.one : prefab.transform.localScale;
            if (useOffset && Rule != null)
            {
                Rule.GetOffsets(this, out _, out _, out Vector3 scaleMultiplier);
                Rule.GetFixBounds(this, out _, out bool fixBoundsScale);

                returnValue = Vector3.Scale(returnValue, scaleMultiplier);

                if (fixBoundsScale)
                {
                    Bounds bounds = new Bounds();

                    foreach (var filter in meshFilters)
                    {
                        bounds.Encapsulate(filter.sharedMesh.bounds);
                    }

                    float divisor = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

                    returnValue = new Vector3(returnValue.x / divisor, returnValue.y / divisor, returnValue.z / divisor);
                }
            }

            return returnValue;
        }

        public Vector3 GetTargetScale(bool useOffset = true)
        {
            return Vector3.Scale(parent.transform.localScale, GetTargetLocalScale(useOffset));
        }

        public void SetTransform()
        {
            if (obj == null || Prefab == null || Rule == null) return;

            obj.transform.parent = parent.transform;
            obj.transform.localPosition = GetTargetLocalPosition();
            obj.transform.localRotation = GetTargetLocalRotation();
            obj.transform.localScale = GetTargetLocalScale();
        }

        public Tile(TileGenerator parent, Vector3Int indexPosition)
        {
            this.parent = parent;
            this.indexPosition = indexPosition;
        }
    }
}