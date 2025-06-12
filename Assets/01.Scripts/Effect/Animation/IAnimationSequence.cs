namespace GrowAGarden.Effects._01.Scripts.Effect.Animation
{
    public interface IAnimationSequence
    {
        bool IsPlaying { get; }
        bool IsPaused { get; }
        bool IsCompleted { get; }
        float Progress { get; }
        
        void Play();
        void Pause();
        void Resume();
        void Stop();
        void Complete();
        void UpdateAnimation(float deltaTime);
    }
}