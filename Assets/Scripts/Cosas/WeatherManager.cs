using UnityEngine;
using System;
using System.Linq;

public enum WeatherType
{
    Soleado,
    Nublado,
    Lluvia,
    Granizo
}

public class DailyWeather
{
    public WeatherType type;
    public int intensity;
    public int waterChange;
    public float deathProbability;

    public override string ToString()
    {
        return $"{type} (Intensidad: {intensity})";
    }
}

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance; // Singleton
    private const int FORECAST_LENGTH = 3;
    

    // Events
    public event Action<DailyWeather> OnCurrentWeatherChanged;
    public event Action<DailyWeather[]> OnForecastChanged;


    // Variables 
    public DailyWeather currentWeather = new DailyWeather();
    public DailyWeather[] forecast = new DailyWeather[FORECAST_LENGTH];
    public int maxIntensity = 3;

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
        PlayWeatherSFX(currentWeather);
        OnForecastChanged?.Invoke(forecast);
    }

    public DailyWeather PassDay()
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
        PlayWeatherSFX(currentWeather);
        OnForecastChanged?.Invoke(forecast);

        Debug.Log($"Clima de HOY: {currentWeather.type}. Previsión actualizada.");
        return currentWeather;
    }

    public DailyWeather GenerateWeather()
    {
        Array values = Enum.GetValues(typeof(WeatherType));
        WeatherType randomType = (WeatherType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        int randomIntensity = UnityEngine.Random.Range(1, maxIntensity);
        int waterChangeAux = 0;
        float deathProbabilityAux = 0;

        switch (randomType)
        {
            case WeatherType.Soleado:
                waterChangeAux = -randomIntensity; // Resta agua: -1, -2 o -3
                break;
            case WeatherType.Nublado:
                // No resta agua
                break;
            case WeatherType.Lluvia:
                waterChangeAux = randomIntensity*2; // Suma agua: 2, 4 o 6
                break;
            case WeatherType.Granizo:
                waterChangeAux = randomIntensity;   // Suma agua: 1, 2 o 3
                deathProbabilityAux = randomIntensity/5f; // Probablidad de muerte de plantas pequeñas y con salud baja: 1/5, 2/5, 3/5
                break;
        }



        DailyWeather weather = new DailyWeather
        {
            type = randomType,
            intensity = randomIntensity,
            waterChange = waterChangeAux,
            deathProbability = deathProbabilityAux
        };

        return weather;
    }

    private void PlayWeatherSFX(DailyWeather w)
    {
    // Detiene cualquier sonido anterior primero
    SFXManager.Instance?.StopAmbient();

    switch (w.type)
    {
        case WeatherType.Soleado:
            SFXManager.Instance?.PlaySoleado();
            break;
        case WeatherType.Lluvia:
            SFXManager.Instance?.PlayLluvia();
            break;
        case WeatherType.Granizo:
            SFXManager.Instance?.PlayNieve();
            break;
        case WeatherType.Nublado:
            break;
    }
    }



}
