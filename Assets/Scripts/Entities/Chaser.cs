using Arkham.Map;

namespace Arkham.Entities
{
    // High damage, low HP. Relentlessly chases the player across the whole map and
    // engages (starts combat) the moment its next step would land on the player.
    public class Chaser : Enemy
    {
        public override void RunWorldTurn()
        {
            if (Player == null || Player.CurrentTile == null || CurrentTile == null) return;
            if (CurrentTile == Player.CurrentTile) return;

            MapNode next = StepToward(CurrentTile.Node, Player.CurrentTile.Node);
            if (next == null || next.Visual == null) return;

            if (next.Visual == Player.CurrentTile)
            {
                EngagePlayer();
                return;
            }

            MoveTo(next.Visual);
        }
    }
}
