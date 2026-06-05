using Arkham.Map;

namespace Arkham.Entities
{
    // Low HP, low damage. Ignores the player entirely and patrols the three streets
    // of its home district in a fixed loop (street 0 -> 1 -> 2 -> 0). Because a
    // district's three streets are a fully-connected triangle, each patrol step is a
    // single hop. Combat only starts if the player steps onto it.
    public class Protector : Enemy
    {
        public override void RunWorldTurn()
        {
            if (CurrentTile == null || Graph == null) return;

            int home = CurrentTile.Node.DistrictId;
            if (home < 0) return; // somehow on a bridge — skip this patrol step

            // Find our current street slot within the home district, then advance.
            int cur = -1;
            for (int i = 0; i < 3; i++)
            {
                if (Graph.GetStreet(home, i) == CurrentTile.Node) { cur = i; break; }
            }
            if (cur < 0) return;

            MapNode target = Graph.GetStreet(home, (cur + 1) % 3);
            if (target == null || target.Visual == null) return;

            MoveTo(target.Visual);
        }
    }
}
