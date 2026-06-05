using System;
using UnityEngine;

namespace Arkham.Core
{
    public class ClueTracker : MonoBehaviour
    {
        [SerializeField] private int clueGoal = 5;

        public int CollectedClues { get; private set; }
        public int ClueGoal => clueGoal;

        public event Action<int, int> OnClueCountChanged;
        public event Action OnAllCluesFound;

        public void AddClue()
        {
            if (CollectedClues >= clueGoal) return;
            CollectedClues++;
            OnClueCountChanged?.Invoke(CollectedClues, clueGoal);
            if (CollectedClues >= clueGoal)
                OnAllCluesFound?.Invoke();
        }
    }
}
