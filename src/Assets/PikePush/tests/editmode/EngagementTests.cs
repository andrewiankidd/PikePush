using NUnit.Framework;
using PikePush.Combat;

namespace PikePush.Tests.Combat
{
    public class EngagementTests
    {
        // We don't want tests to allocate Block GameObjects, so the engagement
        // is constructed with nulls for the block refs and we only check the
        // meter semantics. The Winner/Loser nullness still tells us about
        // resolution outcomes (both stay null until resolved).

        [Test]
        public void NotResolved_OnConstruction()
        {
            var e = new Engagement(null, null);
            Assert.IsFalse(e.IsResolved);
        }

        [Test]
        public void Tick_WhenAFills_AWins()
        {
            var e = new Engagement(null, null);
            e.MeterA.FillRate = 100f;
            e.MeterA.StartValue = 0.99f;
            e.MeterA.Reset();

            e.Tick(1f, pushingA: true, pushingB: false);

            Assert.IsTrue(e.IsResolved);
            // Winner has to be A (we passed A's ref as null but the engagement
            // still tracks identity by the original ctor argument — which was
            // null, so we can only check the meter state directly).
            Assert.AreEqual(MeterResult.Won, e.MeterA.Result);
        }

        [Test]
        public void Tick_WhenBDrainsToZero_AWins()
        {
            var e = new Engagement(null, null);
            e.MeterB.DrainRate = 100f;
            e.MeterB.StartValue = 0.01f;
            e.MeterB.Reset();

            e.Tick(1f, pushingA: false, pushingB: false);

            Assert.IsTrue(e.IsResolved);
            Assert.AreEqual(MeterResult.Lost, e.MeterB.Result);
        }

        [Test]
        public void Tick_OnceResolved_NoFurtherChanges()
        {
            var e = new Engagement(null, null);
            e.MeterA.FillRate = 100f;
            e.MeterA.StartValue = 0.99f;
            e.MeterA.Reset();

            e.Tick(1f, pushingA: true, pushingB: false);
            float aFrozen = e.MeterA.Value;
            float bFrozen = e.MeterB.Value;

            // After resolution, additional ticks must not move the meters.
            e.Tick(1f, pushingA: true, pushingB: true);
            Assert.AreEqual(aFrozen, e.MeterA.Value);
            Assert.AreEqual(bFrozen, e.MeterB.Value);
        }

        [Test]
        public void Tick_BothMetersAdvanceIndependently()
        {
            var e = new Engagement(null, null);
            e.MeterA.FillRate = 0.2f;
            e.MeterA.DrainRate = 0.2f;
            e.MeterB.FillRate = 0.2f;
            e.MeterB.DrainRate = 0.2f;
            e.MeterA.StartValue = 0.5f;
            e.MeterB.StartValue = 0.5f;
            e.MeterA.Reset();
            e.MeterB.Reset();

            e.Tick(0.5f, pushingA: true, pushingB: false);
            Assert.Greater(e.MeterA.Value, 0.5f);
            Assert.Less(e.MeterB.Value, 0.5f);
        }
    }
}
