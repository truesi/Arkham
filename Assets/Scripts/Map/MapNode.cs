using System.Collections.Generic;
using UnityEngine;

namespace Arkham.Map
{
    public enum NodeType { Street, Bridge }

    public enum NodeState { Clean, HasClue, Void }

    // Plain data: a single movement node in the map graph.
    // Streets belong to a district (0..2). Bridges have districtId = -1.
    // Neighbours are populated by MapGraph during build.
    public class MapNode
    {
        public int Id { get; }
        public NodeType Type { get; }
        public int DistrictId { get; }
        public Vector3 WorldPos { get; }
        public List<MapNode> Neighbours { get; } = new List<MapNode>();
        public NodeState State { get; set; } = NodeState.Clean;

        // Visual hook: the Tile MonoBehaviour that renders this node, if spawned.
        public Tile Visual { get; set; }

        public MapNode(int id, NodeType type, int districtId, Vector3 worldPos)
        {
            Id = id;
            Type = type;
            DistrictId = districtId;
            WorldPos = worldPos;
        }

        public void LinkBidirectional(MapNode other)
        {
            if (other == null || other == this) return;
            if (!Neighbours.Contains(other)) Neighbours.Add(other);
            if (!other.Neighbours.Contains(this)) other.Neighbours.Add(this);
        }
    }
}
