using UnityEngine;
using UnityEngine.InputSystem;
using Arkham.Entities;
using Arkham.Players;

namespace Arkham.Core
{
    public class CombatController : MonoBehaviour
    {
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private Key attackKey = Key.A;
        [SerializeField] private Key defendKey = Key.D;

        private Player _player;
        private Enemy _enemy;

        public Player CurrentPlayer => _player;
        public Enemy CurrentEnemy => _enemy;

        private void OnEnable()
        {
            if (turnManager != null)
            {
                turnManager.OnCombatStart += HandleCombatStart;
                turnManager.OnCombatEnd += HandleCombatEnd;
            }
        }

        private void OnDisable()
        {
            if (turnManager != null)
            {
                turnManager.OnCombatStart -= HandleCombatStart;
                turnManager.OnCombatEnd -= HandleCombatEnd;
            }
        }

        private void Update()
        {
            if (turnManager == null || turnManager.CurrentState != GameState.Combat) return;
            if (Keyboard.current == null) return;
            if (Keyboard.current[attackKey].wasPressedThisFrame)
                Attack();
            else if (Keyboard.current[defendKey].wasPressedThisFrame)
                Defend();
        }

        /// <summary>
        /// Offensive round: roll Strength to hit. On a hit the enemy takes damage.
        /// You commit to attacking, so the enemy's counter always lands (use Defend to avoid it).
        /// </summary>
        public void Attack()
        {
            if (!CanAct()) return;

            int pRoll = CombatResolver.Roll();
            bool pHit = CombatResolver.IsSuccess(pRoll, _player.Strength);
            turnManager.LogCombat($"You attack. Roll {pRoll} vs STR {_player.Strength}: {(pHit ? "HIT" : "MISS")}");
            if (pHit)
            {
                _enemy.TakeDamage(_player.AttackDamage);
                turnManager.LogCombat($"Enemy HP {_enemy.CurrentHealth}/{_enemy.MaxHealth}.");
                if (_enemy.CurrentHealth <= 0)
                {
                    turnManager.LogCombat("Enemy defeated!");
                    Destroy(_enemy.gameObject);
                    turnManager.ExitCombat();
                    return;
                }
            }

            // Attacking leaves you open; the enemy's counter is not defended.
            EnemyCounter(defended: false);
        }

        /// <summary>
        /// Defensive round: forgo your attack and roll Agility to brace. On success the
        /// enemy's counter this round is blocked. Spends the round either way.
        /// </summary>
        public void Defend()
        {
            if (!CanAct()) return;

            int dRoll = CombatResolver.Roll();
            bool defended = CombatResolver.IsSuccess(dRoll, _player.Agility);
            turnManager.LogCombat($"You defend. Roll {dRoll} vs AGI {_player.Agility}: {(defended ? "BRACED" : "FAILED")}");

            EnemyCounter(defended);
        }

        private void EnemyCounter(bool defended)
        {
            if (defended)
            {
                turnManager.LogCombat("Enemy attacks, but you block it!");
                return;
            }

            turnManager.LogCombat($"Enemy attacks and hits for {_enemy.AttackDamage}.");
            _player.TakeDamage(_enemy.AttackDamage);
        }

        private bool CanAct()
        {
            if (turnManager == null || turnManager.CurrentState != GameState.Combat) return false;
            return _player != null && _enemy != null;
        }

        private void HandleCombatStart(Player player, Enemy enemy)
        {
            _player = player;
            _enemy = enemy;
            turnManager.LogCombat($"Combat! You {_player.CurrentHealth}/{_player.MaxHealth} vs Enemy {_enemy.CurrentHealth}/{_enemy.MaxHealth}. [{attackKey}] attack, [{defendKey}] defend.");
        }

        private void HandleCombatEnd()
        {
            _player = null;
            _enemy = null;
        }
    }
}
