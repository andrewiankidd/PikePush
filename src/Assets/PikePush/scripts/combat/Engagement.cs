using PikePush.Drill;

namespace PikePush.Combat
{
    // One engagement = two facing blocks, two parallel push-meters.
    // First meter to fill (Won) declares its block the winner; first meter to
    // drain (Lost) declares its block the loser, the other block the winner.
    // Pure logic — input pushed in via Tick(), no Unity dependencies of its
    // own. DrillBootstrap (and later Campaign) own the lifecycle.
    public class Engagement
    {
        public Block A { get; }
        public Block B { get; }
        public MeterModel MeterA { get; } = new MeterModel();
        public MeterModel MeterB { get; } = new MeterModel();

        public Block Winner { get; private set; }
        public Block Loser { get; private set; }
        public bool IsResolved => Winner != null;

        public Engagement(Block a, Block b)
        {
            A = a;
            B = b;
        }

        public void Tick(float dt, bool pushingA, bool pushingB)
        {
            if (IsResolved) return;

            MeterA.Tick(dt, pushingA);
            MeterB.Tick(dt, pushingB);

            // Order matters only for the simultaneous-resolve edge case;
            // 'Won' beats 'Lost' so the side that succeeded gets credit.
            if (MeterA.Result == MeterResult.Won)       Resolve(A, B);
            else if (MeterB.Result == MeterResult.Won)  Resolve(B, A);
            else if (MeterA.Result == MeterResult.Lost) Resolve(B, A);
            else if (MeterB.Result == MeterResult.Lost) Resolve(A, B);
        }

        void Resolve(Block winner, Block loser)
        {
            Winner = winner;
            Loser = loser;
        }
    }
}
