using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public enum ToolType
{
    None,
    WateringCan,
    FertilizerBag,
    Plant
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
    public event Action<ToolType> OnToolChanged;

    // Variables
    private int winCondition;
    private ToolType currentTool;
    private int currentDay = 1;
    private int biodiversity = 0;
    private DailyWeather currentWeather;

    // Resources
    private int currentMoney = 100;
    private int currentWater = 10;
    private int currentFertilizer = 10;

    [Header("Plantas")]
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] private List<PlantType> plantsList; // Lista de Tipos de planta (ScriptableObjects)

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
    public void PassDay()
    {
        CheckWinCondition(); // Condición de victoria

        CurrentDay++; // Pasar al dia siguiente
        Debug.Log("Dia " + currentDay);

        HandleWeatherEvent(); // Evento meteorológico
        DailyUpdatePlotsAndPlants();// Actualizar estado de las parcelas
        DistributeDailyResources();// Sumar recursos
    }

    public void PlantSeed(Plot plot, int idx)
    {
        PlantType plantData = plantsList[idx];        

        if(plantData.price > currentMoney) // No tiene suficiente dinero
        {
            Debug.Log("No tienes dinero suficiente para comprar la planta");
            return;
        }

        CurrentMoney -=plantData.price; // Restar el dinero que cuesta la planta

        GameObject newPlantGO = Instantiate(plantPrefab, (plot.transform.position + new Vector3(0, 0, -1)), Quaternion.identity);

        Plant newPlant = newPlantGO.GetComponent<Plant>(); // Referencia al Plant del GO
        newPlant.InitializePlant(plantData);

        plot.currentPlant= newPlant; // Asociamos la planta a la parcela
        plot.isPlanted = true;

        CurrentTool = ToolType.None; // Desequipamos la semilla
        
        Debug.Log($"Semilla de {plantData.plantName} plantada en la parcela {plot.gridCoordinates}");
    }

    public void PlantsDeath(Plant plantToDeath)
    {
        Debug.Log($"La planta {plantToDeath.plantData.plantName} ha muerto porque no has cubierto sus necesidades");
        Destroy(plantToDeath.gameObject);
    }

    // Private Methods
    private void GenerateWinCondition()
    {
        // De momento fija
        winCondition = 5;
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
        // Implementar calculo...
        return 10;
    }

    #region Métodos de actualización diaria
    private void HandleWeatherEvent()
    {
        // Pasar al evento meteorológico siguiente y generar uno nuevo
        currentWeather = WeatherManager.Instance.PassDay();
    }

    private void DailyUpdatePlotsAndPlants()
    {
        PlotsManager.Instance.DailyUpdate(currentWeather);
    }

    private void DistributeDailyResources()
    {
        int amount = CalculateResourcesAmount();    // Calcular la cantidad que le tienen que dar

        CurrentMoney +=amount;  // Sumar dinero
        CurrentWater += amount;  // Sumar agua
        CurrentFertilizer += amount; // Sumar abono
    }

    private void CheckWinCondition()
    {
        if (biodiversity == winCondition) // Comparar el objetivo con el estado actual
        {
            Debug.Log("Has llegado al objetivo de biodiversidad. Enhorabuena");
            SceneManager.LoadScene("GameOver");
        }
    }

    #endregion
}
