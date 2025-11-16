using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;              // ← para las corutinas
using System.Collections.Generic;
using System;
using System.Linq;

public class HUDUI : MonoBehaviour
{
    #region Propierties
    [Header("Paneles")]
    [SerializeField] private GameObject PauseMenuCanvas;
    [SerializeField] private GameObject HUDPanel;
    [SerializeField] private GameObject plantInfoPanel;
    [SerializeField] private GameObject plotInfoPanel;
    [SerializeField] private GameObject summaryPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject helpPanel;

    // Panel del Dia
    [Header("Resumen Día")]
    [SerializeField] private TextMeshProUGUI summaryBaseIncomeText;
    [SerializeField] private TextMeshProUGUI summaryQuantityBonusText;
    [SerializeField] private TextMeshProUGUI summaryMaturityBonusText;
    [SerializeField] private TextMeshProUGUI summaryDiversityBonusText;
    [SerializeField] private TextMeshProUGUI summarySolarBonusText;
    [SerializeField] private TextMeshProUGUI summaryDeathPenaltyText;
    [SerializeField] private TextMeshProUGUI summaryBailoutText;
    [SerializeField] private TextMeshProUGUI summaryTotalIncomeText;
    [SerializeField] private TextMeshProUGUI summaryWaterIncomeText;
    [SerializeField] private TextMeshProUGUI summaryFertilizerIncomeText;

    // Sistema de Strikes
    [Header("Strikes")]
    [SerializeField] private Image[] strikeIcons; // Array de 5 imágenes
    [SerializeField] private Color strikeColor_Normal = Color.yellow;
    [SerializeField] private Color strikeColor_Permanent = Color.red;

    [Header("Clima")]
    [SerializeField] private TextMeshProUGUI currentWeatherText;
    [SerializeField] private List<TextMeshProUGUI> forecastText = new List<TextMeshProUGUI>();
    [SerializeField] private List<Image> forecastImages = new List<Image>();
    [SerializeField] private List<Sprite> forecastSprites = new List<Sprite>();
    [SerializeField] private List<Image> forecastIntensity = new List<Image>();
    [SerializeField] private List<Sprite> intensitySprites = new List<Sprite>();

