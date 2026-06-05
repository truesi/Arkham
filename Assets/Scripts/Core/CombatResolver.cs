using UnityEngine;

namespace Arkham.Core
{
    /// <summary>
    /// Resolves combat skill checks. Both Attack (vs Strength) and Defend (vs Agility)
    /// roll a d10 and succeed when the roll is &lt;= the relevant stat, so a stat on the
    /// 0..10 scale maps directly to a (stat x 10)% success chance.
    /// </summary>
    public static class CombatResolver
    {
        public const int DiceSides = 10;

        public static int Roll()
        {
            return Random.Range(1, DiceSides + 1);
        }

        /// <summary>True when the roll passes a check against the given stat (roll &lt;= stat).</summary>
        public static bool IsSuccess(int roll, int stat)
        {
            return roll <= stat;
        }
    }
}
