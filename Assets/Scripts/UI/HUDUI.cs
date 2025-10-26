using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Linq;

public class HUDUI : MonoBehaviour
{
    #region Propierties
    [Header("Paneles")]
    [SerializeField] private GameObject PauseMenuPanel;
    [SerializeField] private GameObject HUDPanel;
    [SerializeField] private GameObject plantInfoPanel;
    [SerializeField] private GameObject plotInfoPanel;
    

    [Header("Cursores")]
    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D wateringCanCursor;
    [SerializeField] private Texture2D fertilizerBagCursor;
    [SerializeField] private Texture2D seedCursor;
    [SerializeField] private Texture2D shovelCursor;

    [Header("Clima")]
    [SerializeField] private TextMeshProUGUI currentWeatherText;
    [SerializeField] private List<Image> forecastImages = new List<Image>();
    [SerializeField] private List<Sprite> forecastSprites = new List<Sprite>();

    [Header("Recursos")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI waterText;
    [SerializeField] private TextMeshProUGUI fertilizerText;

    [Header("Progreso")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI biodiversityText;

    [Header("Panel Info")]
    [SerializeField] private Image plantPhoto;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI growthState;
    [SerializeField] private TextMeshProUGUI health;
    [SerializeField] private TextMeshProUGUI resourcesDemand;
    [SerializeField] private TextMeshProUGUI plotInfo;

    [Header("Panel Tipo de planta")]
    [SerializeField] private GameObject plantTypeInfoPanel;
    [SerializeField] private TextMeshProUGUI plantTypeName;


    #endregion

    private void Start()
    {
        // Ocultar paneles
        plantInfoPanel.SetActive(false);
        plotInfoPanel.SetActive(false);

        // Actualizar UI
        UpdateMoneyText(GameManager.Instance.CurrentMoney);
        UpdateWaterText(GameManager.Instance.CurrentWater);
        UpdateFertilizerText(GameManager.Instance.CurrentFertilizer);
        UpdateDayText(GameManager.Instance.CurrentDay);
        UpdateBiodiversityText(GameManager.Instance.CurrentBiodiversity);

        // Subscripci�n a eventos
        if(GameManager.Instance!= null)
        {
            GameManager.Instance.OnMoneyChanged += UpdateMoneyText;
            GameManager.Instance.OnWaterChanged += UpdateWaterText;
            GameManager.Instance.OnFertilizerChanged += UpdateFertilizerText;
            GameManager.Instance.OnDayChanged += UpdateDayText;
            GameManager.Instance.OnToolChanged += UpdateCursor;
            GameManager.Instance.OnBiodiversityChanged += UpdateBiodiversityText;
            GameManager.Instance.OnPlantInfoClick += ShownPlantTypeInfoPanel;
            PlotsManager.Instance.OnPlotSelected += ShowInfoPanel;
            PlotsManager.Instance.OnPlotUnselected += UnShowInfoPanel;

        }

        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.OnCurrentWeatherChanged += UpdateCurrentWeatherDisplay;
            WeatherManager.Instance.OnForecastChanged += UpdateForecastDisplay;

            UpdateCurrentWeatherDisplay(WeatherManager.Instance.currentWeather); 
            UpdateForecastDisplay(WeatherManager.Instance.forecast); 
        } 
    }


    #region Metodos para botones
    public void PauseButton()
    {
        HUDPanel.SetActive(false);
        PauseMenuPanel.SetActive(true);
    }

    public void PassDayButton()
    {
        GameManager.Instance.PassDay();
    }

    public void ShownPlantTypeInfoPanel(PlantType plantType)
    {
        // Poner info planta
        plantTypeName.text = plantType.plantName;

        plantTypeInfoPanel.SetActive(true);
    }

    public void UnShowPlantTypeInfoPanel()
    {
        plantTypeInfoPanel.SetActive(false);
    }

    #endregion

    #region Metodos para Paneles de Info Parcelas

    // Metodo que muestra panel de informaci�n
    private void ShowInfoPanel(Plot plot)
    {
        ShowPlotInfoPanel(plot);

        if (plot.isPlanted)
        {
            ShowPlantInfoPanel(plot.currentPlant);
        } else
        {
            plantInfoPanel.SetActive(false);
        }
    }

    // Metodo que esconde el panel de informaci�n
    private void UnShowInfoPanel()
    {
        plotInfoPanel.SetActive(false);
        plantInfoPanel.SetActive(false);
    }

    // Metodo que muestra panel de informaci�n de la parcela
    private void ShowPlotInfoPanel(Plot plot)
    {
        plotInfo.text = $"Parcela {plot.gridCoordinates}\n" +
            $"Agua: {plot.currentWater}\n" +
            $"Abono: {plot.currentFertility}\n";

        plotInfoPanel.SetActive(true);
    }

    // Metodo que muestra panel de informaci�n de la planta
    private void ShowPlantInfoPanel(Plant plant)
    {
        nameText.text= plant.plantData.plantName;
        plantPhoto.sprite = plant.plantData.plantSprites.Last(); // Sprite de la planta estado: madura

        growthState.text = $"Estado: {plant.currentGrowth}";
        health.text = $"Salud: {plant.currentHealth}";
        resourcesDemand.text = $"NECESIDADES\n" +
            $"Agua: {plant.plantData.waterDemand}\n" +
            $"Abono: {plant.plantData.fertilizerDemand} \n" +
            $"Exposici�n Solar: {plant.plantData.solarExposureDemand} \n";

        plantInfoPanel.SetActive(true);
    }

    #endregion

    #region Actualizaci�n UI
    // Metodo que actualiza el panel de la prevision del tiempo
    private void UpdateForecastDisplay(DailyWeather[] forecastArray)
    {
        for (int i = 0; i < forecastArray.Length && i < forecastImages.Count; i++)
        {
            DailyWeather weather = forecastArray[i];
            int idx = ((int)weather.type * WeatherManager.Instance.maxIntensity) + (weather.intensity - 1);

            forecastImages[i].sprite = forecastSprites[idx]; // Actualizamos la imagen
        }
    }

    // Metodo que actualiza el Texto de la meteorolog�a actual
    private void UpdateCurrentWeatherDisplay(DailyWeather weather)
    {
        if (currentWeatherText != null)
        {
            currentWeatherText.text = $"HOY: {weather.ToString()}";
            // Efectos visuales
        }
    }

    private void UpdateMoneyText(int value)
    {
        if (moneyText != null)
        {
            moneyText.text = value.ToString();
        }
    }

    private void UpdateWaterText(int value)
    {
        if (waterText != null)
        {
            waterText.text = value.ToString();
        }
    }

    private void UpdateFertilizerText(int value)
    {
        if (fertilizerText != null)
        {
            fertilizerText.text = value.ToString();
        }
    }

    private void UpdateDayText(int value)
    {
        if (dayText != null)
        {
            dayText.text = "Dia: " + value.ToString();
        }
    }

    private void UpdateBiodiversityText(int value)
    {
        if(biodiversityText != null)
        {
            biodiversityText.text = "Biodiversidad: " + value.ToString();
        }
    }

    #endregion

    #region Cursor
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

            case ToolType.Plant:
                return seedCursor;

            case ToolType.Shovel:
                return shovelCursor;

            default:
                return normalCursor;
        }
    }

    #endregion


    // Desuscribirse a los metodos al destuir el objeto
    private void OnDestroy()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged -= UpdateMoneyText;
            GameManager.Instance.OnWaterChanged -= UpdateWaterText;
            GameManager.Instance.OnFertilizerChanged -= UpdateFertilizerText;
            GameManager.Instance.OnDayChanged -= UpdateDayText;
            GameManager.Instance.OnToolChanged -= UpdateCursor;
            GameManager.Instance.OnBiodiversityChanged -= UpdateBiodiversityText;
            GameManager.Instance.OnPlantInfoClick -= ShownPlantTypeInfoPanel;
            PlotsManager.Instance.OnPlotSelected -= ShowInfoPanel;
            PlotsManager.Instance.OnPlotUnselected -= UnShowInfoPanel;
        }

        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.OnCurrentWeatherChanged -= UpdateCurrentWeatherDisplay;
            WeatherManager.Instance.OnForecastChanged -= UpdateForecastDisplay;
        }
    }
}
