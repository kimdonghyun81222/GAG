using UnityEngine;

namespace GrowAGarden.Effects._01.Scripts.Effect.Animation
{
    public class PositionAnimation : BaseAnimation
    {
        private Transform target;
        private Vector3 startPosition;
        private Vector3 endPosition;
        private bool useLocalPosition;

        public PositionAnimation(Transform target, Vector3 startPosition, Vector3 endPosition, float duration, bool useLocal = false) 
            : base(duration)
        {
            this.target = target;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.useLocalPosition = useLocal;
        }

        protected override void ApplyAnimation()
        {
            if (target == null) return;
            
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, NormalizedTime);
            
            if (useLocalPosition)
            {
                target.localPosition = currentPosition;
            }
            else
            {
                target.position = currentPosition;
            }
        }

        public PositionAnimation SetLocalPosition(bool useLocal)
        {
            useLocalPosition = useLocal;
            return this;
        }

        public PositionAnimation SetStartPosition(Vector3 start)
        {
            startPosition = start;
            return this;
        }

        public PositionAnimation SetEndPosition(Vector3 end)
        {
            endPosition = end;
            return this;
        }
    }
}