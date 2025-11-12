/*
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using System.Linq;

public class HUDUIX : MonoBehaviour
{
    #region Propierties
    [Header("Paneles")]
    [SerializeField] private GameObject PauseMenuCanvas;
    [SerializeField] private GameObject HUDPanel;
    [SerializeField] private GameObject plantInfoPanel;
    [SerializeField] private GameObject plotInfoPanel;
    [SerializeField] private GameObject summaryPanel;
    [SerializeField] private GameObject settingsPanel;

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

    [Header("Info Planta")]
    [SerializeField] private Image plantPhoto;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI lifeDaysText;
    [SerializeField] private TextMeshProUGUI growthState;
    [SerializeField] private TextMeshProUGUI sunDemand;
    [SerializeField] private TextMeshProUGUI waterDemand;
    [SerializeField] private TextMeshProUGUI fertilizerDemand;
    [SerializeField] private TextMeshProUGUI health;

    [Header("Info Parcela")]
    [SerializeField] private TextMeshProUGUI plotGridInfo;
    [SerializeField] private TextMeshProUGUI plotSolarInfo;
    [SerializeField] private TextMeshProUGUI plotWaterInfo;
    [SerializeField] private TextMeshProUGUI plotFertilizerInfo;

    [Header("Panel Tipo de planta")]
    [SerializeField] private GameObject plantTypeInfoPanel;
    [SerializeField] private TextMeshProUGUI plantTypeName;

    [SerializeField] GameObject toolsRoot;
    [SerializeField] GameObject plotsGrid;


    #endregion

    private void Start()
    {
        // Ocultar paneles
        plantInfoPanel.SetActive(false);
        plotInfoPanel.SetActive(false);
        summaryPanel.SetActive(false);
        Cursor.visible = false;

        // Actualizar UI
        UpdateMoneyText(GameManager.Instance.CurrentMoney);
        UpdateWaterText(GameManager.Instance.CurrentWater);
        UpdateFertilizerText(GameManager.Instance.CurrentFertilizer);
        UpdateDayText(GameManager.Instance.CurrentDay);
        UpdateBiodiversityText(GameManager.Instance.CurrentBiodiversity);

        // Subscripción a eventos
        if(GameManager.Instance!= null)
        {
            GameManager.Instance.OnMoneyChanged += UpdateMoneyText;
            GameManager.Instance.OnWaterChanged += UpdateWaterText;
            GameManager.Instance.OnFertilizerChanged += UpdateFertilizerText;
            GameManager.Instance.OnDayChanged += UpdateDayText;
            GameManager.Instance.OnToolChanged += UpdateCursor;
            GameManager.Instance.OnBiodiversityChanged += UpdateBiodiversityText;
            GameManager.Instance.OnPlantInfoClick += ShownPlantTypeInfoPanel;
            GameManager.Instance.OnDayEnd += ShowDaySummaryPanel;
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
        SFXManager.Instance?.PlayClick();
        HUDPanel.SetActive(false);
        PauseMenuCanvas.SetActive(true);

        GameManager.Instance.SetInputLocked(true);
        SFXManager.Instance?.PauseAmbient(true);
    }

    public void SettingsButton()
    {
        SFXManager.Instance?.PlayClick();
        HUDPanel.SetActive(false);
        if (toolsRoot) toolsRoot.SetActive(false);
        if (plotsGrid) plotsGrid.SetActive(false);
        settingsPanel.SetActive(true);

        // bloquea input y PAUSA ambiente
        GameManager.Instance.SetInputLocked(true);
        SFXManager.Instance?.PauseAmbient(true);
    }

    public void SaveGameButton()
    {
        SFXManager.Instance?.PlayClick();
        int slot = PlayerPrefs.GetInt("lastSlot", 1);
        SaveManager.Instance?.Save(slot);
    }



    public void GoBackFromSettingsButton()
    {
        SFXManager.Instance?.PlayClick();
        settingsPanel.SetActive(false);
        if (toolsRoot) toolsRoot.SetActive(true);
        if (plotsGrid) plotsGrid.SetActive(true);
        HUDPanel.SetActive(true);

        // desbloquea input y REANUDA ambiente
        GameManager.Instance.SetInputLocked(false);
        SFXManager.Instance?.PauseAmbient(false);
    }

    public void PassDayButton()
    {
        SFXManager.Instance?.PlayClick();
        GameManager.Instance.EndDay();
    }


    public void NextButton()
    {
        SFXManager.Instance?.PlayClick();
        GameManager.Instance.StartNewDay();
        summaryPanel.SetActive(false);
    }

    public void ShowDaySummaryPanel()
    {
        SFXManager.Instance?.PlayClick();
        summaryPanel.SetActive(true);
    }

    public void ShownPlantTypeInfoPanel(PlantType plantType)
    {
        SFXManager.Instance?.PlayClick();
        // Poner info planta
        plantTypeName.text = plantType.plantName;

        plantTypeInfoPanel.SetActive(true);
    }

    public void UnShowPlantTypeInfoPanel()
    {
        SFXManager.Instance?.PlayClick();
        plantTypeInfoPanel.SetActive(false);
    }

    #endregion

    #region Metodos para Paneles de Info Parcelas

    // Metodo que muestra panel de información
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

    // Metodo que esconde el panel de información
    private void UnShowInfoPanel()
    {
        plotInfoPanel.SetActive(false);
        plantInfoPanel.SetActive(false);
    }

    // Metodo que muestra panel de información de la parcela
    private void ShowPlotInfoPanel(Plot plot)
    {
        plotGridInfo.text = $"Parcela {plot.gridCoordinates}";
        plotSolarInfo.text = $"{plot.currentSolarExposure}";
        plotWaterInfo.text = $"Agua: {plot.currentWater}";
        plotFertilizerInfo.text = $"Abono: {plot.currentFertility}";

        plotInfoPanel.SetActive(true);
    }

    // Metodo que muestra panel de información de la planta
    private void ShowPlantInfoPanel(Plant plant)
    {
        plantPhoto.sprite = plant.plantData.plantSprites[GameManager.IDX_PLANT_SPRITE]; // Sprite de la planta madura

        // Info
        nameText.text= plant.plantData.plantName;
        typeText.text = plant.plantData.category.ToString();

        // Estado
        lifeDaysText.text = $"Días de vida: {plant.lifeDays}";
        growthState.text = $"Crecimiento: {plant.currentGrowth}";
        health.text = $"Salud: {plant.currentHealth}";
  
        // Necesidades
        waterDemand.text = $"Agua: {plant.plantData.waterDemand}";
        fertilizerDemand.text = $"Abono: {plant.plantData.fertilizerDemand}";
        sunDemand.text = $"Exposición: {plant.plantData.solarExposureDemand}";

        plantInfoPanel.SetActive(true);
    }

    #endregion

    #region Actualización UI
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

    // Metodo que actualiza el Texto de la meteorología actual
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
            biodiversityText.text = $"Biodiversidad: {value}/{GameManager.Instance.winCondition}";
        }
    }

    #endregion

    #region Cursor

    private void UpdateCursor(ToolType _type)
    {
        // Le decimos al manager qué herramienta hemos seleccionado
        if (SoftwareCursorManager.Instance != null)
        {
            SoftwareCursorManager.Instance.SetTool(_type);
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
            GameManager.Instance.OnDayEnd -= ShowDaySummaryPanel;
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
*/