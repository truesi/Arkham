using System.Collections.Generic;
using UnityEngine;

namespace Arkham.Map
{
    // Wires the Arkham map graph from hand-placed Tile GameObjects in the scene.
    //
    // You build the visuals in the editor (sprites, ground, bridges) and assign:
    //   - 3 streets per district (slots [0..2])
    //   - 3 bridges (between districts 0-1, 0-2, 1-2)
    //
    // Convention for the street slots:
    //   - district d's street index d is the OUTWARD street (faces away from the triangle).
    //   - the other two indices are rim streets, each owning one bridge.
    //   - the bridge between districts a and b uses district a's street index b
    //     and district b's street index a.
    //
    // Intra-district: all 3 streets in a district are mutually connected.
    // A bridge node connects only to its 2 designated rim streets.
    public class MapGraph : MonoBehaviour
    {
        [Header("Streets per district (assign exactly 3 each)")]
        [SerializeField] private Tile[] district0Streets = new Tile[3];
        [SerializeField] private Tile[] district1Streets = new Tile[3];
        [SerializeField] private Tile[] district2Streets = new Tile[3];

        [Header("Bridges between district pairs")]
        [SerializeField] private Tile bridge01;
        [SerializeField] private Tile bridge02;
        [SerializeField] private Tile bridge12;

        private readonly List<MapNode> _nodes = new List<MapNode>();
        private readonly MapNode[,] _streets = new MapNode[3, 3];
        private readonly MapNode[] _bridges = new MapNode[3];

        public IReadOnlyList<MapNode> Nodes => _nodes;

        // Three bridge pairs, ordered: (0,1), (0,2), (1,2).
        private static readonly (int a, int b)[] BridgePairs =
        {
            (0, 1),
            (0, 2),
            (1, 2),
        };

        private void Awake()
        {
            BuildGraph();
        }

        private void BuildGraph()
        {
            Tile[][] districtSlots = { district0Streets, district1Streets, district2Streets };
            Tile[] bridgeSlots = { bridge01, bridge02, bridge12 };

            int nextId = 0;

            // 1. Build street nodes from the referenced Tiles' world positions.
            for (int d = 0; d < 3; d++)
            {
                Tile[] streets = districtSlots[d];
                if (streets == null || streets.Length != 3)
                {
                    Debug.LogError($"MapGraph: district{d}Streets must have exactly 3 entries.", this);
                    return;
                }
                for (int s = 0; s < 3; s++)
                {
                    Tile tile = streets[s];
                    if (tile == null)
                    {
                        Debug.LogError($"MapGraph: district{d}Streets[{s}] is not assigned.", this);
                        return;
                    }
                    var node = new MapNode(nextId++, NodeType.Street, d, tile.transform.position);
                    _streets[d, s] = node;
                    _nodes.Add(node);
                    tile.Bind(node);
                    node.Visual = tile;
                }

                // Intra-district: triangle connectivity.
                _streets[d, 0].LinkBidirectional(_streets[d, 1]);
                _streets[d, 1].LinkBidirectional(_streets[d, 2]);
                _streets[d, 2].LinkBidirectional(_streets[d, 0]);
            }

            // 2. Build bridge nodes and wire to their two rim streets.
            for (int i = 0; i < BridgePairs.Length; i++)
            {
                Tile tile = bridgeSlots[i];
                if (tile == null)
                {
                    Debug.LogError($"MapGraph: bridge slot {i} is not assigned.", this);
                    return;
                }
                (int a, int b) = BridgePairs[i];
                MapNode streetA = _streets[a, b]; // a's rim street facing b
                MapNode streetB = _streets[b, a]; // b's rim street facing a

                var bridge = new MapNode(nextId++, NodeType.Bridge, -1, tile.transform.position);
                _bridges[i] = bridge;
                _nodes.Add(bridge);
                tile.Bind(bridge);
                bridge.Visual = tile;

                bridge.LinkBidirectional(streetA);
                bridge.LinkBidirectional(streetB);
            }
        }

        public MapNode GetStreet(int district, int streetIndex) => _streets[district, streetIndex];
        public MapNode GetBridge(int bridgeIndex) => _bridges[bridgeIndex];

        public IEnumerable<MapNode> AllStreets()
        {
            for (int d = 0; d < 3; d++)
                for (int s = 0; s < 3; s++)
                    yield return _streets[d, s];
        }

#if UNITY_EDITOR
        // Scene-view wiring aid. Draws the expected graph straight from the
        // serialized Tile slots (NOT the runtime _nodes, which only exist after
        // Awake), so you can verify the 12 assignments visually while authoring.
        //   - white lines  = intra-district street triangles
        //   - cyan lines   = bridge links to their two rim streets
        //   - sphere colour: street = white, bridge = cyan, unassigned slot = red marker
        private void OnDrawGizmos()
        {
            Tile[][] districtSlots = { district0Streets, district1Streets, district2Streets };
            Tile[] bridgeSlots = { bridge01, bridge02, bridge12 };

            // Intra-district triangles + street markers + slot labels.
            for (int d = 0; d < 3; d++)
            {
                Tile[] streets = districtSlots[d];
                if (streets == null) continue;
                for (int s = 0; s < 3; s++)
                {
                    Tile t = (s < streets.Length) ? streets[s] : null;
                    if (t == null) continue;
                    Vector3 p = t.transform.position;
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(p, 0.35f);
                    string role = (s == d) ? "outward" : $"rim->D{s}";
                    UnityEditor.Handles.Label(p + Vector3.up * 0.6f, $"D{d}.street[{s}] ({role})");
                }

                // Triangle edges (only where both endpoints are assigned).
                Gizmos.color = Color.white;
                DrawSlotLink(streets, 0, 1);
                DrawSlotLink(streets, 1, 2);
                DrawSlotLink(streets, 2, 0);
            }

            // Bridges to their two designated rim streets.
            for (int i = 0; i < BridgePairs.Length; i++)
            {
                Tile bridge = bridgeSlots[i];
                if (bridge == null) continue;
                Vector3 bp = bridge.transform.position;
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(bp, 0.35f);

                (int a, int b) = BridgePairs[i];
                UnityEditor.Handles.Label(bp + Vector3.up * 0.6f, $"bridge{a}{b}");

                Tile rimA = (districtSlots[a].Length > b) ? districtSlots[a][b] : null; // a's street index b
                Tile rimB = (districtSlots[b].Length > a) ? districtSlots[b][a] : null; // b's street index a
                if (rimA != null) Gizmos.DrawLine(bp, rimA.transform.position);
                if (rimB != null) Gizmos.DrawLine(bp, rimB.transform.position);
            }
        }

        private static void DrawSlotLink(Tile[] streets, int i, int j)
        {
            if (i >= streets.Length || j >= streets.Length) return;
            if (streets[i] == null || streets[j] == null) return;
            Gizmos.DrawLine(streets[i].transform.position, streets[j].transform.position);
        }
#endif
    }
}
