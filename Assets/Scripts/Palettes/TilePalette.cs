using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileGeneration
{
    [CreateAssetMenu()]
    public class TilePalette : ScriptableObject
    {
        public List<RuleTile> ruleTiles = new List<RuleTile>();
    }
}