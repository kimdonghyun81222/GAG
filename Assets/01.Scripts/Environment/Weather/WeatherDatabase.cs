using System.Collections.Generic;
using _01.Scripts.Environment.Weather;
using UnityEngine;

namespace GrowAGarden.Environment._01.Scripts.Environment.Weather
{
    [CreateAssetMenu(fileName = "WeatherDatabase", menuName = "GrowAGarden/Environment/Weather Database")]
    public class WeatherDatabase : ScriptableObject
    {
        [Header("Weather Collection")]
        [SerializeField] private List<WeatherData> weatherTypes = new List<WeatherData>();
        
        [Header("Season Settings")]
        [SerializeField] private List<SeasonWeatherProbability> seasonProbabilities = new List<SeasonWeatherProbability>();
        
        // Cache for quick lookup
        private Dictionary<WeatherType, WeatherData> _weatherLookup;
        private Dictionary<Season, SeasonWeatherProbability> _seasonLookup;
        
        public void Initialize()
        {
            BuildWeatherLookup();
            BuildSeasonLookup();
        }
        
        private void BuildWeatherLookup()
        {
            _weatherLookup = new Dictionary<WeatherType, WeatherData>();
            
            foreach (var weather in weatherTypes)
            {
                if (weather != null)
                {
                    _weatherLookup[weather.weatherType] = weather;
                }
            }
        }
        
        private void BuildSeasonLookup()
        {
            _seasonLookup = new Dictionary<Season, SeasonWeatherProbability>();
            
            foreach (var season in seasonProbabilities)
            {
                _seasonLookup[season.season] = season;
            }
        }
        
        public WeatherData GetWeatherData(WeatherType type)
        {
            if (_weatherLookup == null) Initialize();
            
            _weatherLookup.TryGetValue(type, out WeatherData data);
            return data;
        }
        
        public List<WeatherData> GetAllWeatherData()
        {
            return new List<WeatherData>(weatherTypes);
        }
        
        public WeatherType GetRandomWeatherForSeason(Season season)
        {
            if (_seasonLookup == null) Initialize();
            
            if (!_seasonLookup.TryGetValue(season, out SeasonWeatherProbability seasonData))
            {
                // Default to Clear weather if no season data
                return WeatherType.Clear;
            }
            
            return seasonData.GetRandomWeather();
        }
        
        public List<WeatherType> GetPossibleWeatherForSeason(Season season)
        {
            if (_seasonLookup == null) Initialize();
            
            if (!_seasonLookup.TryGetValue(season, out SeasonWeatherProbability seasonData))
            {
                return new List<WeatherType> { WeatherType.Clear };
            }
            
            return seasonData.GetPossibleWeathers();
        }
        
        public void AddWeatherData(WeatherData data)
        {
            if (data != null && !weatherTypes.Contains(data))
            {
                weatherTypes.Add(data);
                if (_weatherLookup != null)
                {
                    _weatherLookup[data.weatherType] = data;
                }
            }
        }
        
        public void RemoveWeatherData(WeatherType type)
        {
            weatherTypes.RemoveAll(w => w.weatherType == type);
            _weatherLookup?.Remove(type);
        }
        
        public bool HasWeatherData(WeatherType type)
        {
            if (_weatherLookup == null) Initialize();
            return _weatherLookup.ContainsKey(type);
        }
    }
    
    [System.Serializable]
    public class SeasonWeatherProbability
    {
        public Season season;
        [SerializeField] private List<WeatherProbability> weatherProbabilities = new List<WeatherProbability>();
        
        public WeatherType GetRandomWeather()
        {
            float totalWeight = 0f;
            foreach (var prob in weatherProbabilities)
            {
                totalWeight += prob.probability;
            }
            
            if (totalWeight <= 0f) return WeatherType.Clear;
            
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var prob in weatherProbabilities)
            {
                currentWeight += prob.probability;
                if (randomValue <= currentWeight)
                {
                    return prob.weatherType;
                }
            }
            
            return WeatherType.Clear;
        }
        
        public List<WeatherType> GetPossibleWeathers()
        {
            var result = new List<WeatherType>();
            foreach (var prob in weatherProbabilities)
            {
                if (prob.probability > 0f)
                {
                    result.Add(prob.weatherType);
                }
            }
            return result;
        }
        
        public void SetWeatherProbability(WeatherType type, float probability)
        {
            var existing = weatherProbabilities.Find(p => p.weatherType == type);
            if (existing != null)
            {
                existing.probability = Mathf.Max(0f, probability);
            }
            else
            {
                weatherProbabilities.Add(new WeatherProbability { weatherType = type, probability = Mathf.Max(0f, probability) });
            }
        }
    }
    
    [System.Serializable]
    public class WeatherProbability
    {
        public WeatherType weatherType;
        [Range(0f, 1f)] public float probability = 0.1f;
    }
    
    public enum Season
    {
        Spring,
        Summer,
        Autumn,
        Winter
    }
}