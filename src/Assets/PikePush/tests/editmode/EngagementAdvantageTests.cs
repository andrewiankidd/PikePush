using NUnit.Framework;
using PikePush.Combat;
using PikePush.Drill;

namespace PikePush.Tests.Combat
{
    public class EngagementAdvantageTests
    {
        // Drill-mode default: pike vs pike.
        const AttackType PvP = AttackType.PikePush;

        [Test]
        public void SamePostureAndSpacing_CancelsToZero()
        {
            // The defining case: matching stances mean nothing should show.
            var (a, b) = EngagementAdvantage.Compute(
                PikePosture.Order, SpacingOrder.Closest, PvP,
                PikePosture.Order, SpacingOrder.Closest, PvP);

            Assert.AreEqual(0f, a.PushDelta, 0.0001f);
            Assert.AreEqual(0f, a.HoldDelta, 0.0001f);
            Assert.AreEqual(0f, b.PushDelta, 0.0001f);
            Assert.AreEqual(0f, b.HoldDelta, 0.0001f);
            Assert.IsFalse(a.HasAdvantage);
            Assert.IsFalse(b.HasAdvantage);
        }

        [Test]
        public void ClosestVsOpen_FavorsClosestOnBothAxes()
        {
            // Period rule — Closest pushes harder AND holds longer than Open.
            var (closest, open) = EngagementAdvantage.Compute(
                PikePosture.Order, SpacingOrder.Closest, PvP,
                PikePosture.Order, SpacingOrder.Open,    PvP);

            Assert.Greater(closest.PushDelta, 0f);
            Assert.Greater(closest.HoldDelta, 0f);
            Assert.IsTrue(closest.HasAdvantage);

            // Disadvantaged side reports negative — its UI displays nothing.
            Assert.Less(open.PushDelta, 0f);
            Assert.Less(open.HoldDelta, 0f);
            Assert.IsFalse(open.HasAdvantage);
        }

        [Test]
        public void OppositeDeltas_AreSymmetric()
        {
            var (a, b) = EngagementAdvantage.Compute(
                PikePosture.Order, SpacingOrder.Closest, PvP,
                PikePosture.Order, SpacingOrder.Open,    PvP);

            Assert.AreEqual(a.PushDelta, -b.PushDelta, 0.0001f);
            Assert.AreEqual(a.HoldDelta, -b.HoldDelta, 0.0001f);
        }

        [Test]
        public void BracedVsRegular_PenalisesBracerInPikePush()
        {
            // ChargeForHorse in a pike push: pikes committed, you take a hit
            // on both axes vs a non-braced opponent.
            var (braced, regular) = EngagementAdvantage.Compute(
                PikePosture.ChargeForHorse, SpacingOrder.Order, PvP,
                PikePosture.Order,          SpacingOrder.Order, PvP);

            Assert.Less(braced.PushDelta, 0f);
            Assert.Less(braced.HoldDelta, 0f);
            Assert.IsFalse(braced.HasAdvantage);

            Assert.Greater(regular.PushDelta, 0f);
            Assert.Greater(regular.HoldDelta, 0f);
            Assert.IsTrue(regular.HasAdvantage);
        }

        [Test]
        public void MatchingClosestOrder_OnBothSides_StillCancels()
        {
            // Both forming Closest Order = both get the +25% push bonus from
            // baseline — but vs each other, neither has an EDGE. The display
            // must show nothing.
            var (a, b) = EngagementAdvantage.Compute(
                PikePosture.Advance, SpacingOrder.Closest, PvP,
                PikePosture.Advance, SpacingOrder.Closest, PvP);

            Assert.IsFalse(a.HasAdvantage);
            Assert.IsFalse(b.HasAdvantage);
        }
    }
}
