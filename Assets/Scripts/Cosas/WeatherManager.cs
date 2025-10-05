using UnityEngine;
using System;
using System.Linq;

public enum WeatherType
{
    Sunny,
    PartlySunny,
    Rainy,
    Stormy,
    Hailing
}

public class DailyWeather
{
    public WeatherType type;
    public int intensity;
    public int waterChange;
    public int deathProbability;

    public override string ToString()
    {
        return $"{type} (Intensidad: {intensity}";
    }
}

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance; // Singleton
    private const int FORECAST_LENGTH = 3;

    // Events
    public event Action<DailyWeather> OnCurrentWeatherChanged;
    public event Action<DailyWeather[]> OnForecastChanged;


    // Variables (CAMBIAR A PRIVATE)
    public DailyWeather currentWeather = new DailyWeather();
    public DailyWeather[] forecast = new DailyWeather[FORECAST_LENGTH];

    void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
        InitializeWeather();
    }

    private void InitializeWeather()
    {
        currentWeather = GenerateWeather();
        for(int i=0; i<FORECAST_LENGTH; i++)
        {
            forecast[i] = GenerateWeather();
        }

        OnCurrentWeatherChanged?.Invoke(currentWeather);
        OnForecastChanged?.Invoke(forecast);
    }

    public void PassDay()
    {
        // Actualizamos el clima actual
        currentWeather = forecast[0];

        // Desplazamos la prevision
        for (int i = 0; i < FORECAST_LENGTH - 1; i++)
        {
            forecast[i] = forecast[i + 1];
        }

        // Genera nuevo dia
        forecast[FORECAST_LENGTH - 1] = GenerateWeather();

        // Notifica cambios
        OnCurrentWeatherChanged?.Invoke(currentWeather);
        OnForecastChanged?.Invoke(forecast);

        Debug.Log($"Clima de HOY: {currentWeather.type}. Previsión actualizada.");
    }

    public DailyWeather GenerateWeather()
    {
        Array values = Enum.GetValues(typeof(WeatherType));
        WeatherType randomType = (WeatherType)values.GetValue(UnityEngine.Random.Range(0, values.Length));

        DailyWeather weather = new DailyWeather
        {
            type = randomType,
            intensity = UnityEngine.Random.Range(1, 4),
            waterChange = (randomType == WeatherType.Rainy) ? 2 : -2,
            deathProbability = (randomType == WeatherType.Hailing) ? 10 : 0
        };

        return weather;
    }
    


}
