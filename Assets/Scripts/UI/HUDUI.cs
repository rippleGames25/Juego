using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Security.Cryptography;

public class HUDUI : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject PauseMenuPanel;
    [SerializeField] private GameObject HUDPanel;

    [Header("Cursores")]
    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D wateringCanCursor;
    [SerializeField] private Texture2D fertilizerBagCursor;

    [Header("Clima")]
    public TextMeshProUGUI currentWeatherText;
    public List<TextMeshProUGUI> forecastDayTexts;

    [Header("Recursos")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI waterText;
    [SerializeField] private TextMeshProUGUI fertilizerText;

    [Header("Progreso")]
    [SerializeField] private TextMeshProUGUI dayText;

    private void Start()
    {
        // Actualizar UI
        UpdateMoneyText(GameManager.Instance.CurrentMoney);
        UpdateWaterText(GameManager.Instance.CurrentWater);
        UpdateFertilizerText(GameManager.Instance.CurrentFertilizer);
        UpdateDayText(GameManager.Instance.CurrentDay);

        // Subscripción a eventos

        if(GameManager.Instance!= null)
        {
            GameManager.Instance.OnMoneyChanged += UpdateMoneyText;
            GameManager.Instance.OnWaterChanged += UpdateWaterText;
            GameManager.Instance.OnFertilizerChanged += UpdateFertilizerText;
            GameManager.Instance.OnDayChanged += UpdateDayText;
            GameManager.Instance.OnToolChanged += UpdateCursor;

        }

        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.OnCurrentWeatherChanged += UpdateCurrentWeatherDisplay;
            WeatherManager.Instance.OnForecastChanged += UpdateForecastDisplay;

            UpdateCurrentWeatherDisplay(WeatherManager.Instance.currentWeather); 
            UpdateForecastDisplay(WeatherManager.Instance.forecast); 
        }
        
    }

    private void UpdateForecastDisplay(DailyWeather[] forecastArray)
    {
        for (int i = 0; i < forecastArray.Length && i < forecastDayTexts.Count; i++)
        {
            // Actualiza cada slot de la UI
            forecastDayTexts[i].text = $"Día {i + 1}: {forecastArray[i].ToString()}";
        }
    }

    private void UpdateCurrentWeatherDisplay(DailyWeather weather)
    {
        if (currentWeatherText != null)
        {
            currentWeatherText.text = $"HOY HACE: {weather.ToString()}";
            // Efectos visuales
        }
    }

    public void PauseButton()
    {
        HUDPanel.SetActive(false);
        PauseMenuPanel.SetActive(true);
    }

    public void PassDayButton()
    {
        GameManager.Instance.PassDay();
    }

    private void UpdateMoneyText(int value)
    {
        if (moneyText != null)
        {
            moneyText.text = "Dinero: " + value.ToString() + "€";
        }
    }

    private void UpdateWaterText(int value)
    {
        if (waterText != null)
        {
            waterText.text = "Agua: " + value.ToString();
        }
    }

    private void UpdateFertilizerText(int value)
    {
        if (fertilizerText != null)
        {
            fertilizerText.text = "Abono: " + value.ToString();
        }
    }

    private void UpdateDayText(int value)
    {
        if (dayText != null)
        {
            dayText.text = "Dia: " + value.ToString();
        }
    }

    private void UpdateCursor(ToolType _type)
    {
        Cursor.SetCursor(ChooseCursor(), default, default);
    }

    private Texture2D ChooseCursor()
    {
        switch (GameManager.Instance.CurrentTool)
        {
            case ToolType.WateringCan:
                return wateringCanCursor;

            case ToolType.FertilizerBag:
                return fertilizerBagCursor;

            default:
                return normalCursor;
        }
    }

    // Desuscribirse al destuir el objeto
    private void OnDestroy()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged -= UpdateMoneyText;
            GameManager.Instance.OnWaterChanged -= UpdateWaterText;
            GameManager.Instance.OnFertilizerChanged -= UpdateFertilizerText;
            GameManager.Instance.OnDayChanged -= UpdateDayText;
        }

        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.OnCurrentWeatherChanged -= UpdateCurrentWeatherDisplay;
            WeatherManager.Instance.OnForecastChanged -= UpdateForecastDisplay;
        }

    }

}
