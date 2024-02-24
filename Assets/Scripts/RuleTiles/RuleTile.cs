using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TileGeneration
{
    [CreateAssetMenu()]
    public class RuleTile : ScriptableObject
    {
        public GameObject defaultGameObject = null;

        public GameObject GetObject(Tile tile)
        {
            if (tile != null && !tile.IgnoreRule)
            {
                foreach (var rule in rules)
                {
                    GameObject evaluatedObj = rule.Evaluate(tile);
                    if (evaluatedObj != null)
                    {
                        return evaluatedObj;
                    }
                }
            }
            return defaultGameObject;
        }

        public List<Rule> rules = new List<Rule>();

        public bool fixBoundsPosition = true;
        public bool fixBoundsScale = true;

        public Vector3 positionOffset = Vector3.zero;
        public Vector3 rotationOffset = Vector3.zero;
        public Vector3 scaleMultiplier = Vector3.one;

        public void GetFixBounds(Tile tile, out bool fixBoundsPosition, out bool fixBoundsScale)
        {
            Rule rule = GetRule(tile);
            if (rule != null)
            {
                fixBoundsPosition = rule.fixBoundsPosition;
                fixBoundsScale = rule.fixBoundsScale;
                return;
            }

            fixBoundsPosition = this.fixBoundsPosition;
            fixBoundsScale = this.fixBoundsScale;
        }

        public void GetOffsets(Tile tile, out Vector3 positionOffset, out Quaternion rotationOffset, out Vector3 scaleMultiplier)
        {
            positionOffset = this.positionOffset;
            rotationOffset = Quaternion.Euler(this.rotationOffset);
            scaleMultiplier = this.scaleMultiplier;

            if (!tile.IgnoreRule)
            {
                Rule rule = GetRule(tile);
                if (rule != null)
                {
                    positionOffset = rule.positionOffset;
                    rotationOffset = Quaternion.Euler(rule.rotationOffset);
                    scaleMultiplier = rule.scaleMultiplier;
                }
            }
        }

        Rule GetRule(Tile tile)
        {
            foreach (var rule in rules)
            {
                GameObject evaluatedObj = rule.Evaluate(tile);
                if (evaluatedObj != null)
                {
                    return rule;
                }
            }
            return null;
        }

        Rule GetRule(Tile tile, out GameObject evaluatedObj)
        {
            evaluatedObj = null;
            foreach (var rule in rules)
            {
                evaluatedObj = rule.Evaluate(tile);
                if (evaluatedObj != null)
                {
                    return rule;
                }
            }
            return null;
        }

        void OnValidate()
        {
            foreach (var rule in rules)
            {
                for (int i = 0; i < rule.slots.Length; i++)
                {
                    int copyI = i;

                    if (i >= 13)
                    {
                        copyI++;
                    }

                    int x = copyI / 9;
                    int y = copyI % 9 / 3;
                    int z = copyI % 9 % 3;

                    rule.slots[i].direction = new Vector3Int(x - 1, y - 1, z - 1);
                }
            }
        }

        [System.Serializable]
        public class Rule
        {
            [System.Serializable]
            public enum Condition
            {
                None,
                ExistingTile,
                NoTile,
            }

            public Rule()
            {
                slots = new Slot[26];

                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i] = new Slot();
                    int copyI = i;

                    if (i >= 13)
                    {
                        copyI++;
                    }

                    int x = copyI / 9;
                    int y = copyI % 9 / 3;
                    int z = copyI % 9 % 3;

                    slots[i].direction = new Vector3Int(x - 1, y - 1, z - 1);
                }
            }

            public Rule(Rule existingRule)
            {
                slots = new Slot[26];

                for (int i = 0; i < slots.Length; i++)
                {
                    slots[i] = new Slot(existingRule.slots[i]);
                }

                open = existingRule.open;

                newObj = existingRule.newObj;
                fixBoundsPosition = existingRule.fixBoundsPosition;
                fixBoundsScale = existingRule.fixBoundsScale;

                positionOffset = existingRule.positionOffset;
                rotationOffset = existingRule.rotationOffset;
                scaleMultiplier = existingRule.scaleMultiplier;
            }

            public Slot[] slots = new Slot[26];
            public GameObject newObj;

            public bool open = false;

            public bool fixBoundsPosition = true;
            public bool fixBoundsScale = true;

            public Vector3 positionOffset = Vector3.zero;
            public Vector3 rotationOffset = Vector3.zero;
            public Vector3 scaleMultiplier = Vector3.one;

            public GameObject Evaluate(Tile tile)
            {
                bool evaluated = true;
                bool anythingRequired = false;

                foreach (var slot in slots)
                {
                    if (!slot.Evaluate(tile))
                    {
                        evaluated = false;
                    }
                    if (slot.condition != Condition.None)
                    {
                        anythingRequired = true;
                    }
                }
                return evaluated && anythingRequired ? newObj : null;
            }

            public Slot GetSlot(Vector3Int direction)
            {
                foreach (var slot in slots)
                {
                    if (slot.direction == direction)
                    {
                        return slot;
                    }
                }

                return null;
            }

            [System.Serializable]
            public class Slot
            {
                public Vector3Int direction;

                public Condition condition;

                public Slot()
                {

                }

                public Slot(Slot existingSlot)
                {
                    direction = existingSlot.direction;
                    condition = existingSlot.condition;
                }

                /// <summary>
                /// Draws a texture based on the condition type
                /// </summary>
                public void Draw(Rect position)
                {
                    // Cache proper texture
                    Texture properTexture = GetProperTexture();

                    GUIContent buttonContent = new GUIContent(string.Empty, GetToolTip());

                    if (GUI.Button(position, buttonContent))
                    {
                        if (Event.current.button == 0)
                        {
                            NextCondition();
                        }
                        else if (Event.current.button == 1)
                        {
                            PreviousCondition();
                        }
                    }

                    Rect textureRect = position;

                    textureRect.size *= 0.75f;
                    textureRect.center = position.center;

                    if (properTexture != null)
                    {
                        GUI.DrawTexture(textureRect, properTexture);
                    }
                }

                public void NextCondition()
                {
                    condition++;
                    if (condition > (Condition)2)
                    {
                        condition = 0;
                    }
                }

                public void PreviousCondition()
                {
                    condition--;
                    if (condition < 0)
                    {
                        condition = (Condition)2;
                    }
                }

                public Texture GetProperTexture()
                {
                    switch (condition)
                    {
                        case Condition.ExistingTile:
                            return TextureLibrary.GetTexture("CheckMark");
                        case Condition.NoTile:
                            return TextureLibrary.GetTexture("XMark");
                        default:
                            return null;
                    }
                }

                public Mesh GetProperMesh(out Vector3 scale)
                {
                    scale = Vector3.one * 0.125f;
                    switch (condition)
                    {
                        case Condition.ExistingTile:
                            scale = Vector3.one * 0.125f;
                            return TextureLibrary.GetMesh("CheckMark");
                        case Condition.NoTile:
                            scale = Vector3.one * 0.125f;
                            return TextureLibrary.GetMesh("XMark");
                        default:
                            return null;
                    }
                }

                public Color GetProperColor()
                {
                    switch (condition)
                    {
                        case Condition.ExistingTile:
                            return Color.green;
                        case Condition.NoTile:
                            return Color.red;
                        default:
                            return Color.gray;
                    }
                }

                string GetToolTip()
                {
                    switch (condition)
                    {
                        case Condition.None:
                            return "This direction is currently ignored";
                        case Condition.ExistingTile:
                            return "There must be an existing tile in this direction";
                        case Condition.NoTile:
                            return "There must be no tile in this direction";
                        default:
                            return default;
                    }
                }

                public bool Evaluate(Tile tile)
                {
                    if (tile == null) return false;

                    Tile adjacentTile = tile.GetAdjacentTile(direction);

                    switch (condition)
                    {
                        case Condition.ExistingTile:
                            return adjacentTile != null && adjacentTile.Prefab != null;
                        case Condition.NoTile:
                            return adjacentTile == null || adjacentTile.Prefab == null;
                        default:
                            return true;
                    }
                }
            }
        }
    }
}