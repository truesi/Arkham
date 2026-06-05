using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Arkham.Core;
using Arkham.Entities;
using Arkham.Players;

namespace Arkham.UI
{
    public class CombatPanel : MonoBehaviour
    {
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private CombatController combatController;

        [Header("Panel root (toggled on/off)")]
        [SerializeField] private GameObject panelRoot;

        [Header("Player side")]
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text playerHpText;
        [SerializeField] private Slider playerHpBar;

        [Header("Enemy side")]
        [SerializeField] private TMP_Text enemyNameText;
        [SerializeField] private TMP_Text enemyHpText;
        [SerializeField] private Slider enemyHpBar;

        [Header("Controls")]
        [SerializeField] private Button attackButton;
        [SerializeField] private Button defendButton;
        [SerializeField] private TMP_Text combatLogText;

        private Player _player;
        private Enemy _enemy;

        private void OnEnable()
        {
            if (turnManager != null)
            {
                turnManager.OnCombatStart += HandleCombatStart;
                turnManager.OnCombatEnd += HandleCombatEnd;
                turnManager.OnCombatLog += HandleCombatLog;
            }
            if (attackButton != null) attackButton.onClick.AddListener(OnAttackClicked);
            if (defendButton != null) defendButton.onClick.AddListener(OnDefendClicked);
        }

        private void OnDisable()
        {
            if (turnManager != null)
            {
                turnManager.OnCombatStart -= HandleCombatStart;
                turnManager.OnCombatEnd -= HandleCombatEnd;
                turnManager.OnCombatLog -= HandleCombatLog;
            }
            if (attackButton != null) attackButton.onClick.RemoveListener(OnAttackClicked);
            if (defendButton != null) defendButton.onClick.RemoveListener(OnDefendClicked);
            UnsubscribeCombatants();
        }

        private void Start()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnAttackClicked()
        {
            if (combatController != null) combatController.Attack();
        }

        private void OnDefendClicked()
        {
            if (combatController != null) combatController.Defend();
        }

        private void HandleCombatStart(Player player, Enemy enemy)
        {
            _player = player;
            _enemy = enemy;

            if (_player != null) _player.OnHealthChanged += HandlePlayerHealthChanged;
            if (_enemy != null) _enemy.OnHealthChanged += HandleEnemyHealthChanged;

            if (playerNameText != null) playerNameText.text = _player != null ? _player.name : "Player";
            if (enemyNameText != null) enemyNameText.text = _enemy != null ? _enemy.name : "Enemy";

            if (_player != null) HandlePlayerHealthChanged(_player.CurrentHealth, _player.MaxHealth);
            if (_enemy != null) HandleEnemyHealthChanged(_enemy.CurrentHealth, _enemy.MaxHealth);

            if (combatLogText != null) combatLogText.text = "";
            if (panelRoot != null) panelRoot.SetActive(true);
        }

        private void HandleCombatEnd()
        {
            UnsubscribeCombatants();
            _player = null;
            _enemy = null;
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void UnsubscribeCombatants()
        {
            if (_player != null) _player.OnHealthChanged -= HandlePlayerHealthChanged;
            if (_enemy != null) _enemy.OnHealthChanged -= HandleEnemyHealthChanged;
        }

        private void HandlePlayerHealthChanged(int current, int max)
        {
            if (playerHpText != null) playerHpText.text = $"HP {current}/{max}";
            if (playerHpBar != null)
            {
                playerHpBar.maxValue = max;
                playerHpBar.value = current;
            }
        }

        private void HandleEnemyHealthChanged(int current, int max)
        {
            if (enemyHpText != null) enemyHpText.text = $"HP {current}/{max}";
            if (enemyHpBar != null)
            {
                enemyHpBar.maxValue = max;
                enemyHpBar.value = current;
            }
        }

        private void HandleCombatLog(string message)
        {
            if (combatLogText == null) return;
            combatLogText.text = message + "\n" + combatLogText.text;
            const int maxLen = 600;
            if (combatLogText.text.Length > maxLen)
                combatLogText.text = combatLogText.text.Substring(0, maxLen);
        }
    }
}
