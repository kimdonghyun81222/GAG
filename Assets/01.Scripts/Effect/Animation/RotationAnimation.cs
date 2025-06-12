using UnityEngine;

namespace GrowAGarden.Effects._01.Scripts.Effect.Animation
{
    public class RotationAnimation : BaseAnimation
    {
        private Transform target;
        private Quaternion startRotation;
        private Quaternion endRotation;
        private bool useLocalRotation;

        public RotationAnimation(Transform target, Quaternion startRotation, Quaternion endRotation, float duration, bool useLocal = false) 
            : base(duration)
        {
            this.target = target;
            this.startRotation = startRotation;
            this.endRotation = endRotation;
            this.useLocalRotation = useLocal;
        }

        protected override void ApplyAnimation()
        {
            if (target == null) return;
            
            Quaternion currentRotation = Quaternion.Lerp(startRotation, endRotation, NormalizedTime);
            
            if (useLocalRotation)
            {
                target.localRotation = currentRotation;
            }
            else
            {
                target.rotation = currentRotation;
            }
        }

        public RotationAnimation SetLocalRotation(bool useLocal)
        {
            useLocalRotation = useLocal;
            return this;
        }

        public RotationAnimation SetStartRotation(Quaternion start)
        {
            startRotation = start;
            return this;
        }

        public RotationAnimation SetEndRotation(Quaternion end)
        {
            endRotation = end;
            return this;
        }
    }
}