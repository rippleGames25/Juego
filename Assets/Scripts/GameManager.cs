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

    // Variables
    private int winCondition;
    private ToolType currentTool;
    private int currentDay = 1;
    private int currentBiodiversity = 0;
    private DailyWeather currentWeather;
    private bool isDayTransitioning = false;

    // Resources
    private int currentMoney = 3;
    private int currentWater = 8;
    private int currentFertilizer = 8;
    private const int INITIAL_AMOUNT = 1;
    private const int AMOUNT_PER_PLANT = 2;
    public const int IDX_PLANT_SPRITE = 7; // Indice de la imagen principal de la planta en el plantSprites

    [Header("Plantas")]
    [SerializeField] private Vector3 plantPosition = new Vector3(0, 0.1f, -0.1f);
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] public List<PlantType> plantsList; // Lista de Tipos de planta (ScriptableObjects)

    #endregion

    #region Getters y Setters
    public int CurrentMoney
    {
        get { return currentMoney; }
        set
        {
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
            if (currentBiodiversity != value)
            {
                currentBiodiversity = value;
                Debug.Log("La biodiversidad ha cambiado a " + currentBiodiversity);
                OnBiodiversityChanged?.Invoke(currentBiodiversity);
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
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput();
        }
    }

    // Public Methods
    public void EndDay()
    {
        if (isDayTransitioning)
        {
            Debug.Log("Transición de día en curso, espera un momento.");
            return;
        }

        StartCoroutine(EndDayCoroutine());

    }

    private IEnumerator EndDayCoroutine()
    {
        isDayTransitioning = true;

        Debug.Log("Fin de día");

        // 1. Animación de consumo de recursos
        yield return PlotsManager.Instance.AnimateDailyConsumptionAndConsume(); ;

        // 2. Actualizar estado de salud
        PlotsManager.Instance.DailyUpdatePlantsHealth();

        // 3. Condición de victoria
        CheckWinCondition(); 

        OnDayEnd?.Invoke();

        isDayTransitioning = false;
    }

    public void StartNewDay()
    {
        CurrentDay++;
        Debug.Log("INICIO DE DÍA: Día " + CurrentDay);

        HandleWeatherEvent();
        PlotsManager.Instance.DailyUpdateWeatherWater(currentWeather.waterChange);

        PlotsManager.Instance.DailyUpdatePlantsGrowthAndEffects();

        DistributeDailyResources();

        Debug.Log("Inicio de día completado.");
    }

    public void PlantSeed(Plot plot, int idx)
    {
        PlantType plantData = plantsList[idx];        

        if(plantData.price > currentMoney) // No tiene suficiente dinero
        {
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
        
        Debug.Log($"Semilla de {plantData.plantName} plantada en la parcela {plot.gridCoordinates}");
    }

    public void PlantsDeath(Plant plantToDeath)
    {
        Debug.Log($"La planta {plantToDeath.plantData.plantName} ha muerto porque no has cubierto sus necesidades");
        Destroy(plantToDeath.gameObject);
        CurrentBiodiversity--;
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
        int amount = CurrentBiodiversity * AMOUNT_PER_PLANT;

        if (amount == 0)
        {
            return INITIAL_AMOUNT;
        }
        else
        {
            return amount;
        }
    }

    #region Métodos de actualización diaria
    private void HandleWeatherEvent()
    {
        // Pasar al evento meteorológico siguiente y generar uno nuevo
        currentWeather = WeatherManager.Instance.PassDay();
    }


    private void DistributeDailyResources()
    {
        int amount = CalculateResourcesAmount();    // Calcular la cantidad que le tienen que dar

        CurrentMoney += CurrentBiodiversity;  // Sumar dinero
        CurrentWater += amount;  // Sumar agua
        CurrentFertilizer += amount; // Sumar abono
    }

    private void CheckWinCondition()
    {
        if (currentBiodiversity == winCondition) // Comparar el objetivo con el estado actual
        {
            Debug.Log("Has llegado al objetivo de biodiversidad. Enhorabuena");
            SceneManager.LoadScene("GameOverScene");
        }
    }

    #endregion
}
