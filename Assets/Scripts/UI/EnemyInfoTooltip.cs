using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Arkham.Core;
using Arkham.Entities;

namespace Arkham.UI
{
    // Shows a small info panel (name / description / HP / attack) while the mouse
    // hovers over an enemy in the world.
    //
    // Pattern: this script lives on an always-active GameObject under the Canvas and
    // toggles a CHILD `panelRoot`. Never put it on the object it disables, or it would
    // switch itself off and stop running (same gotcha as CombatPanel).
    //
    // Hover detection reuses the PlayerInputController approach: raycast from the mouse
    // into the 3D scene each frame and resolve the hit back to an Enemy. Enemy capsules
    // use trigger colliders, which Physics.Raycast hits by default.
    public class EnemyInfoTooltip : MonoBehaviour
    {
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private Camera cam;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private TMP_Text descText;

        [Header("Placement")]
        [SerializeField] private Vector2 cursorOffset = new Vector2(18f, -18f);
        [SerializeField] private float edgePadding = 8f;

        private Canvas _canvas;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
            _canvas = GetComponentInParent<Canvas>();
            Hide();
        }

        private void Update()
        {
            Enemy enemy = HoverTarget();
            if (enemy == null) { Hide(); return; }
            Show(enemy);
        }

        private Enemy HoverTarget()
        {
            // Don't pop the tooltip over the full-screen combat panel / end screens —
            // only while examining the board on the player or world turn.
            if (turnManager != null &&
                turnManager.CurrentState != GameState.PlayerTurn &&
                turnManager.CurrentState != GameState.WorldTurn)
                return null;

            Mouse mouse = Mouse.current;
            if (mouse == null || cam == null) return null;

            Vector2 mousePos = mouse.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);
            if (!Physics.Raycast(ray, out RaycastHit hit)) return null;
            return hit.collider.GetComponentInParent<Enemy>();
        }

        private void Show(Enemy e)
        {
            if (panelRoot == null) return;
            if (!panelRoot.gameObject.activeSelf) panelRoot.gameObject.SetActive(true);

            if (nameText != null) nameText.text = e.DisplayName;
            if (statsText != null) statsText.text = $"HP {e.CurrentHealth}/{e.MaxHealth}    ATK {e.AttackDamage}";
            if (descText != null) descText.text = e.Description;

            PositionAtCursor();
        }

        private void PositionAtCursor()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;
            Vector2 m = mouse.position.ReadValue();

            // Overlay canvas: a child's world position maps 1:1 to screen pixels.
            // The panel pivot is top-left, so it grows right and downward from here.
            Vector3 target = new Vector3(m.x + cursorOffset.x, m.y + cursorOffset.y, 0f);

            // Keep it fully on screen (panel pixel size = local size × canvas scale).
            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            Vector2 px = panelRoot.rect.size * scale;
            target.x = Mathf.Clamp(target.x, edgePadding, Mathf.Max(edgePadding, Screen.width - px.x - edgePadding));
            target.y = Mathf.Clamp(target.y, px.y + edgePadding, Screen.height - edgePadding);
            panelRoot.position = target;
        }

        private void Hide()
        {
            if (panelRoot != null && panelRoot.gameObject.activeSelf)
                panelRoot.gameObject.SetActive(false);
        }
    }
}
