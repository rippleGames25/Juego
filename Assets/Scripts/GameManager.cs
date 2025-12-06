using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;

public enum ToolType
{
    None,
    WateringCan,
    FertilizerBag,
    Plant,
    Shovel
}

public enum StrikeReason
{
    PlantDeath,  // Por muertes
    Inactivity,  // Por pasar días sin jugar
    Bankruptcy   // Por quedarse sin dinero y plantas
}

public class GameManager : MonoBehaviour
{
    #region Propiedades

    public static GameManager Instance;

    // Events
    public event Action<int> OnMoneyChanged;
    public event Action<int> OnWaterChanged;
    public event Action<int> OnFertilizerChanged;
    public event Action<int> OnDayChanged;
    public event Action<int> OnBiodiversityChanged;
    public event Action<ToolType> OnToolChanged;
    public event Action<PlantType> OnPlantInfoClick;
    public event Action OnDayEnd;
    public event Action<int, int> OnStrikesChanged;

    public event Action<StrikeReason> OnNewStrike;

    // Variables
    public int winCondition;
    private ToolType currentTool;
    private int currentDay = 1;
    public int currentBiodiversity = 0;
    private DailyWeather currentWeather;
    private bool isDayTransitioning = false;

    // Resources
    private int currentMoney = 5;
    private int currentWater = 8;
    private int currentFertilizer = 8;
    private const int BASE_INCOME = 1;
    private const int AMOUNT_PER_PLANT = 1;
    [SerializeField] private int cheapestPlantPrice = 1;
    public const int IDX_PLANT_SPRITE = 7;

    // Sistema de strikes 
    private const int MAX_STRIKES = 5;
    private int normalStrikes = 0;
    private int permanentStrikes = 0;
    private int plantDeathCounter = 0;

    private int inactivityDays = 0;

    private int daysWithoutDeathRacha = 0;
    private int diversityBonusRacha = 0;

    private int maxBiodiversityAchieved = 0;
    private int maxMaturePlantsAchieved = 0;

    private int penaltiesThisDay = 0;

    // Para panel de final del dia
    public DailyBonusData lastDayBonusData { get; private set; }
    public int lastDayBaseIncome { get; private set; }
    public int lastDayPlantBonus { get; private set; }
    public int lastDayPenalties { get; private set; }
    public int lastDayWaterIncome { get; private set; }
    public int lastDayFertilizerIncome { get; private set; }
    public int lastDayBailoutIncome { get; private set; }


    private bool isBailoutPending = false;
    public bool IsBailoutPending => isBailoutPending;

