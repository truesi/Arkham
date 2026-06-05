using System.Collections.Generic;
using UnityEngine;
using Arkham.Map;
using Arkham.Players;

namespace Arkham.Events
{
    public class ClueSpawner : MonoBehaviour
    {
        [SerializeField] private MapGraph graph;
        [SerializeField] private Player player;
        [SerializeField] private ClueEvent cluePrefab;
        [SerializeField] private int clueCount = 8;
        [SerializeField] private float yOffset = 0.5f;

        private void Start()
        {
            if (graph == null || cluePrefab == null) return;

            List<Tile> candidates = new List<Tile>();
            foreach (MapNode node in graph.AllStreets())
            {
                if (node.Visual == null) continue;
                if (player != null && node.Visual == player.CurrentTile) continue;
                candidates.Add(node.Visual);
            }

            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            int n = Mathf.Min(clueCount, candidates.Count);
            for (int i = 0; i < n; i++)
            {
                Tile t = candidates[i];
                ClueEvent clue = Instantiate(cluePrefab, t.transform);
                clue.transform.localPosition = new Vector3(0f, yOffset, 0f);
            }
        }
    }
}
