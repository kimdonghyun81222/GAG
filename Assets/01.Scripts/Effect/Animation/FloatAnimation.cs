using UnityEngine;

namespace GrowAGarden.Effects._01.Scripts.Effect.Animation
{
    public class FloatAnimation : BaseAnimation
    {
        private float startValue;
        private float endValue;
        private System.Action<float> onUpdate;

        public FloatAnimation(float startValue, float endValue, float duration, System.Action<float> onUpdate) 
            : base(duration)
        {
            this.startValue = startValue;
            this.endValue = endValue;
            this.onUpdate = onUpdate;
        }

        protected override void ApplyAnimation()
        {
            float currentValue = Mathf.Lerp(startValue, endValue, NormalizedTime);
            onUpdate?.Invoke(currentValue);
        }

        public FloatAnimation SetStartValue(float start)
        {
            startValue = start;
            return this;
        }

        public FloatAnimation SetEndValue(float end)
        {
            endValue = end;
            return this;
        }

        public FloatAnimation SetUpdateCallback(System.Action<float> callback)
        {
            onUpdate = callback;
            return this;
        }
    }
}