    [Header("Recursos")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI waterText;
    [SerializeField] private TextMeshProUGUI fertilizerText;

    [Header("Progreso")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI biodiversityText;
    [SerializeField] private Image currentWeather;
    [SerializeField] private Image currentIntensity;

    [Header("Info Planta")]
    [SerializeField] private Image plantPhoto;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI lifeDaysText;
    [SerializeField] private Image growthState;
    [SerializeField] private Image sunDemand;
    [SerializeField] private TextMeshProUGUI waterDemand;
    [SerializeField] private TextMeshProUGUI fertilizerDemand;
    [SerializeField] private Image health;
    [SerializeField] private List<Sprite> healthSprites;
    [SerializeField] private List<Sprite> growthStateSprites;
    [SerializeField] private List<Sprite> solarSprites;


    [Header("Info Parcela")]
    [SerializeField] private TextMeshProUGUI plotGridInfo;
    [SerializeField] private Image plotSolarInfo;
    [SerializeField] private TextMeshProUGUI plotWaterInfo;
    [SerializeField] private TextMeshProUGUI plotFertilizerInfo;

    [Header("Panel Tipo de planta")]
    [SerializeField] private GameObject plantTypeInfoPanel;
    [SerializeField] private TextMeshProUGUI plantTypeName;
    [SerializeField] private TextMeshProUGUI plantTypeScientificName;
    [SerializeField] private TextMeshProUGUI plantTypeDescription;
    [SerializeField] private Image plantTypePhoto;
    [SerializeField] private TextMeshProUGUI plantTypeCategoryText;
    [SerializeField] private TextMeshProUGUI plantTypeWaterDemandText;
    [SerializeField] private TextMeshProUGUI plantTypeFertilizerDemandText;
    [SerializeField] private TextMeshProUGUI plantTypeToSprout;
    [SerializeField] private TextMeshProUGUI plantTypeToGrow;
    [SerializeField] private TextMeshProUGUI plantTypeToMature;
    [SerializeField] private Image plantTypeSolarDemandImage;

    [Header("Transición de Día")]
    [SerializeField] private Image dayTransitionPanel;      // panel blanco que tapa todo
    [SerializeField] private TMP_Text dayTransitionText;    // texto "Día X"
    [SerializeField] private float dayFadeDuration = 0.4f;  // tiempo de fade in/out
    [SerializeField] private float dayHoldDuration = 0.3f;  // tiempo en blanco

    [SerializeField] GameObject toolsRoot;
    [SerializeField] GameObject plotsGrid;
    private Plot currentlySelectedPlot;
    #endregion

    private void Start()
    {
        // Ocultar paneles
        plantInfoPanel.SetActive(false);
        plotInfoPanel.SetActive(false);
        summaryPanel.SetActive(false);

        // Actualizar UI
        UpdateMoneyText(GameManager.Instance.CurrentMoney);
        UpdateWaterText(GameManager.Instance.CurrentWater);
        UpdateFertilizerText(GameManager.Instance.CurrentFertilizer);
        UpdateDayText(GameManager.Instance.CurrentDay);
        UpdateBiodiversityText(GameManager.Instance.currentBiodiversity);

        // Subscripción a eventos
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged += UpdateMoneyText;
            GameManager.Instance.OnWaterChanged += UpdateWaterText;
            GameManager.Instance.OnFertilizerChanged += UpdateFertilizerText;
            GameManager.Instance.OnDayChanged += UpdateDayText;
            GameManager.Instance.OnToolChanged += UpdateCursor;
            GameManager.Instance.OnBiodiversityChanged += UpdateBiodiversityText;
            GameManager.Instance.OnPlantInfoClick += ShownPlantTypeInfoPanel;
            GameManager.Instance.OnDayEnd += ShowDaySummaryPanel;

            // Suscripción a Strikes
            GameManager.Instance.OnStrikesChanged += UpdateStrikesVisuals;
        }

        if (PlotsManager.Instance != null)
        {
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

        // Inicializar transición de día (invisible)
        if (dayTransitionPanel != null)
        {
            var c = dayTransitionPanel.color;
            c.a = 0f;
            dayTransitionPanel.color = c;
            dayTransitionPanel.gameObject.SetActive(false);
        }

        if (dayTransitionText != null)
        {
            var tc = dayTransitionText.color;
            tc.a = 0f;
            dayTransitionText.color = tc;
            dayTransitionText.gameObject.SetActive(false);
        }
    }

    #region Metodos para botones
    public void PauseButton()
    {
        SFXManager.Instance?.PlayClick();
        SFXManager.Instance?.PauseAmbient(true);
        HUDPanel.SetActive(false);
        PauseMenuCanvas.SetActive(true);
        GameManager.Instance.SetInputLocked(true);
    }

    public void SettingsButton()
    {
        SFXManager.Instance?.PlayClick();
        SFXManager.Instance?.PauseAmbient(true);
        HUDPanel.SetActive(false);
        if (toolsRoot) toolsRoot.SetActive(false);
        settingsPanel.SetActive(true);
        GameManager.Instance.SetInputLocked(true);
    }

    public void GoBackFromSettingsButton()
    {
        SFXManager.Instance?.PlayClick();
        SFXManager.Instance?.PauseAmbient(false);
        settingsPanel.SetActive(false);
        if (toolsRoot) toolsRoot.SetActive(true);
        HUDPanel.SetActive(true);
        GameManager.Instance.SetInputLocked(false);
    }

    public void PassDayButton()
    {
        SFXManager.Instance?.PlayClick();
        GameManager.Instance.EndDay();
    }

    public void NextButton()
    {
        SFXManager.Instance?.PlayClick();
        StartCoroutine(DayTransitionRoutine());
    }

    public void HelpButton()
    {
        SFXManager.Instance?.PlayClick();
        SFXManager.Instance?.PauseAmbient(true);
        HUDPanel.SetActive(false);
        if (toolsRoot) toolsRoot.SetActive(false);
        helpPanel.SetActive(true);
        GameManager.Instance.SetInputLocked(true);
    }

    public void GoBackFromHelpButton()
    {
        SFXManager.Instance?.PlayClick();
        SFXManager.Instance?.PauseAmbient(false);
        helpPanel.SetActive(false);
        if (toolsRoot) toolsRoot.SetActive(true);
        HUDPanel.SetActive(true);
        GameManager.Instance.SetInputLocked(false);
    }

    public void ShowDaySummaryPanel()
    {
        SFXManager.Instance?.PlayClick();

        if (GameManager.Instance == null)
        {
            Debug.LogError("HUDUI: No se puede mostrar resumen, GameManager no encontrado.");
            return;
        }

        // Coger los valores calculados del GameManager
        int baseIncome = GameManager.Instance.lastDayBaseIncome;
        int plantBonus = GameManager.Instance.lastDayPlantBonus;
        DailyBonusData bonusData = GameManager.Instance.lastDayBonusData;
        int penalties = GameManager.Instance.lastDayPenalties;

        int waterIncome = GameManager.Instance.lastDayWaterIncome;
        int fertilizerIncome = GameManager.Instance.lastDayFertilizerIncome;

        bool bailoutIsPending = GameManager.Instance.IsBailoutPending;

        // Asignar textos
        if (summaryBaseIncomeText) summaryBaseIncomeText.text = $"Ingreso base: +{baseIncome}";
        if (summaryQuantityBonusText) summaryQuantityBonusText.text = $"Bono por Cantidad: +{plantBonus}";
        if (summaryMaturityBonusText) summaryMaturityBonusText.text = $"Bono de Madurez: +{bonusData.madurityBonus}";
        if (summaryDiversityBonusText) summaryDiversityBonusText.text = $"Bono de Biodiversidad: +{bonusData.diversityBonus}";
        if (summarySolarBonusText) summarySolarBonusText.text = $"Bono Exposición Solar: +{bonusData.solarExposureBonus}";
        if (summaryDeathPenaltyText) summaryDeathPenaltyText.text = $"Plantas Muertas: -{penalties}";

        // Calcular y asignar total
        int total = baseIncome + plantBonus + bonusData.madurityBonus + bonusData.diversityBonus + bonusData.solarExposureBonus - penalties;

        if (summaryTotalIncomeText) summaryTotalIncomeText.text = $"Total: +{total}";

        if (summaryWaterIncomeText) summaryWaterIncomeText.text = $"Agua Obtenida: {waterIncome}";
        if (summaryFertilizerIncomeText) summaryFertilizerIncomeText.text = $"Abono Obtenido: {fertilizerIncome}";
        if (summaryBailoutText)
        {
            if (bailoutIsPending)
            {
                summaryBailoutText.text = "¡Aviso grave!\nSe te otorgarán 3 pétalos para el siguiente día";
                summaryBailoutText.gameObject.SetActive(true);
            }
            else
            {
                summaryBailoutText.text = "";
                summaryBailoutText.gameObject.SetActive(false);
            }
        }

        summaryPanel.SetActive(true);
    }

    public void OnDynamicInfoPanel_InfoButtonClick()
    {
        if (currentlySelectedPlot != null && currentlySelectedPlot.isPlanted)
        {
            PlantType type = currentlySelectedPlot.currentPlant.plantData;
            ShownPlantTypeInfoPanel(type);
        }
    }

    public void ShownPlantTypeInfoPanel(PlantType plantType)
    {
        if (plantType == null) return;
        SFXManager.Instance?.PlayClick();

        plantTypeName.text = plantType.plantName;

        if (plantTypeScientificName != null)
            plantTypeScientificName.text = plantType.scientificName; //

        if (plantTypeDescription != null)
            plantTypeDescription.text = plantType.description; //

        if (plantTypePhoto != null && plantType.plantSprites != null && plantType.plantSprites.Length > GameManager.IDX_PLANT_SPRITE)
        {
            // Usamos la misma imagen que en la tienda
            plantTypePhoto.sprite = plantType.plantSprites[GameManager.IDX_PLANT_SPRITE]; //
        }

        if (plantTypeCategoryText != null)
            plantTypeCategoryText.text = plantType.category.ToString();

        if (plantTypeWaterDemandText != null)
            plantTypeWaterDemandText.text = $"Agua necesaria: {plantType.waterDemand.ToString()}";

        if (plantTypeFertilizerDemandText != null)
            plantTypeFertilizerDemandText.text = $"Abono necesario: {plantType.fertilizerDemand.ToString()}";

        if (plantTypeToSprout != null)
            plantTypeToSprout.text = $"Tiempo para brotar: {plantType.timeToSprout.ToString()}";

        if (plantTypeToGrow != null)
            plantTypeToGrow.text = $"Tiempo para crecer: {plantType.timeToGrow.ToString()}";

        if (plantTypeToMature != null)
            plantTypeToMature.text = $"Tiempo para madurar: {plantType.timeToMature.ToString()}";

        // Para la imagen del sol, usamos la lista 'solarSprites' que ya tienes en HUDUI
        if (plantTypeSolarDemandImage != null && solarSprites != null && solarSprites.Count > 0)
        {
            int solarIndex = (int)plantType.solarExposureDemand;
            if (solarIndex < solarSprites.Count)
            {
                plantTypeSolarDemandImage.sprite = solarSprites[solarIndex];
            }
        }

        if (plantInfoPanel.activeSelf)
        {
            plantInfoPanel.SetActive(false);
        }
        if (plotInfoPanel.activeSelf)
        {
            plotInfoPanel.SetActive(false);
        }

        // Muestra el panel de enciclopedia
        plantTypeInfoPanel.SetActive(true);

        GameManager.Instance.SetInputLocked(true);
    }

    public void UnShowPlantTypeInfoPanel()
    {
        SFXManager.Instance?.PlayClick();
        plantTypeInfoPanel.SetActive(false);
        GameManager.Instance.SetInputLocked(false);
    }
    #endregion

    #region Metodos para Paneles de Info Parcelas

    private void ShowInfoPanel(Plot plot)
    {
        if (currentlySelectedPlot != null)
        {
            currentlySelectedPlot.OnPlotDataUpdated -= UpdatePlotInfoPanelText;
        }

        currentlySelectedPlot = plot;
        currentlySelectedPlot.OnPlotDataUpdated += UpdatePlotInfoPanelText;

        ShowPlotInfoPanel(plot);
        if (plot.isPlanted)
        {
            ShowPlantInfoPanel(plot.currentPlant);
        }
        else
        {
            plantInfoPanel.SetActive(false);
        }
    }

    private void UnShowInfoPanel()
    {
        if (currentlySelectedPlot != null)
        {
            currentlySelectedPlot.OnPlotDataUpdated -= UpdatePlotInfoPanelText;
            currentlySelectedPlot = null;
        }

        plotInfoPanel.SetActive(false);
        plantInfoPanel.SetActive(false);
    }

    private void UpdatePlotInfoPanelText(Plot plot)
    {
        // Comprobamos que el panel siga abierto y que la info sea del plot correcto
        if (plotInfoPanel.activeSelf && plot == currentlySelectedPlot)
        {
            plotWaterInfo.text = $" {plot.currentWater}";
            plotFertilizerInfo.text = $"{plot.currentFertility}";
        }
    }

    private void ShowPlotInfoPanel(Plot plot)
    {
        plotGridInfo.text = $"Parcela {plot.gridCoordinates}";
        plotSolarInfo.sprite = solarSprites[(int)plot.currentSolarExposure];
        plotWaterInfo.text = $"{plot.currentWater}";
        plotFertilizerInfo.text = $"{plot.currentFertility}";
        plotInfoPanel.SetActive(true);
    }

    private void ShowPlantInfoPanel(Plant plant)
    {
        plantPhoto.sprite = plant.plantData.plantSprites[GameManager.IDX_PLANT_SPRITE];

        // Info
        nameText.text = plant.plantData.plantName;
        typeText.text = plant.plantData.category.ToString();

        // Estado
        lifeDaysText.text = $"{plant.lifeDays}";

        switch (plant.currentGrowth)
        {
            case GrowthState.semilla:
                growthState.sprite = growthStateSprites[0];
                break;
            case GrowthState.brote:
                growthState.sprite = growthStateSprites[1];
                break;
            case GrowthState.joven:
                growthState.sprite = growthStateSprites[1];
                break;
            case GrowthState.madura:
                growthState.sprite = growthStateSprites[2];
                break;

        }
        health.sprite = healthSprites[(int)plant.currentHealth];

        // Necesidades
        waterDemand.text = $"{plant.plantData.waterDemand}";
        fertilizerDemand.text = $"{plant.plantData.fertilizerDemand}";
        sunDemand.sprite = solarSprites[(int) plant.plantData.solarExposureDemand];

        plantInfoPanel.SetActive(true);
    }
    #endregion

    #region Strikes UI
    private void UpdateStrikesVisuals(int normalStrikes, int permanentStrikes)
    {
        if (strikeIcons == null) return;

        int totalStrikes = normalStrikes + permanentStrikes;

        for (int i = 0; i < strikeIcons.Length; i++)
        {
            if (i < permanentStrikes)
            {
                strikeIcons[i].enabled = true;
                strikeIcons[i].color = strikeColor_Permanent;
            }
            else if (i < totalStrikes)
            {
                strikeIcons[i].enabled = true;
                strikeIcons[i].color = strikeColor_Normal;
            }
            else
            {
                strikeIcons[i].enabled = false;
            }
        }
    }
    #endregion

    #region Actualización UI
    private void UpdateForecastDisplay(DailyWeather[] forecastArray)
    {
        int currentDay = GameManager.Instance.CurrentDay;

        for (int i = 0; i < forecastText.Count; i++)
        {
            forecastText[i].text = $"Día {currentDay + i + 1}";
        }

        for (int i = 0; i < forecastArray.Length && i < forecastImages.Count; i++)
        {
            DailyWeather weather = forecastArray[i];
            int idx = ((int)weather.type * WeatherManager.Instance.maxIntensity) + (weather.intensity - 1);
            forecastImages[i].sprite = forecastSprites[idx];
            forecastIntensity[i].sprite = intensitySprites[i];
        }
    }

    private void UpdateCurrentWeatherDisplay(DailyWeather weather)
    {
        if (currentWeather != null)
        {
            int idx = ((int)weather.type * WeatherManager.Instance.maxIntensity) + (weather.intensity - 1);
            currentWeather.sprite = forecastSprites[idx];

            currentIntensity.sprite = intensitySprites[(int)weather.intensity-1];
        }
    }

    private void UpdateMoneyText(int value)
    {
        if (moneyText != null) moneyText.text = value.ToString();
    }
    private void UpdateWaterText(int value)
    {
        if (waterText != null) waterText.text = value.ToString();
    }
    private void UpdateFertilizerText(int value)
    {
        if (fertilizerText != null) fertilizerText.text = value.ToString();
    }
    private void UpdateDayText(int value)
    {
        if (dayText != null) dayText.text = "Dia: " + value.ToString();
    }
    private void UpdateBiodiversityText(int value)
    {
        if (biodiversityText != null) biodiversityText.text = $"{value}/{GameManager.Instance.winCondition}";
    }
    #endregion

    #region Cursor
    private void UpdateCursor(ToolType _type)
    {
        if (SoftwareCursorManager.Instance != null)
        {
            SoftwareCursorManager.Instance.SetTool(_type);
        }
    }
    #endregion

    #region Transición de Día
    private IEnumerator DayTransitionRoutine()
    {
        if (dayTransitionPanel != null) dayTransitionPanel.gameObject.SetActive(true);
        if (dayTransitionText != null)
        {
            dayTransitionText.gameObject.SetActive(true);
            dayTransitionText.text = $"Día {GameManager.Instance.CurrentDay + 1}";
        }

        Color panelColor = dayTransitionPanel ? dayTransitionPanel.color : Color.white;
        Color textColor = dayTransitionText ? dayTransitionText.color : Color.white;

        float t = 0f;
        float startA_panel = panelColor.a;
        float startA_text = textColor.a;

        // Fade-in a blanco
        while (t < dayFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dayFadeDuration);

            if (dayTransitionPanel != null)
            {
                panelColor.a = Mathf.Lerp(startA_panel, 1f, k);
                dayTransitionPanel.color = panelColor;
            }

            if (dayTransitionText != null)
            {
                textColor.a = Mathf.Lerp(startA_text, 1f, k);
                dayTransitionText.color = textColor;
            }

            // Cuando ya casi está blanco, quitamos el summary para que no se vea "desaparecer"
            if (k >= 1f && summaryPanel.activeSelf)
                summaryPanel.SetActive(false);

            yield return null;
        }

        // Asegurar full blanco
        if (dayTransitionPanel != null)
        {
            panelColor.a = 1f;
            dayTransitionPanel.color = panelColor;
        }
        if (dayTransitionText != null)
        {
            textColor.a = 1f;
            dayTransitionText.color = textColor;
        }
        if (summaryPanel.activeSelf) summaryPanel.SetActive(false);

        // Mantener un pelín
        yield return new WaitForSecondsRealtime(dayHoldDuration);

        // Nuevo día
        GameManager.Instance.StartNewDay();

        // Fade-out
        t = 0f;
        startA_panel = panelColor.a;
        startA_text = textColor.a;

        while (t < dayFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dayFadeDuration);
            float a = Mathf.Lerp(startA_panel, 0f, k);

            if (dayTransitionPanel != null)
            {
                panelColor.a = a;
                dayTransitionPanel.color = panelColor;
            }

            if (dayTransitionText != null)
            {
                textColor.a = a;
                dayTransitionText.color = textColor;
            }

            yield return null;
        }

        // Dejarlo limpio
        if (dayTransitionPanel != null)
        {
            panelColor.a = 0f;
            dayTransitionPanel.color = panelColor;
            dayTransitionPanel.gameObject.SetActive(false);
        }
        if (dayTransitionText != null)
        {
            textColor.a = 0f;
            dayTransitionText.color = textColor;
            dayTransitionText.gameObject.SetActive(false);
        }
    }
    #endregion

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged -= UpdateMoneyText;
            GameManager.Instance.OnWaterChanged -= UpdateWaterText;
            GameManager.Instance.OnFertilizerChanged -= UpdateFertilizerText;
            GameManager.Instance.OnDayChanged -= UpdateDayText;
            GameManager.Instance.OnToolChanged -= UpdateCursor;
            GameManager.Instance.OnBiodiversityChanged -= UpdateBiodiversityText;
            GameManager.Instance.OnPlantInfoClick -= ShownPlantTypeInfoPanel;
            GameManager.Instance.OnDayEnd -= ShowDaySummaryPanel;
            GameManager.Instance.OnStrikesChanged -= UpdateStrikesVisuals;
        }

        if (PlotsManager.Instance != null)
        {
            PlotsManager.Instance.OnPlotSelected -= ShowInfoPanel;
            PlotsManager.Instance.OnPlotUnselected -= UnShowInfoPanel;
        }

        if (WeatherManager.Instance != null)
        {
            WeatherManager.Instance.OnCurrentWeatherChanged -= UpdateCurrentWeatherDisplay;
            WeatherManager.Instance.OnForecastChanged -= UpdateForecastDisplay;
        }

        if (currentlySelectedPlot != null)
        {
            currentlySelectedPlot.OnPlotDataUpdated -= UpdatePlotInfoPanelText;
        }
    }
}
