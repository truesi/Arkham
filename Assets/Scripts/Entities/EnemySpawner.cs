using System.Collections.Generic;
using UnityEngine;
using Arkham.Map;
using Arkham.Core;
using Arkham.Players;

namespace Arkham.Entities
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private MapGraph graph;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private Player player;

        [Header("Enemy types (one prefab + count each)")]
        [SerializeField] private Enemy chaserPrefab;
        [SerializeField] private int chaserCount = 1;
        [SerializeField] private Enemy brutPrefab;
        [SerializeField] private int brutCount = 1;
        [SerializeField] private Enemy protectorPrefab;
        [SerializeField] private int protectorCount = 1;

        [Header("Placement")]
        [SerializeField] private int minDistanceFromPlayer = 3;

        private void Start()
        {
            if (graph == null) return;

            MapNode playerNode = player != null && player.CurrentTile != null ? player.CurrentTile.Node : null;
            Dictionary<MapNode, int> dist = playerNode != null ? BFSDistances(playerNode) : null;

            List<Tile> candidates = new List<Tile>();
            foreach (MapNode node in graph.AllStreets())
            {
                if (node.Visual == null) continue;
                if (dist != null && dist.TryGetValue(node, out int d) && d < minDistanceFromPlayer) continue;
                candidates.Add(node.Visual);
            }

            // One shared, shuffled pool so the different types don't overlap on spawn.
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            int next = 0;
            next = SpawnBatch(chaserPrefab, chaserCount, candidates, next);
            next = SpawnBatch(brutPrefab, brutCount, candidates, next);
            next = SpawnBatch(protectorPrefab, protectorCount, candidates, next);
        }

        // Spawn up to `count` of `prefab` onto the next free candidate tiles, starting
        // at `start`. Returns the index of the next free candidate.
        private int SpawnBatch(Enemy prefab, int count, List<Tile> candidates, int start)
        {
            if (prefab == null) return start;
            int i = start;
            int spawned = 0;
            while (spawned < count && i < candidates.Count)
            {
                Enemy enemy = Instantiate(prefab, transform);
                enemy.Init(graph, turnManager, player, candidates[i]);
                i++;
                spawned++;
            }
            return i;
        }

        private static Dictionary<MapNode, int> BFSDistances(MapNode from)
        {
            var dist = new Dictionary<MapNode, int> { { from, 0 } };
            var queue = new Queue<MapNode>();
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                MapNode cur = queue.Dequeue();
                foreach (MapNode n in cur.Neighbours)
                {
                    if (dist.ContainsKey(n)) continue;
                    dist[n] = dist[cur] + 1;
                    queue.Enqueue(n);
                }
            }
            return dist;
        }
    }
}
