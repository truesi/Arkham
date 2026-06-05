using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Arkham.Core;
using Arkham.Players;

namespace Arkham.UI
{
    public class GameHUD : MonoBehaviour
    {
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private DoomsdayClock doomsdayClock;
        [SerializeField] private ClueTracker clueTracker;
        [SerializeField] private Player player;

        [SerializeField] private Button endTurnButton;
        [SerializeField] private TMP_Text turnStateText;
        [SerializeField] private TMP_Text doomText;
        [SerializeField] private TMP_Text clueText;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private TMP_Text combatLogText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;

        [Header("Hotkeys")]
        [SerializeField] private Key endTurnKey = Key.Enter;

        private void OnEnable()
        {
            if (endTurnButton != null) endTurnButton.onClick.AddListener(OnEndTurnClicked);
            if (turnManager != null)
            {
                turnManager.OnPlayerTurnStart += HandlePlayerTurnStart;
                turnManager.OnWorldTurnStart += HandleWorldTurnStart;
                turnManager.OnGameOver += HandleGameOver;
                turnManager.OnVictory += HandleVictory;
                turnManager.OnCombatLog += HandleCombatLog;
            }
            if (doomsdayClock != null)
                doomsdayClock.OnDoomChanged += HandleDoomChanged;
            if (clueTracker != null)
                clueTracker.OnClueCountChanged += HandleClueChanged;
            if (player != null)
                player.OnHealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            if (endTurnButton != null) endTurnButton.onClick.RemoveListener(OnEndTurnClicked);
            if (turnManager != null)
            {
                turnManager.OnPlayerTurnStart -= HandlePlayerTurnStart;
                turnManager.OnWorldTurnStart -= HandleWorldTurnStart;
                turnManager.OnGameOver -= HandleGameOver;
                turnManager.OnVictory -= HandleVictory;
                turnManager.OnCombatLog -= HandleCombatLog;
            }
            if (doomsdayClock != null)
                doomsdayClock.OnDoomChanged -= HandleDoomChanged;
            if (clueTracker != null)
                clueTracker.OnClueCountChanged -= HandleClueChanged;
            if (player != null)
                player.OnHealthChanged -= HandleHealthChanged;
        }

        private void Start()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (combatLogText != null) combatLogText.text = "";
            if (doomsdayClock != null)
                HandleDoomChanged(doomsdayClock.CurrentDoom, doomsdayClock.MaxDoom);
            if (clueTracker != null)
                HandleClueChanged(clueTracker.CollectedClues, clueTracker.ClueGoal);
            if (player != null)
                HandleHealthChanged(player.CurrentHealth, player.MaxHealth);
        }

        private void Update()
        {
            if (turnManager == null) return;
            if (turnManager.CurrentState != GameState.PlayerTurn) return;
            Keyboard kb = Keyboard.current;
            if (kb == null) return;
            if (kb[endTurnKey].wasPressedThisFrame) turnManager.EndPlayerTurn();
        }

        private void OnEndTurnClicked()
        {
            if (turnManager != null) turnManager.EndPlayerTurn();
        }

        private void HandlePlayerTurnStart()
        {
            if (turnStateText != null) turnStateText.text = "Your Turn";
            if (endTurnButton != null) endTurnButton.interactable = true;
        }

        private void HandleWorldTurnStart()
        {
            if (turnStateText != null) turnStateText.text = "World Turn...";
            if (endTurnButton != null) endTurnButton.interactable = false;
        }

        private void HandleGameOver()
        {
            if (turnStateText != null) turnStateText.text = "Doomsday";
            if (endTurnButton != null) endTurnButton.interactable = false;
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }

        private void HandleVictory()
        {
            if (turnStateText != null) turnStateText.text = "Victory";
            if (endTurnButton != null) endTurnButton.interactable = false;
            if (victoryPanel != null) victoryPanel.SetActive(true);
        }

        private void HandleDoomChanged(int current, int max)
        {
            if (doomText != null) doomText.text = $"Doom: {current}/{max}";
        }

        private void HandleClueChanged(int current, int goal)
        {
            if (clueText != null) clueText.text = $"Clues: {current}/{goal}";
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (healthText != null) healthText.text = $"HP: {current}/{max}";
        }

        private void HandleCombatLog(string message)
        {
            if (combatLogText != null) combatLogText.text = message;
        }
    }
}
