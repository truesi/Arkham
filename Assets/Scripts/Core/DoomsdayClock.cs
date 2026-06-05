using System;
using UnityEngine;

namespace Arkham.Core
{
    public class DoomsdayClock : MonoBehaviour
    {
        [SerializeField] private int maxDoom = 15;

        public int CurrentDoom { get; private set; }
        public int MaxDoom => maxDoom;

        public event Action<int, int> OnDoomChanged;
        public event Action OnDoomsdayReached;

        public void Advance(int amount = 1)
        {
            CurrentDoom = Mathf.Min(CurrentDoom + amount, maxDoom);
            OnDoomChanged?.Invoke(CurrentDoom, maxDoom);
            if (CurrentDoom >= maxDoom)
                OnDoomsdayReached?.Invoke();
        }
    }
}
