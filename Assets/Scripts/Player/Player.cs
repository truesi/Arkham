using System;
using UnityEngine;
using Arkham.Map;
using Arkham.Core;
using Arkham.Events;
using Arkham.Entities;

namespace Arkham.Players
{
    [DefaultExecutionOrder(-100)]
    public class Player : MonoBehaviour
    {
        [SerializeField] private MapGraph graph;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private ClueTracker clueTracker;

        [Header("Start node (district 0..2, street 0..2)")]
        [SerializeField] private int startDistrict = 0;
        [SerializeField] private int startStreetIndex = 0;

        [Header("Turn budget")]
        [SerializeField] private int movesPerTurn = 2;

        [Header("Combat")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private int attackDamage = 1;

        [Header("RPG stats (0..MaxStat)")]
        [Tooltip("Strength drives the Attack check (roll <= Strength on a d10 = hit).")]
        [Range(0, MaxStat)] [SerializeField] private int strength = 5;
        [Tooltip("Intelligence drives clue / mystery checks (not wired to a check yet).")]
        [Range(0, MaxStat)] [SerializeField] private int intelligence = 5;
        [Tooltip("Agility drives the Defend check (roll <= Agility on a d10 = block).")]
        [Range(0, MaxStat)] [SerializeField] private int agility = 5;

        /// <summary>Hard cap on every RPG stat, kept simple per the design.</summary>
        public const int MaxStat = 10;

        public Tile CurrentTile { get; private set; }
        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public int AttackDamage => attackDamage;

        public int Strength => strength;
        public int Intelligence => intelligence;
        public int Agility => agility;

        public event Action<int, int> OnHealthChanged;

        private int _movesRemaining;
        private TileMover _mover;

        private void Awake()
        {
            _mover = GetComponent<TileMover>();
            if (_mover == null) _mover = gameObject.AddComponent<TileMover>();
        }

        private void OnEnable()
        {
            if (turnManager != null)
                turnManager.OnPlayerTurnStart += ResetMoves;
        }

        private void OnDisable()
        {
            if (turnManager != null)
                turnManager.OnPlayerTurnStart -= ResetMoves;
        }

        private void Start()
        {
            MapNode startNode = graph.GetStreet(startDistrict, startStreetIndex);
            CurrentTile = startNode.Visual;
            _mover.SnapTo(startNode.WorldPos + WorldYOffset());
            _movesRemaining = movesPerTurn;
            CurrentHealth = maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public bool TryMoveTo(Tile target)
        {
            if (target == null || target.Node == null || CurrentTile == null) return false;
            if (turnManager != null && turnManager.CurrentState != GameState.PlayerTurn) return false;
            if (_mover != null && _mover.IsMoving) return false; // finish the current slide first
            if (_movesRemaining <= 0) return false;
            if (!CurrentTile.Node.Neighbours.Contains(target.Node)) return false;

            _movesRemaining--;

            Enemy enemy = FindEnemyOn(target);
            if (enemy != null)
            {
                if (turnManager != null) turnManager.EnterCombat(this, enemy);
                return false;
            }

            CurrentTile = target;
            _mover.MoveTo(target.Node.WorldPos + WorldYOffset());

            TryCollectClue(target);
            return true;
        }

        public void TakeDamage(int amount)
        {
            if (CurrentHealth <= 0) return;
            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
            Log($"You take {amount} damage ({CurrentHealth}/{maxHealth} HP).");
            if (CurrentHealth == 0 && turnManager != null)
                turnManager.TriggerGameOver();
        }

        private Enemy FindEnemyOn(Tile tile)
        {
            Enemy[] enemies = UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            foreach (Enemy e in enemies)
                if (e.CurrentTile == tile) return e;
            return null;
        }

        private void TryCollectClue(Tile tile)
        {
            ClueEvent clue = tile.GetComponentInChildren<ClueEvent>();
            if (clue == null) return;
            if (clueTracker != null) clueTracker.AddClue();
            Destroy(clue.gameObject);
        }

        private void Log(string msg)
        {
            if (turnManager != null) turnManager.LogCombat(msg);
        }

        private void ResetMoves()
        {
            _movesRemaining = movesPerTurn;
        }

        private Vector3 WorldYOffset()
        {
            return new Vector3(0f, transform.localScale.y, 0f);
        }
    }
}
