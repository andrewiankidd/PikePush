using UnityEngine;

namespace PikePush.Combat
{
    public enum MeterResult
    {
        InProgress,
        Won,
        Lost,
    }

    // Pure-logic core of the push-of-pike mash meter. Used by the runner's
    // MeterGame UI and by drill / campaign engagements. No Unity component
    // dependencies — host calls Tick() each frame with the current 'pushing'
    // state from whatever source drives it (player input or AI).
    public class MeterModel
    {
        public float Value { get; private set; }
        public float FillRate { get; set; } = 0.6f;
        public float DrainRate { get; set; } = 0.12f;
        public float StartValue { get; set; } = 0.5f;
        // Counter-matrix hooks. 1.0 = baseline on both.
        //   FillRateMultiplier  <1 = harder push;     >1 = easier push.
        //   DrainRateMultiplier <1 = harder to budge; >1 = breaks easier.
        public float FillRateMultiplier { get; set; } = 1f;
        public float DrainRateMultiplier { get; set; } = 1f;

        public MeterModel()
        {
            Value = StartValue;
        }

        public MeterModel(float fillRate, float drainRate, float startValue)
        {
            FillRate = fillRate;
            DrainRate = drainRate;
            StartValue = startValue;
            Value = Mathf.Clamp01(startValue);
        }

        public void Reset()
        {
            Value = Mathf.Clamp01(StartValue);
        }

        public void Tick(float dt, bool pushing)
        {
            if (dt <= 0f) return;

            if (pushing)
                Value = Mathf.Min(1f, Value + FillRate * FillRateMultiplier * dt);
            else
                Value = Mathf.Max(0f, Value - DrainRate * DrainRateMultiplier * dt);
        }

        public MeterResult Result
        {
            get
            {
                if (Value >= 1f) return MeterResult.Won;
                if (Value <= 0f) return MeterResult.Lost;
                return MeterResult.InProgress;
            }
        }

        public bool IsResolved => Result != MeterResult.InProgress;
    }
}
