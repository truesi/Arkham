using UnityEngine;
using Arkham.Core;

namespace Arkham.Entities
{
    // Bridges the visual TileMover slide to the creep model's Animator.
    //
    // What it does: every frame it tells the Animator whether the enemy is
    // currently gliding to a new tile. The shared CreepAnimator controller then
    // blends Idle <-> Walk off that single "Moving" bool. Nothing here touches
    // gameplay — it only reads TileMover.IsMoving (the same flag the turn manager
    // already uses to sequence world-turn moves).
    //
    // Lives on the enemy ROOT (next to TileMover + the Enemy subclass). The creep
    // model is a LODGroup with TWO Animators (one per level of detail), so we drive
    // ALL of them — otherwise whichever LOD the camera switches to might be running a
    // different (or no) controller. GetComponentsInChildren (plural) grabs them all.
    [RequireComponent(typeof(TileMover))]
    public class EnemyAnimator : MonoBehaviour
    {
        // Hashing the parameter name once is the idiomatic Unity way to set
        // Animator params cheaply every frame (avoids a string lookup each call).
        private static readonly int MovingHash = Animator.StringToHash("Moving");

        private TileMover _mover;
        private Animator[] _animators;

        private void Awake()
        {
            _mover = GetComponent<TileMover>();
            _animators = GetComponentsInChildren<Animator>(true);
        }

        private void Update()
        {
            if (_mover == null || _animators == null) return;
            bool moving = _mover.IsMoving;
            foreach (var a in _animators)
                if (a != null) a.SetBool(MovingHash, moving);
        }
    }
}
