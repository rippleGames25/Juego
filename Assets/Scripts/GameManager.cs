using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;

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

public enum StrikeRemovedReason
{
    ThreeBiodiversity,  // Sumar 3 de biodiversidad
    FiveDaysNoDeath     // Estar 5 dias sin muertes de plantas
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

    public event Action<int, int, StrikeReason> OnNewStrike;
    public event Action<int, int, StrikeRemovedReason> OnStrikeRemoved;

    // Variables
    public int winCondition;
    private ToolType currentToolEnum;
    private ITool currentActiveToolObject;

    private int currentDay = 1;
    public int currentBiodiversity = 0;
    private DailyWeather currentWeather;
    private bool isDayTransitioning = false;

    // Resources
    private int currentMoney = 5;
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

    // Tanques
    public WaterTank GlobalWaterTank { get; private set; }
    public FertilizerTank GlobalFertilizerTank { get; private set; }

    // Herramientas
    private Dictionary<ToolType, ITool> toolsDictionary;

    private int INITIAL_WATER = 8;
    private int INITIAL_FERTILIZER = 8;


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
        // Ahora le preguntamos al tanque, no a una variable local
        get { return GlobalWaterTank != null ? GlobalWaterTank.GetCurrentWater() : 0; }
    }

    public int CurrentFertilizer
    {
        // Ahora le preguntamos al tanque, no a una variable local
        get { return GlobalFertilizerTank != null ? GlobalFertilizerTank.GetFertilizer() : 0; }
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
        get { return currentToolEnum; }
        set
        {
            currentToolEnum = value;

            if (toolsDictionary.ContainsKey(value))
            {
                currentActiveToolObject = toolsDictionary[value];
                Debug.Log("Herramienta equipada: " + currentToolEnum);
            } else
            {
                currentActiveToolObject = null;
            }

            OnToolChanged?.Invoke(currentToolEnum);
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

        // Inicializacion Tanques
        GlobalWaterTank = new WaterTank(INITIAL_WATER);
        GlobalFertilizerTank = new FertilizerTank(INITIAL_FERTILIZER);

        GlobalWaterTank.OnWaterChanged += (newValue) => {
            OnWaterChanged?.Invoke(newValue);
        };

        GlobalFertilizerTank.OnFertilizerChanged += (newValue) => {
            OnFertilizerChanged?.Invoke(newValue);
        };

        InitializeTools();

        currentActiveToolObject = null;

        GenerateWinCondition();
    }

    void Start()
    {
        Time.timeScale = 1f;

        currentToolEnum = ToolType.None;

        PlotsManager.Instance.CreatePlots();

        GameSessionStats.Instance?.ResetStats();
        normalStrikes = 0;
        permanentStrikes = 0;

        UpdateBiodiversityScore(0);
        HandleWeatherEvent();

        // Subsripción a eventos

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPlotClicked += HandlePlotClick;
            InputManager.Instance.OnToolClicked += HandleToolClick;
            InputManager.Instance.OnBackgroundClicked += HandleBackgroundClick;
        }

        if (PlotsManager.Instance != null)
        {
            PlotsManager.Instance.OnPlantDied += PlantDied;
            PlotsManager.Instance.OnBiodiversityChange += UpdateBiodiversityScore;
        }
    }

    private void InitializeTools()
    {
        toolsDictionary = new Dictionary<ToolType, ITool>();

        toolsDictionary.Add(ToolType.WateringCan, new WateringCan(GlobalWaterTank));
        toolsDictionary.Add(ToolType.FertilizerBag, new FertilizerBag(GlobalFertilizerTank));
        toolsDictionary.Add(ToolType.Shovel, new Shovel());
        toolsDictionary.Add(ToolType.Plant, new Seed());
    }

    void Update()
    {
        if (inputLocked || isDayTransitioning) return;
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

    public void UpdateBiodiversityScore( int _biodiversity)
    {

        CurrentBiodiversity = _biodiversity;

    }

    private void ReportPlantDeath()
    {
        daysWithoutDeathRacha = 0;

        plantDeathCounter++;
        if (plantDeathCounter >= 2)
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

        OnNewStrike?.Invoke(normalStrikes, permanentStrikes, reason);
    }

    public void PlantDied(int penalty)
    {
        AddPenalty(penalty);
        Debug.Log($"Se ha restado {penalty} pétalo de tu economía total");
        ReportPlantDeath();
    }
    private void AddPenalty(int amount)
    {
        penaltiesThisDay += amount;
    }

    private void RemoveStrike(StrikeRemovedReason reason)
    {
        if (normalStrikes > 0)
        {
            normalStrikes--;
            Debug.Log($"[GameManager] ¡Strike Normal ELIMINADO por buen comportamiento! Quedan: {normalStrikes} Normales, {permanentStrikes} Permanentes.");

            OnStrikeRemoved?.Invoke(normalStrikes, permanentStrikes, reason);
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
            case PlantCategory.Producer:
                newPlant = newPlantGO.AddComponent<ProducerPlant>();
                break;
            case PlantCategory.ShadeProvider:
                newPlant = newPlantGO.AddComponent<ShaderProviderPlant>();
                break;
            case PlantCategory.PollinatorAtractor:
                newPlant = newPlantGO.AddComponent<PollinatorAttractorPlant>();
                break;
            case PlantCategory.WildlifeRefuge:
                newPlant = newPlantGO.AddComponent<WildlifeRefugePlant>();
                break;
            default:
                newPlant = newPlantGO.AddComponent<Plant>();
                break;
        }

        newPlant.InitializePlant(plantData, plot);

        plot.AddPlant(newPlant);


        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.NotifyFirstPlantPlaced(plot);          
            TutorialManager.Instance.NotifyPlantCategoryPlanted(plantData); 
        }

        CurrentTool = ToolType.None;
        SFXManager.Instance?.PlayPlantar();

        Debug.Log($"Semilla de {plantData.plantName} plantada en la parcela {plot.gridCoordinates}");

        // Avisar al tutorial (solo interesa en el Día 1 y si el jugador quiso ayuda)
        if (TutorialManager.Instance != null &&
            TutorialManager.Instance.wantsDay1Tutorial &&
            TutorialManager.Instance.hasChosenAtStart &&
            CurrentDay == 1)
        {
            TutorialManager.Instance.NotifyFirstPlantPlaced(plot);
        }
    }


    public void ShowPlantTypePanel(PlantType plantType)
    {
        OnPlantInfoClick?.Invoke(plantType);
    }

    private void GenerateWinCondition()
    {
        winCondition = 10;
    } 

    private void HandlePlotClick(Plot plotClicked)
    {
        if (currentActiveToolObject != null)
        {
            currentActiveToolObject.Usar(plotClicked); // Usar herramienta equipada

        }
        else
        {
            SelectPlot(plotClicked);
        }

    }

    private void HandleToolClick(ToolItem toolClicked)
    {
        if (CurrentTool == toolClicked.type)
        {
            CurrentTool = ToolType.None;
        }
        else
        {
            CurrentTool = toolClicked.type;
        }

        SFXManager.Instance?.PlayClick();
    }

    private void HandleBackgroundClick()
    {
        if (PlotsManager.Instance.currentSelectedPlot != null)
        {
            PlotsManager.Instance.PlotUnselected(PlotsManager.Instance.currentSelectedPlot);
        }
    }

    public void SelectPlot(Plot plot)
    {
        if (plot.currentPlant != null && plot.currentPlant.hasProduct && plot.currentPlant is ProducerPlant producerPlant)
        {
            producerPlant.CollectProduct();
            SFXManager.Instance?.PlayComprar();

            int price = plot.currentPlant.plantData.price / 2;

            plot.ChangePlotAnimation($"+ {price}", 2, false);
        }
        else
        {
            PlotsManager.Instance.PlotSelected(plot);
            SFXManager.Instance?.PlayClick();
            Debug.Log($"Parcela {plot.gridCoordinates} seleccionada.");
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

        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.NotifyDailyWeather(currentWeather);
        }
    }

    private void ApplyDailyResourcesAndPenalties()
    {
        GlobalWaterTank.RefillWater(lastDayWaterIncome);
        GlobalFertilizerTank.RefillFertilizer(lastDayFertilizerIncome);

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

        if (totalPlanted > 2)
        {
            daysWithoutDeathRacha++;
            if (daysWithoutDeathRacha >= 5)
            {
                RemoveStrike(StrikeRemovedReason.FiveDaysNoDeath);
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
            RemoveStrike(StrikeRemovedReason.ThreeBiodiversity);
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

    void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPlotClicked -= HandlePlotClick;
            InputManager.Instance.OnToolClicked -= HandleToolClick;
            InputManager.Instance.OnBackgroundClicked -= HandleBackgroundClick;
        }

        if (PlotsManager.Instance != null)
        {
            PlotsManager.Instance.OnPlantDied -= PlantDied;
            PlotsManager.Instance.OnBiodiversityChange -= UpdateBiodiversityScore;
        }
    }
}