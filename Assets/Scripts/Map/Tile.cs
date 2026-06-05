using UnityEngine;

namespace Arkham.Map
{
    // Visual + click target for a single MapNode. Holds no game data of its own —
    // game state (type, district, neighbours, Clean/Void/HasClue) lives on MapNode.
    public class Tile : MonoBehaviour
    {
        public MapNode Node { get; private set; }

        // Highlight tint multiplied onto the tile sprite when it's a reachable neighbour.
        // >1 channels brighten the (dark) art so the tile clearly "lights up".
        private static readonly Color HighlightTint = new Color(1.85f, 1.75f, 1.25f, 1f);

        private SpriteRenderer _sprite;   // art tiles: tint this to highlight
        private Color _baseSpriteColor;
        private Renderer _renderer;       // fallback for placeholder (cube) tiles
        private Color _baseColor;

        private void Awake()
        {
            _sprite = GetComponentInChildren<SpriteRenderer>();
            if (_sprite != null)
            {
                _baseSpriteColor = _sprite.color;
            }
            else
            {
                _renderer = GetComponentInChildren<Renderer>();
                if (_renderer != null)
                    _baseColor = _renderer.material.color;
            }
        }

        public void Bind(MapNode node)
        {
            Node = node;
            gameObject.name = node.Type == NodeType.Bridge
                ? $"Bridge_{node.Id}"
                : $"Street_d{node.DistrictId}_n{node.Id}";
        }

        public void SetHighlight(bool on)
        {
            if (_sprite != null)
            {
                _sprite.color = on ? HighlightTint : _baseSpriteColor;
                return;
            }
            if (_renderer == null) return;
            _renderer.material.color = on ? Color.yellow : _baseColor;
        }
    }
}
