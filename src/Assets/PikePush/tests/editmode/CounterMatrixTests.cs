using NUnit.Framework;
using PikePush.Combat;
using PikePush.Drill;

namespace PikePush.Tests.Combat
{
    public class CounterMatrixTests
    {
        [Test]
        public void OrderSpacing_VsPikePush_IsBaseline()
        {
            float m = CounterMatrix.FillRateMultiplier(PikePosture.Order, SpacingOrder.Order, AttackType.PikePush);
            Assert.AreEqual(1.00f, m, 0.0001f);
        }

        [Test]
        public void ClosestOrder_VsPikePush_BeatsCloseBeatsOrderBeatsOpen()
        {
            // The signature drill-mode call: form Closest Order to win a push.
            float closest = CounterMatrix.FillRateMultiplier(PikePosture.Order, SpacingOrder.Closest, AttackType.PikePush);
            float close   = CounterMatrix.FillRateMultiplier(PikePosture.Order, SpacingOrder.Close,   AttackType.PikePush);
            float order   = CounterMatrix.FillRateMultiplier(PikePosture.Order, SpacingOrder.Order,   AttackType.PikePush);
            float open    = CounterMatrix.FillRateMultiplier(PikePosture.Order, SpacingOrder.Open,    AttackType.PikePush);

            Assert.Greater(closest, close);
            Assert.Greater(close, order);
            Assert.Greater(order, open);
        }

        [Test]
        public void Bracing_VsCavalry_IsPerfectCounter()
        {
            float braced  = CounterMatrix.FillRateMultiplier(PikePosture.ChargeForHorse, SpacingOrder.Order, AttackType.CavalryCharge);
            float regular = CounterMatrix.FillRateMultiplier(PikePosture.Order,          SpacingOrder.Order, AttackType.CavalryCharge);

            Assert.Greater(braced, regular);
            Assert.GreaterOrEqual(braced, 1.0f, "Bracing should swing favourable vs horse");
        }

        [Test]
        public void Bracing_VsPikePush_IsHandicap()
        {
            // Brace mid-push-of-pike and you tank — pikes grounded, can't push back.
            float braced  = CounterMatrix.FillRateMultiplier(PikePosture.ChargeForHorse, SpacingOrder.Order, AttackType.PikePush);
            float regular = CounterMatrix.FillRateMultiplier(PikePosture.Order,          SpacingOrder.Order, AttackType.PikePush);

            Assert.Less(braced, regular);
        }

        [Test]
        public void ClosestOrder_VsCavalry_IsCatastrophic()
        {
            // Period rule — tightly-packed pike block is cavalry food if not
            // formally braced. Spacing alone is not enough.
            float closest = CounterMatrix.FillRateMultiplier(PikePosture.Order, SpacingOrder.Closest, AttackType.CavalryCharge);
            Assert.Less(closest, 0.6f);
        }

        [Test]
        public void BracingDominatesSpacing()
        {
            // Spacing varies, but ChargeForHorse always returns the same
            // pair of multipliers (one per attacker) — your pikes are committed.
            float a = CounterMatrix.FillRateMultiplier(PikePosture.ChargeForHorse, SpacingOrder.Open,    AttackType.CavalryCharge);
            float b = CounterMatrix.FillRateMultiplier(PikePosture.ChargeForHorse, SpacingOrder.Closest, AttackType.CavalryCharge);
            Assert.AreEqual(a, b);

            float c = CounterMatrix.FillRateMultiplier(PikePosture.ChargeForHorse, SpacingOrder.Open,    AttackType.PikePush);
            float d = CounterMatrix.FillRateMultiplier(PikePosture.ChargeForHorse, SpacingOrder.Closest, AttackType.PikePush);
            Assert.AreEqual(c, d);
        }

        // --- Drain rate (defensive) tests --------------------------------

        [Test]
        public void DrainMultiplier_TightFormation_HoldsLonger()
        {
            // Closest Order should be more durable than Open Order under
            // a pike-vs-pike push — that's the whole point of forming up tight.
            float closest = CounterMatrix.DrainRateMultiplier(PikePosture.Order, SpacingOrder.Closest, AttackType.PikePush);
            float close   = CounterMatrix.DrainRateMultiplier(PikePosture.Order, SpacingOrder.Close,   AttackType.PikePush);
            float order   = CounterMatrix.DrainRateMultiplier(PikePosture.Order, SpacingOrder.Order,   AttackType.PikePush);
            float open    = CounterMatrix.DrainRateMultiplier(PikePosture.Order, SpacingOrder.Open,    AttackType.PikePush);

            Assert.Less(closest, close);
            Assert.Less(close, order);
            Assert.Less(order, open);
            Assert.Less(closest, 1f);
            Assert.Greater(open, 1f);
        }

        [Test]
        public void DrainMultiplier_BracingVsCavalry_HoldsBest()
        {
            float braced  = CounterMatrix.DrainRateMultiplier(PikePosture.ChargeForHorse, SpacingOrder.Order, AttackType.CavalryCharge);
            float regular = CounterMatrix.DrainRateMultiplier(PikePosture.Order,          SpacingOrder.Order, AttackType.CavalryCharge);
            Assert.Less(braced, regular);
            Assert.Less(braced, 1f);
        }

        [Test]
        public void DrainMultiplier_BracingVsPikePush_BreaksFaster()
        {
            float braced  = CounterMatrix.DrainRateMultiplier(PikePosture.ChargeForHorse, SpacingOrder.Order, AttackType.PikePush);
            float regular = CounterMatrix.DrainRateMultiplier(PikePosture.Order,          SpacingOrder.Order, AttackType.PikePush);
            Assert.Greater(braced, regular);
        }

        [Test]
        public void DrainMultiplier_DenseUnbracedVsCavalry_IsCatastrophic()
        {
            // Tight formations are food for cavalry if you haven't braced.
            float closest = CounterMatrix.DrainRateMultiplier(PikePosture.Order, SpacingOrder.Closest, AttackType.CavalryCharge);
            float open    = CounterMatrix.DrainRateMultiplier(PikePosture.Order, SpacingOrder.Open,    AttackType.CavalryCharge);
            Assert.Greater(closest, open);
        }
    }
}