    [Header("Plantas")]
    [SerializeField] private Vector3 plantPosition = new Vector3(0, 0.35f, -1f);
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] public List<PlantType> plantsList;

    private bool inputLocked = false;
    public bool InputLocked => inputLocked;

    #endregion

    #region Getters y Setters
    public int CurrentMoney
    {
        get { return currentMoney; }
        set
        {
            int newValue = Mathf.Max(0, value);
            if (currentMoney != value)
            {
                currentMoney = value;
                Debug.Log("El dinero ha cambiado a " + currentMoney);
                OnMoneyChanged?.Invoke(currentMoney);
            }
        }
    }

    public int CurrentWater
    {
        get { return currentWater; }
        set
        {
            if (currentWater != value)
            {
                currentWater = value;
                OnWaterChanged?.Invoke(currentWater);
            }
        }
    }

    public int CurrentFertilizer
    {
        get { return currentFertilizer; }
        set
        {
            if (currentFertilizer != value)
            {
                currentFertilizer = value;
                OnFertilizerChanged?.Invoke(currentFertilizer);
            }
        }
    }

    public int CurrentDay
    {
        get { return currentDay; }
        set
        {
            if (currentDay != value)
            {
                currentDay = value;
                OnDayChanged?.Invoke(currentDay);
            }
        }
    }

    public ToolType CurrentTool
    {
        get { return currentTool; }
        set
        {
            if (currentTool != value)
            {
                currentTool = value;
                OnToolChanged?.Invoke(currentTool);
                Debug.Log("Herramienta cambiada a: " + currentTool);
            }
        }
    }

    public int CurrentBiodiversity
    {
        get { return currentBiodiversity; }
        set
        {
            int newValue = Mathf.Max(0, value);
            if (currentBiodiversity != newValue)
            {
                currentBiodiversity = newValue;
                Debug.Log("La biodiversidad ha cambiado a " + currentBiodiversity);
                OnBiodiversityChanged?.Invoke(currentBiodiversity);

                if (currentBiodiversity > maxBiodiversityAchieved)
                {
                    maxBiodiversityAchieved = currentBiodiversity;
                }
            }
        }
    }

    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        GenerateWinCondition();
    }

    void Start()
    {
        Time.timeScale = 1f;

        currentTool = ToolType.None;

        PlotsManager.Instance.CreatePlots();

        GameSessionStats.Instance?.ResetStats();
        OnStrikesChanged?.Invoke(normalStrikes, permanentStrikes);

        UpdateBiodiversityScore();
        HandleWeatherEvent();
    }

    void Update()
    {
        if (inputLocked || isDayTransitioning) return;

        if (Input.GetMouseButtonDown(0))
            HandleInput();
    }

    public void EndDay()
    {
        if (inputLocked) return;
        SFXManager.Instance?.PlayPasarDia();

        if (isDayTransitioning)
        {
            Debug.Log("Transición de día en curso, espera un momento.");
            return;
        }

        PlotsManager.Instance.PlotUnselected(PlotsManager.Instance.currentSelectedPlot);

        SFXManager.Instance?.StopAmbient();
        StartCoroutine(EndDayCoroutine());
    }

    private IEnumerator EndDayCoroutine()
    {
        isDayTransitioning = true;

        Debug.Log("Fin de día");

        PlotsManager.Instance.DailyUpdatePlantsHealth();

        yield return PlotsManager.Instance.AnimateDailyConsumptionAndConsume(); ;

        CalculateEndOfDayBonuses();

        int totalPlanted = PlotsManager.Instance.GetTotalPlantedCount();

        if (totalPlanted == 0)
        {
            if (CurrentMoney < cheapestPlantPrice)
            {
                // Bancarrota (Strike Rojo)
                isBailoutPending = true;
                inactivityDays = 0;
                Debug.LogWarning("[GameManager] BAILOUT PENDING!");
            }
            else
            {
                // Tiene dinero pero no planta nada (Strike Amarillo por Inactividad)
                isBailoutPending = false;
                inactivityDays++;

                if (inactivityDays >= 3)
                {
                    AddStrike(StrikeReason.Inactivity);
                    inactivityDays = 0;
                    Debug.LogWarning("[GameManager] Strike por inactividad (3 días vacíos teniendo dinero).");
                }
            }
        }
        else
        {
            isBailoutPending = false;
            inactivityDays = 0;
        }

        OnDayEnd?.Invoke();

        isDayTransitioning = false;
    }

    public void StartNewDay()
    {
        CurrentDay++;

        CheckDailyRachas();
        ApplyPendingBailout();

        HandleWeatherEvent();
        PlotsManager.Instance.DailyUpdateWeatherWater(currentWeather);
        PlotsManager.Instance.DailyUpdatePlantsGrowthAndEffects();
        PlotsManager.Instance.DailyPlagueUpdate();

        UpdateBiodiversityScore();

        if (CheckForGameOver())
        {
            return;
        }

        ApplyDailyResourcesAndPenalties();

        if (GameSessionStats.Instance != null)
        {
            GameSessionStats.Instance.daysSurvived = currentDay;
        }

        Debug.Log($"Inicio del día {CurrentDay}. Previsión : {currentWeather} nivel {currentWeather.intensity}");
    }

    public void UpdateBiodiversityScore()
    {
        if (PlotsManager.Instance != null)
        {
            CurrentBiodiversity = PlotsManager.Instance.CalculateCurrentBiodiversity();
        }
    }

    public void ReportPlantDeath()
    {
        daysWithoutDeathRacha = 0;

        plantDeathCounter++;
        if (plantDeathCounter >= 3)
        {
            AddStrike(StrikeReason.PlantDeath);
            plantDeathCounter = 0;
        }
    }

    private void AddStrike(StrikeReason reason)
    {
        bool isPermanent = (reason == StrikeReason.Bankruptcy);

        if (isPermanent)
        {
            permanentStrikes++;
            Debug.LogWarning($"[GameManager] ¡Strike PERMANENTE añadido! Total: {normalStrikes} Normales, {permanentStrikes} Permanentes.");
        }
        else
        {
            normalStrikes++;
            Debug.LogWarning($"[GameManager] ¡Strike Normal añadido! Total: {normalStrikes} Normales, {permanentStrikes} Permanentes.");
        }

        OnNewStrike?.Invoke(reason);
        OnStrikesChanged?.Invoke(normalStrikes, permanentStrikes);
    }

    public void AddPenalty(int amount)
    {
        penaltiesThisDay += amount;
    }

    private void RemoveStrike()
    {
        if (normalStrikes > 0)
        {
            normalStrikes--;
            Debug.Log($"[GameManager] ¡Strike Normal ELIMINADO por buen comportamiento! Quedan: {normalStrikes} Normales, {permanentStrikes} Permanentes.");
            OnStrikesChanged?.Invoke(normalStrikes, permanentStrikes);
        }
    }

    private void CalculateEndOfDayBonuses()
    {
        lastDayBaseIncome = (CurrentBiodiversity == 0) ? 0 : BASE_INCOME;
        lastDayPlantBonus = CurrentBiodiversity / 2;
        lastDayBonusData = PlotsManager.Instance.GetDailyBonusData();

        int resourceAmount = CalculateResourcesAmount();
        lastDayWaterIncome = resourceAmount;
        lastDayFertilizerIncome = resourceAmount;

        lastDayPenalties = penaltiesThisDay;

        if (lastDayBonusData.maturePlantCount > maxMaturePlantsAchieved)
        {
            maxMaturePlantsAchieved = lastDayBonusData.maturePlantCount;
        }
        if (lastDayBonusData.diversityBonus > 0)
        {
            diversityBonusRacha++;
        }
        else
        {
            diversityBonusRacha = 0;
        }
    }

    public void PlantSeed(Plot plot, int idx)
    {
        PlantType plantData = plantsList[idx];

        if (plantData.price > currentMoney)
        {
            SFXManager.Instance?.PlayDenegar();
            Debug.Log("No tienes dinero suficiente para comprar la planta");
            return;
        }

        CurrentMoney -= plantData.price;

        GameObject newPlantGO = Instantiate(plantPrefab, (plot.transform.position + plantPosition), Quaternion.identity);

        newPlantGO.transform.SetParent(plot.transform);

        Plant newPlant;

        switch (plantData.category)
        {
            case PlantCategory.Productor:
                newPlant = newPlantGO.AddComponent<ProducerPlant>();
                break;
            case PlantCategory.Sombra:
                newPlant = newPlantGO.AddComponent<ShaderProviderPlant>();
                break;
            case PlantCategory.Polinizadores:
                newPlant = newPlantGO.AddComponent<PollinatorAttractorPlant>();
                break;
            case PlantCategory.RefugioFauna:
                newPlant = newPlantGO.AddComponent<WildlifeRefugePlant>();
                break;
            default:
                newPlant = newPlantGO.AddComponent<Plant>();
                break;
        }


        newPlant.InitializePlant(plantData, plot);

        plot.currentPlant = newPlant;
        plot.isPlanted = true;

        plot.UpdatePollinatorVisual();

        CurrentTool = ToolType.None;
        SFXManager.Instance?.PlayPlantar();

        Debug.Log($"Semilla de {plantData.plantName} plantada en la parcela {plot.gridCoordinates}");

        UpdateBiodiversityScore();
    }

    public void ShowPlantTypePanel(PlantType plantType)
    {
        OnPlantInfoClick?.Invoke(plantType);
    }

    private void GenerateWinCondition()
    {
        winCondition = 10;
    }

    private void HandleInput()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);
        bool isPointerOverUI = EventSystem.current.IsPointerOverGameObject();

        if (hit.collider != null)
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.CompareTag("Tool"))
            {
                ToolItem tool = hitObject.GetComponent<ToolItem>();

                if (CurrentTool == tool.type)
                {
                    CurrentTool = ToolType.None;
                }
                else
                {
                    CurrentTool = tool.type;
                }

                SFXManager.Instance?.PlayClick();
            }
            else if (hitObject.CompareTag("Plot"))
            {
                Plot plot = hitObject.GetComponent<Plot>();

                plot.SelectPlot();
            }

        }
        else
        {
            if (!isPointerOverUI)
            {
                Plot _currentSelectedPlot = PlotsManager.Instance.currentSelectedPlot;
                this.CurrentTool = ToolType.None;

                if (_currentSelectedPlot != null)
                {
                    PlotsManager.Instance.PlotUnselected(_currentSelectedPlot);
                }
            }
        }

    }

    private int CalculateResourcesAmount()
    {
        int plantCount = PlotsManager.Instance.GetTotalPlantedCount();

        if (plantCount == 0)
        {
            return 0;
        }

        int amount = BASE_INCOME + (plantCount * AMOUNT_PER_PLANT);
        return amount;
    }

    #region Métodos de actualización diaria
    private void HandleWeatherEvent()
    {
        currentWeather = WeatherManager.Instance.PassDay();
    }

    private void ApplyDailyResourcesAndPenalties()
    {
        CurrentWater += lastDayWaterIncome;
        CurrentFertilizer += lastDayFertilizerIncome;

        int totalIncome = lastDayBaseIncome + lastDayPlantBonus + lastDayBonusData.madurityBonus + lastDayBonusData.diversityBonus + lastDayBonusData.solarExposureBonus;
        CurrentMoney += totalIncome;

        CurrentMoney -= lastDayPenalties;

        penaltiesThisDay = 0;

        Debug.Log($"[GameManager] Ingresos aplicados: +{totalIncome}. Penalizaciones: -{lastDayPenalties}.");
    }

    private void CheckDailyRachas()
    {
        // verificar si hay plantas vivas. Si no hay plantas, no cuenta como no muertes.
        int totalPlanted = 0;
        if (PlotsManager.Instance != null)
        {
            totalPlanted = PlotsManager.Instance.GetTotalPlantedCount();
        }

        if (totalPlanted > 0)
        {
            daysWithoutDeathRacha++;
            if (daysWithoutDeathRacha >= 5)
            {
                RemoveStrike();
                daysWithoutDeathRacha = 0;
                Debug.Log("[GameManager] ¡Strike retirado por 5 días sin muertes!");
            }
        }
        else
        {
            daysWithoutDeathRacha = 0;
        }

        // racha de diversidad
        if (diversityBonusRacha >= 3)
        {
            RemoveStrike();
            diversityBonusRacha = 0;
            Debug.Log("[GameManager] ¡Strike retirado por diversidad mantenida!");
        }
    }

    private void ApplyPendingBailout()
    {
        if (isBailoutPending)
        {
            Debug.LogWarning("[GameManager] ¡BAILOUT APLICADO!");

            AddStrike(StrikeReason.Bankruptcy);

            CurrentMoney += 3;

            isBailoutPending = false;
        }
    }

    private bool CheckForGameOver()
    {
        if (currentBiodiversity >= winCondition)
        {
            Debug.Log("[GameManager] ¡VICTORIA! Biodiversidad alcanzada.");
            EndGame(true);
            return true;
        }

        if (normalStrikes + permanentStrikes >= MAX_STRIKES)
        {
            Debug.LogError("[GameManager] GAME OVER: Límite de Strikes alcanzado.");
            EndGame(false);
            return true;
        }

        return false;
    }

    private void EndGame(bool didWin)
    {
        if (GameSessionStats.Instance != null)
        {
            GameSessionStats.Instance.daysSurvived = currentDay;
            GameSessionStats.Instance.maxBiodiversityAchieved = maxBiodiversityAchieved;
            GameSessionStats.Instance.maxMaturePlantsAchieved = maxMaturePlantsAchieved;

            GameSessionStats.Instance.didWinGame = didWin;
        }

        SceneManager.LoadScene("GameOverScene");
    }

    #endregion

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        Time.timeScale = locked ? 0f : 1f;
    }
}