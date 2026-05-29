using PikePush.Drill;

namespace PikePush.Combat
{
    public enum AttackType
    {
        PikePush,
        CavalryCharge,
    }

    // Period-flavoured counter matrix. The defender's posture + spacing
    // modifies the multiplier on its own MeterModel.FillRate during an
    // engagement of the given AttackType.
    //
    // Numbers mirror the table in
    // memory/project_campaign_combat.md — Closest Order is the +25% push
    // bonus that defines the genre, Bracing for Horse swings +50% vs
    // cavalry but -30% in a normal push (you've committed your pikes).
    public static class CounterMatrix
    {
        public static float FillRateMultiplier(PikePosture posture, SpacingOrder spacing, AttackType attacker)
        {
            // Bracing for Horse dominates: the pikes are grounded and
            // committed to the anti-cavalry stance regardless of spacing.
            if (posture == PikePosture.ChargeForHorse)
                return attacker == AttackType.CavalryCharge ? 1.50f : 0.70f;

            if (attacker == AttackType.PikePush)
            {
                switch (spacing)
                {
                    case SpacingOrder.Open:                return 0.90f;
                    case SpacingOrder.Order:               return 1.00f;
                    case SpacingOrder.Close:               return 1.15f;
                    case SpacingOrder.Closest:             return 1.25f;
                    case SpacingOrder.DoubleDistance:      return 0.80f;
                    case SpacingOrder.TwiceDoubleDistance: return 0.70f;
                }
                return 1.00f;
            }

            // CavalryCharge: density helps survive a charge but never as well
            // as a proper anti-horse brace.
            switch (spacing)
            {
                case SpacingOrder.Open:                return 0.95f;
                case SpacingOrder.Order:               return 0.80f;
                case SpacingOrder.Close:               return 0.75f;
                case SpacingOrder.Closest:             return 0.50f; // catastrophic
                case SpacingOrder.DoubleDistance:      return 0.95f;
                case SpacingOrder.TwiceDoubleDistance: return 0.95f;
            }
            return 0.80f;
        }
    }
}
