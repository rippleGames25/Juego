using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

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
        // Obtenemos la puntuación del jugador
        int playerSuccessScore = 0;
        if (GameManager.Instance != null)
        {
            playerSuccessScore = GameManager.Instance.CurrentBiodiversity;
        }

        // Calcular la Intensidad maxima (1, 2, o 3)
        int maxIntensity;
        if (playerSuccessScore <= 3)
        {
            maxIntensity = 1; // Nivel 1: Solo intensidad 1
        }
        else if (playerSuccessScore <= 5)
        {
            maxIntensity = 2; // Nivel 2: Intensidad 1 o 2
        }
        else
        {
            maxIntensity = 3; // Nivel 3: Intensidad 1, 2 o 3
        }

        // Definir el umbral para desbloquear el granizo
        const int GRANIZO_UNLOCK_THRESHOLD = 3; // El granizo no aparecerá hasta que tengas 3 de biodiversidad

        // Crear el saco de probabilidades para la frecuencua
        List<WeatherType> weatherPool = new List<WeatherType>();

        // Eventos base (siempre presentes)
        weatherPool.Add(WeatherType.Nublado);
        weatherPool.Add(WeatherType.Lluvia);
        weatherPool.Add(WeatherType.Lluvia);
        weatherPool.Add(WeatherType.Soleado);

        // Añadir Granizo solo si el jugador ha superado el umbral
        if (playerSuccessScore >= GRANIZO_UNLOCK_THRESHOLD)
        {
            weatherPool.Add(WeatherType.Granizo); // Desbloqueado
        }

        // Añadir más eventos malos según el éxito del jugador
        int bonusBadEvents = playerSuccessScore / 2;
        for (int i = 0; i < bonusBadEvents; i++)
        {
            weatherPool.Add(WeatherType.Soleado);

            if (playerSuccessScore >= GRANIZO_UNLOCK_THRESHOLD && i % 2 == 0)
            {
                weatherPool.Add(WeatherType.Granizo);
            }
        }

        // Elegir un tipo de clima del saco
        WeatherType randomType = weatherPool[UnityEngine.Random.Range(0, weatherPool.Count)];

        // Generar la intensidad y los efectos
        int randomIntensity = UnityEngine.Random.Range(1, maxIntensity + 1);

        int waterChangeAux = 0;
        float deathProbabilityAux = 0;

        switch (randomType)
        {
            case WeatherType.Soleado:
                waterChangeAux = -randomIntensity;
                break;
            case WeatherType.Nublado:
                break;
            case WeatherType.Lluvia:
                waterChangeAux = randomIntensity * 2;
                break;
            case WeatherType.Granizo:
                waterChangeAux = randomIntensity;
                deathProbabilityAux = randomIntensity / 5f;
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
