using UnityEngine;

namespace GrowAGarden.Effects._01.Scripts.Effect.Animation
{
    public class ColorAnimation : BaseAnimation
    {
        private Renderer targetRenderer;
        private Material targetMaterial;
        private Color startColor;
        private Color endColor;
        private string colorProperty;

        public ColorAnimation(Renderer target, Color startColor, Color endColor, float duration, string property = "_Color") 
            : base(duration)
        {
            this.targetRenderer = target;
            this.targetMaterial = target != null ? target.material : null;
            this.startColor = startColor;
            this.endColor = endColor;
            this.colorProperty = property;
        }

        protected override void ApplyAnimation()
        {
            if (targetMaterial == null) return;
            
            Color currentColor = Color.Lerp(startColor, endColor, NormalizedTime);
            targetMaterial.SetColor(colorProperty, currentColor);
        }

        public ColorAnimation SetColorProperty(string property)
        {
            colorProperty = property;
            return this;
        }

        public ColorAnimation SetStartColor(Color start)
        {
            startColor = start;
            return this;
        }

        public ColorAnimation SetEndColor(Color end)
        {
            endColor = end;
            return this;
        }
    }
}