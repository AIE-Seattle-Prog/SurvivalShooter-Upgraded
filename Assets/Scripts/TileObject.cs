using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TileGeneration
{
    [ExecuteInEditMode]
    public class TileObject : MonoBehaviour
    {
        public Tile tile;

        void Update()
        {
            if (tile != null && tile.obj != gameObject)
            {
#if UNITY_EDITOR
                EditorApplication.delayCall += () => DestroyImmediate(gameObject);
#endif
            }
        }

        void OnValidate()
        {
            if (tile != null && tile.obj != gameObject)
            {
#if UNITY_EDITOR
                EditorApplication.delayCall += () => DestroyImmediate(gameObject);
#endif
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}