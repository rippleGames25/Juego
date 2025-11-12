using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Collections;

public enum ToolType
{
    None,
    WateringCan,
    FertilizerBag,
    Plant,
    Shovel
}

public class GameManager : MonoBehaviour
{
    #region Propiedades
    
    public static GameManager Instance; // Singleton

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
    private const int BASE_INCOME_IF_PLANTED = 3;
    private const int AMOUNT_PER_PLANT = 1;
    [SerializeField] private int cheapestPlantPrice = 1;
    public const int IDX_PLANT_SPRITE = 7; // Indice de la imagen principal de la planta en el plantSprites

    // Sistema de strikes 
    private const int MAX_STRIKES = 5;
    private int normalStrikes = 0;
    private int permanentStrikes = 0;
    private int plantDeathCounter = 0; // Contador para (1 strike por 3 muertes)

    private int daysWithoutDeathRacha = 0;
    private int diversityBonusRacha = 0;

    private int maxBiodiversityAchieved = 0;
    private int maxMaturePlantsAchieved = 0;

    [Header("Plantas")]
    [SerializeField] private Vector3 plantPosition = new Vector3(0, 0.1f, -0.1f);
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] public List<PlantType> plantsList; // Lista de Tipos de planta (ScriptableObjects)

    private bool inputLocked = false;
    public bool InputLocked => inputLocked;

    #endregion

    #region Getters y Setters
    public int CurrentMoney
    {
        get { return currentMoney; }
        set
        {
            int newValue = Mathf.Max(0, value); // Dinero nunca negativo
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
    }

    void Start()
    {
        currentTool = ToolType.None; // No tiene cogida ninguna herramienta

        // Generar nivel
        PlotsManager.Instance.CreatePlots(); // Inicializar parcelas

        // Generar planta inicial
        // Generar proximos eventos meteorologicos
        // Generar objetivo
        GenerateWinCondition();

        GameSessionStats.Instance?.ResetStats();
        OnStrikesChanged?.Invoke(normalStrikes, permanentStrikes);
    }

    void Update()
    {
        if (inputLocked) return;                 
        if (Input.GetMouseButtonDown(0))
            HandleInput();
    }

    // Public Methods
    public void EndDay()
    {
        if (inputLocked) return;
        SFXManager.Instance?.PlayPasarDia();

        if (isDayTransitioning)
        {
            Debug.Log("Transición de día en curso, espera un momento.");
            return;
        }

        SFXManager.Instance?.StopAmbient();
        StartCoroutine(EndDayCoroutine());
    }

    private IEnumerator EndDayCoroutine()
    {
        isDayTransitioning = true;

        Debug.Log("Fin de día");
        
        // 1. Actualizar estado de salud
        PlotsManager.Instance.DailyUpdatePlantsHealth();

        // 2. Animación de consumo de recursos
        yield return PlotsManager.Instance.AnimateDailyConsumptionAndConsume(); ;

        OnDayEnd?.Invoke();

        isDayTransitioning = false;
    }

    public void StartNewDay()
    {
        CurrentDay++;
        Debug.Log("INICIO DE DÍA: Día " + CurrentDay);

        CheckDailyRachas(); // Comprueba si gana o pierde rachas
        CheckForBailout();    // Comprueba si necesita el rescate

        // Comprueba Game Over antes de hacer nada
        if (CheckForGameOver())
        {
            return;
        }

        HandleWeatherEvent();
        PlotsManager.Instance.DailyUpdateWeatherWater(currentWeather);

        PlotsManager.Instance.DailyUpdatePlantsGrowthAndEffects();

        DistributeDailyResources();

        if (GameSessionStats.Instance != null)
        {
            GameSessionStats.Instance.daysSurvived = currentDay;
        }

        Debug.Log("Inicio de día completado.");
    }

    public void ReportPlantDeath()
    {
        // penalización de dinero
        CurrentMoney -= 1;
        Debug.Log("Se ha restado 1 pétalo por muerte de planta.");

        // resetea la racha de días sin muerte
        daysWithoutDeathRacha = 0;

        // lógica de strikes
        plantDeathCounter++;
        if (plantDeathCounter >= 3)
        {
            AddStrike(false); // añade un strike normal (no permanente)
            plantDeathCounter = 0; // resetea el contador
        }
    }

    private void AddStrike(bool isPermanent)
    {
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
        OnStrikesChanged?.Invoke(normalStrikes, permanentStrikes);
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

    public void PlantSeed(Plot plot, int idx)
    {
        PlantType plantData = plantsList[idx];        

        if(plantData.price > currentMoney) // No tiene suficiente dinero
        {
            SFXManager.Instance?.PlayDenegar();
            Debug.Log("No tienes dinero suficiente para comprar la planta");
            return;
        }

        CurrentMoney -= plantData.price; // Restar el dinero que cuesta la planta

        GameObject newPlantGO = Instantiate(plantPrefab, (plot.transform.position + plantPosition), Quaternion.identity);

        newPlantGO.transform.SetParent(plot.transform); // Establecemos la parcela como padre

        Plant newPlant;

        switch (plantData.category)
        {
            case PlantCategory.Producer:
                newPlant = newPlantGO.AddComponent<ProducerPlant>();
                break;
            case PlantCategory.ProvidesShade:
                newPlant = newPlantGO.AddComponent<ShaderProviderPlant>();
                break;
            case PlantCategory.PollinatorAttractor:
                newPlant = newPlantGO.AddComponent<PollinatorAttractorPlant>();
                break;
            case PlantCategory.WildlifeRefuge:
                newPlant = newPlantGO.AddComponent<WildlifeRefugePlant>();
                break;
            default:
                newPlant = newPlantGO.AddComponent<Plant>(); // O una clase base Plant simple, o PollinatorAttractorPlant
                break;
        }


        newPlant.InitializePlant(plantData, plot);

        plot.currentPlant= newPlant; // Asociamos la planta a la parcela
        plot.isPlanted = true;

        CurrentTool = ToolType.None; // Desequipamos la semilla
        SFXManager.Instance?.PlayPlantar();

        Debug.Log($"Semilla de {plantData.plantName} plantada en la parcela {plot.gridCoordinates}");
    }

    public void ShowPlantTypePanel(PlantType plantType)
    {
        OnPlantInfoClick?.Invoke(plantType);
    }

    // Private Methods
    private void GenerateWinCondition()
    {
        // De momento fija
        winCondition = 10;
    }

    private void HandleInput()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Convertir mouse a posicion del mundo
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero); // Lanzar rayo

        if (hit.collider != null) // Si colisiona
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.CompareTag("Tool")) // Herramienta
            {
                ToolItem tool = hitObject.GetComponent<ToolItem>();

                if (CurrentTool == tool.type)
                {
                    CurrentTool = ToolType.None; // Si ya la tiene, la desactiva
                }
                else
                {
                    CurrentTool = tool.type; // Si no la tiene la equipa
                }

                SFXManager.Instance?.PlayClick();
            }
            else if (hitObject.CompareTag("Plot"))
            {
                Plot plot = hitObject.GetComponent<Plot>(); // Parcela

                plot.SelectPlot();
            }

        } else
        {
            // Si no ha colisionado con nada
            Plot _currentSelectedPlot = PlotsManager.Instance.currentSelectedPlot;
            this.CurrentTool = ToolType.None;

            if (_currentSelectedPlot != null)
            {
                PlotsManager.Instance.PlotUnselected(_currentSelectedPlot); // Deseleccionar Parcela

            }
        }

    }

    // Metodo para calcular que cantidad de recursos se distribuyen según la biodiversidad
    private int CalculateResourcesAmount()
    {
        int plantCount = CurrentBiodiversity;

        // si no tienes plantas, no ganas nada
        if (plantCount == 0)
        {
            return 0;
        }

        // gana un fijo + el bono por cantidad
        int amount = BASE_INCOME_IF_PLANTED + (plantCount * AMOUNT_PER_PLANT);
        return amount;
    }

    #region Métodos de actualización diaria
    private void HandleWeatherEvent()
    {
        // Pasar al evento meteorológico siguiente y generar uno nuevo
        currentWeather = WeatherManager.Instance.PassDay();
    }


    private void DistributeDailyResources()
    {
        // agua y abono basado en la cantidad de plantas
        int amount = CalculateResourcesAmount();
        CurrentWater += amount;
        CurrentFertilizer += amount;

        int dailyIncome = 1;
        Debug.Log($"[GameManager] Ingresos: +1 (Bono base diario)");

        int plantBonus = CurrentBiodiversity / 2;
        if (plantBonus > 0)
        {
            dailyIncome += plantBonus;
            Debug.Log($"[GameManager] Ingresos: +{plantBonus} (Bono por cantidad de plantas)");
        }

        // dinero llamando al método de PlotsManager
        if (PlotsManager.Instance != null)
        {
            DailyBonusData bonusData = PlotsManager.Instance.GetDailyBonusData();

            // actualizamos stats
            if (bonusData.maturePlantCount > maxMaturePlantsAchieved)
            {
                maxMaturePlantsAchieved = bonusData.maturePlantCount;
            }

            // añadimos bonos de dinero
            dailyIncome += bonusData.madurityBonus;
            dailyIncome += bonusData.diversityBonus;
            dailyIncome += bonusData.solarExposureBonus;

            // actualizamos racha de diversidad
            if (bonusData.diversityBonus > 0)
            {
                diversityBonusRacha++;
            }
            else
            {
                diversityBonusRacha = 0; // se rompió la racha
            }
        }

        CurrentMoney += dailyIncome;
    }

    private void CheckDailyRachas()
    {
        // racha sin muertes
        daysWithoutDeathRacha++;
        if (daysWithoutDeathRacha >= 5)
        {
            RemoveStrike(); // Quita 1 strike normal
            daysWithoutDeathRacha = 0; // Resetea la racha
        }

        // racha de diversidad
        if (diversityBonusRacha >= 3)
        {
            RemoveStrike(); // Quita 1 strike normal
            diversityBonusRacha = 0; // Resetea la racha
        }
    }

    private void CheckForBailout()
    {
        if (CurrentBiodiversity == 0 && CurrentMoney < cheapestPlantPrice)
        {
            Debug.LogWarning("[GameManager] ¡BAILOUT! 0 plantas y 0 dinero.");

            // da el strike permanente
            AddStrike(true);

            // da el dinero de rescate
            CurrentMoney += 3;
        }
    }

    private bool CheckForGameOver()
    {
        // condición de derrota (Strikes)
        if (normalStrikes + permanentStrikes >= MAX_STRIKES)
        {
            Debug.LogError("[GameManager] GAME OVER: Límite de Strikes alcanzado.");
            EndGame(false);
            return true;
        }

        // condición de victoria
        if (currentBiodiversity >= winCondition)
        {
            Debug.Log("[GameManager] ¡VICTORIA! Biodiversidad alcanzada.");
            EndGame(true);
            return true;
        }

        return false;
    }

    private void EndGame(bool didWin)
    {
        // Guardamos las estadísticas finales
        if (GameSessionStats.Instance != null)
        {
            GameSessionStats.Instance.daysSurvived = currentDay;
            GameSessionStats.Instance.maxBiodiversityAchieved = maxBiodiversityAchieved;
            GameSessionStats.Instance.maxMaturePlantsAchieved = maxMaturePlantsAchieved;

            GameSessionStats.Instance.didWinGame = didWin;
        }

        // Cargamos la escena de fin de partida
        SceneManager.LoadScene("GameOverScene");
    }

    #endregion

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        Time.timeScale = locked ? 0f : 1f; // pausa simulación también
    }
}
