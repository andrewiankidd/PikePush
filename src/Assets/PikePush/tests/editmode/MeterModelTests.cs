using NUnit.Framework;
using PikePush.Combat;

namespace PikePush.Tests.Combat
{
    public class MeterModelTests
    {
        const float Dt = 0.1f;

        [Test]
        public void DefaultStartValue_IsHalf()
        {
            var m = new MeterModel();
            Assert.AreEqual(0.5f, m.Value, 0.0001f);
        }

        [Test]
        public void Tick_WithPushing_IncreasesValue()
        {
            var m = new MeterModel { FillRate = 1f, DrainRate = 1f, StartValue = 0.5f };
            m.Reset();
            m.Tick(Dt, pushing: true);
            Assert.Greater(m.Value, 0.5f);
        }

        [Test]
        public void Tick_WithoutPushing_DecreasesValue()
        {
            var m = new MeterModel { FillRate = 1f, DrainRate = 1f, StartValue = 0.5f };
            m.Reset();
            m.Tick(Dt, pushing: false);
            Assert.Less(m.Value, 0.5f);
        }

        [Test]
        public void Tick_ClampsAtOne()
        {
            var m = new MeterModel { FillRate = 100f, DrainRate = 1f, StartValue = 0.9f };
            m.Reset();
            m.Tick(1f, pushing: true);
            Assert.AreEqual(1f, m.Value);
        }

        [Test]
        public void Tick_ClampsAtZero()
        {
            var m = new MeterModel { FillRate = 1f, DrainRate = 100f, StartValue = 0.1f };
            m.Reset();
            m.Tick(1f, pushing: false);
            Assert.AreEqual(0f, m.Value);
        }

        [Test]
        public void Result_WhenValueOne_IsWon()
        {
            var m = new MeterModel { FillRate = 100f, StartValue = 0.99f };
            m.Reset();
            m.Tick(1f, pushing: true);
            Assert.AreEqual(MeterResult.Won, m.Result);
            Assert.IsTrue(m.IsResolved);
        }

        [Test]
        public void Result_WhenValueZero_IsLost()
        {
            var m = new MeterModel { DrainRate = 100f, StartValue = 0.01f };
            m.Reset();
            m.Tick(1f, pushing: false);
            Assert.AreEqual(MeterResult.Lost, m.Result);
            Assert.IsTrue(m.IsResolved);
        }

        [Test]
        public void Result_MidRange_IsInProgress()
        {
            var m = new MeterModel();
            Assert.AreEqual(MeterResult.InProgress, m.Result);
            Assert.IsFalse(m.IsResolved);
        }

        [Test]
        public void FillRateMultiplier_ScalesFill()
        {
            // Same fill rate, two meters: one boosted, one baseline.
            var baseline = new MeterModel { FillRate = 1f, DrainRate = 0f, StartValue = 0.5f, FillRateMultiplier = 1f };
            var boosted  = new MeterModel { FillRate = 1f, DrainRate = 0f, StartValue = 0.5f, FillRateMultiplier = 1.5f };
            baseline.Reset();
            boosted.Reset();

            baseline.Tick(Dt, pushing: true);
            boosted.Tick(Dt, pushing: true);

            Assert.Greater(boosted.Value, baseline.Value);
            // 50% advantage: boosted should be ~1.5x the baseline's gain.
            float baselineGain = baseline.Value - 0.5f;
            float boostedGain = boosted.Value - 0.5f;
            Assert.AreEqual(1.5f * baselineGain, boostedGain, 0.0001f);
        }

        [Test]
        public void FillRateMultiplier_DoesNotAffectDrain()
        {
            var penalised = new MeterModel { FillRate = 1f, DrainRate = 1f, StartValue = 0.5f, FillRateMultiplier = 0.1f };
            var baseline  = new MeterModel { FillRate = 1f, DrainRate = 1f, StartValue = 0.5f, FillRateMultiplier = 1f };
            penalised.Reset();
            baseline.Reset();

            penalised.Tick(Dt, pushing: false);
            baseline.Tick(Dt, pushing: false);

            Assert.AreEqual(baseline.Value, penalised.Value, 0.0001f);
        }

        [Test]
        public void DrainRateMultiplier_ScalesDrain()
        {
            var slow     = new MeterModel { FillRate = 0f, DrainRate = 1f, StartValue = 0.5f, DrainRateMultiplier = 0.5f };
            var baseline = new MeterModel { FillRate = 0f, DrainRate = 1f, StartValue = 0.5f, DrainRateMultiplier = 1f  };
            slow.Reset();
            baseline.Reset();

            slow.Tick(Dt, pushing: false);
            baseline.Tick(Dt, pushing: false);

            // Slower drain leaves more value on the meter after the same dt.
            Assert.Greater(slow.Value, baseline.Value);
            float slowLoss = 0.5f - slow.Value;
            float baseLoss = 0.5f - baseline.Value;
            Assert.AreEqual(0.5f * baseLoss, slowLoss, 0.0001f);
        }

        [Test]
        public void DrainRateMultiplier_DoesNotAffectFill()
        {
            var slow     = new MeterModel { FillRate = 1f, DrainRate = 1f, StartValue = 0.5f, DrainRateMultiplier = 0.1f };
            var baseline = new MeterModel { FillRate = 1f, DrainRate = 1f, StartValue = 0.5f, DrainRateMultiplier = 1f  };
            slow.Reset();
            baseline.Reset();

            slow.Tick(Dt, pushing: true);
            baseline.Tick(Dt, pushing: true);

            Assert.AreEqual(baseline.Value, slow.Value, 0.0001f);
        }

        [Test]
        public void Reset_RestoresStartValue()
        {
            var m = new MeterModel { FillRate = 1f, StartValue = 0.5f };
            m.Reset();
            m.Tick(1f, pushing: true);
            Assert.AreNotEqual(0.5f, m.Value);
            m.Reset();
            Assert.AreEqual(0.5f, m.Value);
        }

        [Test]
        public void NegativeOrZeroDt_NoChange()
        {
            var m = new MeterModel();
            float before = m.Value;
            m.Tick(0f, pushing: true);
            m.Tick(-1f, pushing: true);
            Assert.AreEqual(before, m.Value);
        }
    }
}
