using UnityEngine;

namespace Arkham.Core
{
    // Smoothly slides a transform to a target world position over a fixed duration,
    // like a chess piece sliding into place.
    //
    // IMPORTANT: this is visual only. Gameplay logic (which tile you're on, clue
    // collection, combat triggers, doom) still resolves the instant a move happens —
    // this just animates transform.position toward where the logic already put you.
    // So turn order and combat are never blocked waiting on an animation.
    //
    // The Player and every Enemy get one of these (auto-added in their Awake if the
    // prefab doesn't already carry it). Use SnapTo for instant placement (spawn) and
    // MoveTo to glide.
    public class TileMover : MonoBehaviour
    {
        [Tooltip("Seconds for one tile-to-tile slide. 0 = instant (teleport).")]
        [SerializeField] private float moveDuration = 0.25f;

        [Header("Facing")]
        [Tooltip("Degrees added to the look-at, for models whose forward isn't +Z. " +
                 "The creep models face +Z, so leave at 0.")]
        [SerializeField] private float yawOffset = 0f;

        private Vector3 _start;
        private Vector3 _target;
        private float _elapsed;
        private bool _moving;

        private Quaternion _startRot;
        private Quaternion _targetRot;
        private float _rotElapsed;
        private bool _rotating;

        public bool IsMoving => _moving;

        private void Awake()
        {
            _target = transform.position;
        }

        // Instant placement, no slide (use on spawn / respawn).
        public void SnapTo(Vector3 position)
        {
            _moving = false;
            _target = position;
            transform.position = position;
        }

        // Begin gliding to a new world position.
        public void MoveTo(Vector3 position)
        {
            if (moveDuration <= 0f) { SnapTo(position); return; }
            _start = transform.position;
            _target = position;
            _elapsed = 0f;
            _moving = true;
        }

        // Smoothly turn to face a horizontal world direction over moveDuration.
        // Call this on a real tile-to-tile step (see Enemy.MoveTo). Cosmetic cluster
        // re-fans deliberately DON'T call it, so survivors keep their facing instead
        // of spinning toward their new ring slot.
        public void FaceDirection(Vector3 worldDir)
        {
            worldDir.y = 0f;
            if (worldDir.sqrMagnitude < 0.0001f) return; // no direction → keep current facing
            Quaternion target = Quaternion.LookRotation(worldDir.normalized) * Quaternion.Euler(0f, yawOffset, 0f);
            if (moveDuration <= 0f) { transform.rotation = target; return; }
            _startRot = transform.rotation;
            _targetRot = target;
            _rotElapsed = 0f;
            _rotating = true;
        }

        private void Update()
        {
            // SmoothStep gives a gentle ease-in / ease-out so the piece settles nicely.
            if (_moving)
            {
                _elapsed += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_elapsed / moveDuration));
                transform.position = Vector3.Lerp(_start, _target, k);

                if (_elapsed >= moveDuration)
                {
                    transform.position = _target;
                    _moving = false;
                }
            }

            if (_rotating)
            {
                _rotElapsed += Time.deltaTime;
                float kr = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_rotElapsed / moveDuration));
                transform.rotation = Quaternion.Slerp(_startRot, _targetRot, kr);

                if (_rotElapsed >= moveDuration)
                {
                    transform.rotation = _targetRot;
                    _rotating = false;
                }
            }
        }
    }
}
