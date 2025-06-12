using UnityEngine;

namespace _01.Scripts.Environment.Weather
{
    [CreateAssetMenu(fileName = "WeatherData", menuName = "GrowAGarden/Environment/Weather Data")]
    public class WeatherData : ScriptableObject
    {
        [Header("Weather Information")]
        public string weatherName;
        public WeatherType weatherType;
        public Sprite weatherIcon;
        public Color skyColor = Color.blue;
        public Color fogColor = Color.white;
        
        [Header("Environment Settings")]
        [Range(0f, 1f)] public float cloudCoverage = 0.5f;
        [Range(0f, 1f)] public float precipitation = 0f;
        [Range(0f, 1f)] public float windStrength = 0.3f;
        [Range(0f, 1f)] public float humidity = 0.5f;
        [Range(-30f, 50f)] public float temperature = 20f;
        
        [Header("Lighting")]
        [Range(0f, 2f)] public float sunIntensity = 1f;
        [Range(0f, 1f)] public float ambientIntensity = 0.5f;
        public Color sunColor = Color.white;
        public Color ambientColor = Color.gray;
        
        [Header("Fog Settings")]
        public bool enableFog = false;
        [Range(0f, 1f)] public float fogDensity = 0.1f;
        public float fogStartDistance = 10f;
        public float fogEndDistance = 100f;
        
        [Header("Particle Effects")]
        public GameObject rainParticles;
        public GameObject snowParticles;
        public GameObject fogParticles;
        public GameObject dustParticles;
        
        [Header("Audio")]
        public AudioClip ambientSound;
        [Range(0f, 1f)] public float ambientVolume = 0.5f;
        
        [Header("Transition")]
        public float transitionDuration = 5f;
        public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("Gameplay Effects")]
        public float cropGrowthMultiplier = 1f;
        public float visibilityMultiplier = 1f;
        public float movementSpeedMultiplier = 1f;
        
        // Validation
        private void OnValidate()
        {
            weatherName = string.IsNullOrEmpty(weatherName) ? weatherType.ToString() : weatherName;
            temperature = Mathf.Clamp(temperature, -30f, 50f);
            transitionDuration = Mathf.Max(0.1f, transitionDuration);
        }
        
        // Utility methods
        public bool IsRaining() => weatherType == WeatherType.Rain || weatherType == WeatherType.Storm;
        public bool IsSnowing() => weatherType == WeatherType.Snow || weatherType == WeatherType.Blizzard;
        public bool IsCloudy() => cloudCoverage > 0.7f;
        public bool IsFoggy() => enableFog && fogDensity > 0.3f;
        public bool IsWindy() => windStrength > 0.6f;
        
        public string GetTemperatureString() => $"{temperature:F1}°C";
        public string GetHumidityString() => $"{humidity * 100:F0}%";
        public string GetWindString() => windStrength < 0.3f ? "Calm" : windStrength < 0.6f ? "Moderate" : "Strong";
    }
    
    public enum WeatherType
    {
        Clear,
        PartlyCloudy,
        Cloudy,
        Overcast,
        Rain,
        Storm,
        Snow,
        Blizzard,
        Fog,
        Dust
    }
}