using PikePush.Drill;

namespace PikePush.Combat
{
    // Per-side advantage breakdown for an engagement, expressed as
    // percentage-point deltas between the two blocks' counter-matrix
    // multipliers. The whole point: if both blocks are in the same stance
    // every delta is zero and nothing gets shown to the player — only
    // genuine asymmetries surface as advantages.
    //
    // Push: delta on FillRateMultiplier. Higher = pushes harder.
    // Hold: delta on DrainRateMultiplier inverted. Higher = holds longer.
    public static class EngagementAdvantage
    {
        public struct Side
        {
            public float PushDelta; // percentage points, positive = this side has the edge
            public float HoldDelta; // percentage points, positive = this side resists drain better

            public bool HasAdvantage => PushDelta > Epsilon || HoldDelta > Epsilon;
        }

        // Small threshold so floating-point noise doesn't briefly flash text on
        // mathematically-identical formations.
        const float Epsilon = 0.5f;

        public static (Side a, Side b) Compute(
            PikePosture aPosture, SpacingOrder aSpacing, AttackType aFaces,
            PikePosture bPosture, SpacingOrder bSpacing, AttackType bFaces)
        {
            float aFill  = CounterMatrix.FillRateMultiplier(aPosture, aSpacing, bFaces);
            float aDrain = CounterMatrix.DrainRateMultiplier(aPosture, aSpacing, bFaces);
            float bFill  = CounterMatrix.FillRateMultiplier(bPosture, bSpacing, aFaces);
            float bDrain = CounterMatrix.DrainRateMultiplier(bPosture, bSpacing, aFaces);

            // Push: A vs B = A's fill mult minus B's (higher = bigger push edge).
            // Hold: A vs B = B's drain mult minus A's (higher = A drains slower,
            //   i.e. holds longer under pressure).
            float pushDelta = (aFill - bFill) * 100f;
            float holdDelta = (bDrain - aDrain) * 100f;

            return (
                new Side { PushDelta =  pushDelta, HoldDelta =  holdDelta },
                new Side { PushDelta = -pushDelta, HoldDelta = -holdDelta }
            );
        }
    }
}
