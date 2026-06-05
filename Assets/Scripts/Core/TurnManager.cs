using System;
using System.Collections;
using Arkham.Entities;
using Arkham.Players;
using UnityEngine;

namespace Arkham.Core
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private DoomsdayClock doomsdayClock;
        [SerializeField] private ClueTracker clueTracker;

        public GameState CurrentState { get; private set; } = GameState.PlayerTurn;
        public int TurnNumber { get; private set; } = 1;

        public event Action OnPlayerTurnStart;
        public event Action OnWorldTurnStart;
        public event Action OnGameOver;
        public event Action OnVictory;
        public event Action<string> OnCombatLog;
        public event Action<Player, Enemy> OnCombatStart;
        public event Action OnCombatEnd;

        private GameState _preCombatState = GameState.PlayerTurn;

        private void OnEnable()
        {
            if (doomsdayClock != null) doomsdayClock.OnDoomsdayReached += HandleDoomsdayReached;
            if (clueTracker != null) clueTracker.OnAllCluesFound += HandleAllCluesFound;
        }

        private void OnDisable()
        {
            if (doomsdayClock != null) doomsdayClock.OnDoomsdayReached -= HandleDoomsdayReached;
            if (clueTracker != null) clueTracker.OnAllCluesFound -= HandleAllCluesFound;
        }

        private void Start()
        {
            OnPlayerTurnStart?.Invoke();
        }

        public void EndPlayerTurn()
        {
            if (CurrentState != GameState.PlayerTurn) return;
            StartCoroutine(RunWorldTurn());
        }

        private IEnumerator RunWorldTurn()
        {
            CurrentState = GameState.WorldTurn;
            OnWorldTurnStart?.Invoke();

            Enemy[] enemies = UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            foreach (Enemy enemy in enemies)
            {
                if (CurrentState == GameState.GameOver || CurrentState == GameState.Victory) yield break;
                if (enemy == null) continue;

                enemy.RunWorldTurn();

                // Let this enemy finish sliding before the next one acts, so world-turn
                // moves read one at a time (visual only — logic already resolved above).
                while (enemy != null && enemy.IsMoving)
                {
                    if (CurrentState == GameState.GameOver || CurrentState == GameState.Victory) yield break;
                    yield return null;
                }

                while (CurrentState == GameState.Combat) yield return null;
                if (CurrentState == GameState.GameOver || CurrentState == GameState.Victory) yield break;
            }

            if (doomsdayClock != null) doomsdayClock.Advance(1);
            if (CurrentState == GameState.GameOver || CurrentState == GameState.Victory) yield break;

            TurnNumber++;
            CurrentState = GameState.PlayerTurn;
            OnPlayerTurnStart?.Invoke();
        }

        public void EnterCombat(Player player, Enemy enemy)
        {
            if (CurrentState != GameState.PlayerTurn && CurrentState != GameState.WorldTurn) return;
            if (player == null || enemy == null) return;

            _preCombatState = CurrentState;
            CurrentState = GameState.Combat;
            Debug.Log($"[TurnManager] Entering combat: {player.name} vs {enemy.name}");
            OnCombatStart?.Invoke(player, enemy);
        }

        public void ExitCombat()
        {
            if (CurrentState != GameState.Combat) return;

            CurrentState = _preCombatState;
            Debug.Log($"[TurnManager] Exiting combat, returning to {CurrentState}");
            OnCombatEnd?.Invoke();
        }

        public void TriggerGameOver()
        {
            if (CurrentState == GameState.GameOver || CurrentState == GameState.Victory) return;
            bool wasInCombat = CurrentState == GameState.Combat;
            CurrentState = GameState.GameOver;
            if (wasInCombat) OnCombatEnd?.Invoke();
            OnGameOver?.Invoke();
        }

        public void LogCombat(string message)
        {
            OnCombatLog?.Invoke(message);
        }

        private void HandleDoomsdayReached()
        {
            TriggerGameOver();
        }

        private void HandleAllCluesFound()
        {
            if (CurrentState == GameState.GameOver) return;
            CurrentState = GameState.Victory;
            OnVictory?.Invoke();
        }
    }
}
