namespace GrowAGarden.Effects._01.Scripts.Effect.Animation
{
    public class WaitAnimation : BaseAnimation
    {
        public WaitAnimation(float duration) : base(duration)
        {
        }

        protected override void ApplyAnimation()
        {
            // Wait animation doesn't need to apply anything
        }
    }
}