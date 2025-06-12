using UnityEngine;

namespace GrowAGarden.Effects._01.Scripts.Effect.Animation
{
    public class ScaleAnimation : BaseAnimation
    {
        private Transform target;
        private Vector3 startScale;
        private Vector3 endScale;

        public ScaleAnimation(Transform target, Vector3 startScale, Vector3 endScale, float duration) 
            : base(duration)
        {
            this.target = target;
            this.startScale = startScale;
            this.endScale = endScale;
        }

        protected override void ApplyAnimation()
        {
            if (target == null) return;
            
            Vector3 currentScale = Vector3.Lerp(startScale, endScale, NormalizedTime);
            target.localScale = currentScale;
        }

        public ScaleAnimation SetStartScale(Vector3 start)
        {
            startScale = start;
            return this;
        }

        public ScaleAnimation SetEndScale(Vector3 end)
        {
            endScale = end;
            return this;
        }
    }
}