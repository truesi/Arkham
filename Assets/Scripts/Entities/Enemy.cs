using System;
using System.Collections.Generic;
using UnityEngine;
using Arkham.Map;
using Arkham.Core;
using Arkham.Players;

namespace Arkham.Entities
{
    // Base class for every enemy. Holds the shared state (health, current tile),
    // the visual clustering when several enemies share a tile, and the BFS
    // movement helpers. Each concrete type (Chaser, Brut, Protector) overrides
    // RunWorldTurn to define how it acts.
    //
    // TurnManager drives enemies directly via RunWorldTurn during the world turn;
    // enemies do NOT subscribe to OnWorldTurnStart. FindObjectsByType<Enemy> still
    // finds every subclass, so the manager/player lookups are unchanged.
    public abstract class Enemy : MonoBehaviour
    {
        [SerializeField] private int attackDamage = 1;
        [SerializeField] private int maxHealth = 2;

        [Header("Identity (shown in the hover tooltip)")]
        [SerializeField] private string displayName = "Enemy";
        [SerializeField] [TextArea] private string description = "";

        private MapGraph _graph;
        private TurnManager _turnManager;
        private Player _player;
        private TileMover _mover;

        public Tile CurrentTile { get; private set; }
        public int AttackDamage => attackDamage;
        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public string DisplayName => displayName;
        public string Description => description;

        // True while this enemy is mid-slide; TurnManager waits on it so world-turn
        // moves read one at a time.
        public bool IsMoving => _mover != null && _mover.IsMoving;

        public event Action<int, int> OnHealthChanged;

        protected virtual void Awake()
        {
            _mover = GetComponent<TileMover>();
            if (_mover == null) _mover = gameObject.AddComponent<TileMover>();
        }

        // Subclass access to the wired scene references.
        protected MapGraph Graph => _graph;
        protected TurnManager TurnManager => _turnManager;
        protected Player Player => _player;

        public void Init(MapGraph graph, TurnManager turnManager, Player player, Tile startTile)
        {
            _graph = graph;
            _turnManager = turnManager;
            _player = player;
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            Place(startTile);
        }

        public void TakeDamage(int amount)
        {
            if (CurrentHealth <= 0) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        // Each concrete enemy defines its own world-turn behaviour.
        public abstract void RunWorldTurn();

        private void Place(Tile tile)
        {
            CurrentTile = tile;
            RelayoutTile(tile, null, snap: true); // appear instantly on spawn
        }

        // Step onto an adjacent tile and re-fan both the vacated and entered tiles.
        protected void MoveTo(Tile next)
        {
            if (next == null) return;
            Tile previous = CurrentTile;
            CurrentTile = next;
            // Turn to face the real travel direction (node→node, so cluster offsets
            // don't skew it). RelayoutTile below handles the position slide.
            if (previous != null && _mover != null)
                _mover.FaceDirection(next.Node.WorldPos - previous.Node.WorldPos);
            RelayoutTile(previous, null, snap: false);   // re-center whoever's left behind
            RelayoutTile(CurrentTile, null, snap: false); // fan out everyone now sharing this tile
        }

        // Ask the turn manager to start a fight between this enemy and the player.
        protected void EngagePlayer()
        {
            if (_turnManager != null && _player != null)
                _turnManager.EnterCombat(_player, this);
        }

        // Visually fan out all enemies sharing a tile onto a small ring so the count
        // is readable. The graph/gameplay is unaffected — this only sets positions.
        // `excluding` lets a departing/dying enemy be left out of the layout.
        // `snap` places instantly (spawn); otherwise each enemy glides via its mover.
        private static void RelayoutTile(Tile tile, Enemy excluding, bool snap)
        {
            if (tile == null) return;
            var here = new List<Enemy>();
            foreach (Enemy e in UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                if (e == null || e == excluding) continue;
                if (e.CurrentTile == tile) here.Add(e);
            }
            here.Sort((a, b) => a.GetInstanceID().CompareTo(b.GetInstanceID()));
            for (int i = 0; i < here.Count; i++)
            {
                Enemy e = here[i];
                Vector3 pos = tile.Node.WorldPos + ClusterOffset(i, here.Count) + e.WorldYOffset();
                if (e._mover == null || snap) { e.transform.position = pos; e._mover?.SnapTo(pos); }
                else e._mover.MoveTo(pos);
            }
        }

        private static Vector3 ClusterOffset(int index, int count)
        {
            if (count <= 1) return Vector3.zero;
            const float ringRadius = 0.55f;
            float ang = (index / (float)count) * Mathf.PI * 2f;
            return new Vector3(Mathf.Sin(ang), 0f, Mathf.Cos(ang)) * ringRadius;
        }

        private void OnDestroy()
        {
            // When an enemy is removed (e.g. killed), re-center the survivors on its tile.
            RelayoutTile(CurrentTile, this, snap: false);
        }

        // One-step BFS: pick the neighbour of `from` that lies on a shortest path to `target`.
        protected static MapNode StepToward(MapNode from, MapNode target)
        {
            if (from == target) return null;

            var parent = new Dictionary<MapNode, MapNode> { { from, null } };
            var queue = new Queue<MapNode>();
            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                MapNode cur = queue.Dequeue();
                if (cur == target) break;
                foreach (MapNode n in cur.Neighbours)
                {
                    if (parent.ContainsKey(n)) continue;
                    parent[n] = cur;
                    queue.Enqueue(n);
                }
            }

            if (!parent.ContainsKey(target)) return null;

            // Walk back from target to find the first step out of `from`.
            MapNode step = target;
            while (parent[step] != from)
            {
                step = parent[step];
                if (step == null) return null;
            }
            return step;
        }

        private Vector3 WorldYOffset()
        {
            return new Vector3(0f, transform.localScale.y, 0f);
        }
    }
}
