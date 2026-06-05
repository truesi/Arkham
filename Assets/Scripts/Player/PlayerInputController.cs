using UnityEngine;
using UnityEngine.InputSystem;
using Arkham.Map;
using Arkham.Entities;

namespace Arkham.Players
{
    public class PlayerInputController : MonoBehaviour
    {
        [SerializeField] private Player player;
        [SerializeField] private Camera cam;

        private void Awake()
        {
            if (cam == null) cam = Camera.main;
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;
            if (!mouse.leftButton.wasPressedThisFrame) return;

            Vector2 mousePos = mouse.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;

            Tile tile = hit.collider.GetComponentInParent<Tile>();
            if (tile == null)
            {
                Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
                if (enemy != null) tile = enemy.CurrentTile;
            }
            if (tile == null) return;

            player.TryMoveTo(tile);
        }
    }
}
