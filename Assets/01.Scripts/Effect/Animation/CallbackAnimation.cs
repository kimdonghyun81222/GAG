namespace GrowAGarden.Effects._01.Scripts.Effect.Animation
{
    public class CallbackAnimation : BaseAnimation
    {
        private System.Action callback;
        private bool hasExecuted = false;

        public CallbackAnimation(System.Action callback) : base(0f)
        {
            this.callback = callback;
        }

        public override void UpdateAnimation(float deltaTime)
        {
            if (!hasExecuted)
            {
                callback?.Invoke();
                hasExecuted = true;
                Complete();
            }
        }

        protected override void ApplyAnimation()
        {
            // Callback animation executes immediately
        }
    }
}