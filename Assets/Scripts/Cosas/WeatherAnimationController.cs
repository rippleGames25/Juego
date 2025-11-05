using UnityEngine;

public class WeatherAnimationController : MonoBehaviour
{
    private Animator weatherAnimator;
    private const string WEATHER_STATE_PARAM = "weatherState";
    private const string INTENSITY_PARAM = "intensity";

    void Awake()
    {
        weatherAnimator = GetComponent<Animator>();

        if (weatherAnimator == null)
        {
            Debug.LogError("No se encuentra el animator");
        }
    }

    void Start()
    {
        // Suscribirse al evento de cambio de clima
        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.OnCurrentWeatherChanged += UpdateWeatherAnimation;
        }
    }

    private void UpdateWeatherAnimation(DailyWeather _weather)
    {
        if (weatherAnimator != null)
        {
            int weatherType = (int) _weather.type;
            int intensity = (int) _weather.intensity;
            weatherAnimator.SetInteger(WEATHER_STATE_PARAM, weatherType);
            weatherAnimator.SetInteger(INTENSITY_PARAM, intensity);

            Debug.Log($"Animator: Establecido '{WEATHER_STATE_PARAM}' a: {(WeatherType)weatherType} {intensity}");
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento
        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.OnCurrentWeatherChanged -= UpdateWeatherAnimation;
        }
    }
}
