using Arkham.Map;

namespace Arkham.Entities
{
    // Low damage, high HP. Chases the player like a Chaser but never engages on its
    // own: it stops one tile short and holds, menacing and occupying tiles around
    // the player. Combat only starts if the player chooses to step onto the Brut.
    public class Brut : Enemy
    {
        public override void RunWorldTurn()
        {
            if (Player == null || Player.CurrentTile == null || CurrentTile == null) return;
            if (CurrentTile == Player.CurrentTile) return;

            MapNode next = StepToward(CurrentTile.Node, Player.CurrentTile.Node);
            if (next == null || next.Visual == null) return;

            // Stop one tile short of the player — block, don't engage.
            if (next.Visual == Player.CurrentTile) return;

            MoveTo(next.Visual);
        }
    }
}
