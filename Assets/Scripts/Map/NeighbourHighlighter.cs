using System.Collections.Generic;
using UnityEngine;
using Arkham.Core;
using Arkham.Players;

namespace Arkham.Map
{
    // Highlights the 1-hop neighbours of the player's current tile while it's
    // the player's turn. Polls in LateUpdate and diffs against the previously
    // highlighted set, so we don't need a Player.OnTileChanged event.
    public class NeighbourHighlighter : MonoBehaviour
    {
        [SerializeField] private Player player;
        [SerializeField] private TurnManager turnManager;

        private readonly HashSet<Tile> _highlighted = new HashSet<Tile>();

        private void LateUpdate()
        {
            bool active = turnManager == null || turnManager.CurrentState == GameState.PlayerTurn;
            HashSet<Tile> next = new HashSet<Tile>();

            if (active && player != null && player.CurrentTile != null && player.CurrentTile.Node != null)
            {
                foreach (MapNode n in player.CurrentTile.Node.Neighbours)
                    if (n.Visual != null) next.Add(n.Visual);
            }

            foreach (Tile t in _highlighted)
                if (!next.Contains(t)) t.SetHighlight(false);
            foreach (Tile t in next)
                if (!_highlighted.Contains(t)) t.SetHighlight(true);

            _highlighted.Clear();
            foreach (Tile t in next) _highlighted.Add(t);
        }
    }
}
