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
    }
}
