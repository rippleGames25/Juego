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
    //Variables
    public static GameManager Instance; // Singleton

    // Events
    public event Action<int> OnMoneyChanged;
    public event Action<int> OnWaterChanged;
    public event Action<int> OnFertilizerChanged;
    public event Action<int> OnDayChanged;
    public event Action<ToolType> OnToolChanged;

    // Tipos de planta (ScriptableObjects)
    [SerializeField] private PlantType plantTest;
    //...


    // Prefabs
    [SerializeField] private GameObject plantTestPrefab;

    private int winCondition;
    private ToolType currentTool;
    private int currentDay = 1;
    private int biodiversity = 0;

    // Resources
    private int currentMoney = 100;
    private int currentWater = 10;
    private int currentFertilizer = 10;

    [SerializeField] private PlotsManager plotsManager;
    [SerializeField] private Texture2D normalCursor;
    [SerializeField] private Texture2D wateringCanCursor;
    [SerializeField] private Texture2D fertilizerBagCursor;
    [SerializeField] private Texture2D plantTestCursor;

    // Garden State
    // Lista de parcelas
    private List<Plant> plantedPlants = new List<Plant>(); // Lista de plantas

    // Weather
    private DailyWeather currentWeather;

    #region Propiedades
    // Propiedades
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTool = ToolType.None; // No tiene cogida ninguna herramienta

        // Generar nivel
        plotsManager.CreatePlots(); // Inicializar parcelas

        // Generar planta inicial
        // Generar proximos eventos meteorologicos
        // Generar objetivo
        GenerateWinCondition();

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

    public void PlantSeed(Plot plot)
    {
        // Provisional con 1 tipo de planta
        PlantType plantData = plantTest;
        GameObject plantPrefab = plantTestPrefab;
        ////////////////////////////////////
        

        if(plantData.price > currentMoney) // No tiene suficiente dinero
        {
            Debug.Log("No tienes dinero suficiente para comprar la planta");
            return;
        }

        EarnMoney(-plantData.price); // Restar el dinero que cuesta la planta

        GameObject newPlantGO = Instantiate(plantPrefab, (plot.transform.position + new Vector3(0, 0, -1)), Quaternion.identity);

        Plant newPlant = newPlantGO.GetComponent<Plant>(); // Referencia al Plant del GO
        newPlant.InitializePlant(plantData);

        plot.currentPlant= newPlant; // Asociamos la planta a la parcela
        plot.isPlanted = true;

        plantedPlants.Add(newPlant); // Añadimos a la lista de plantas
        Debug.Log($"Semilla de {plantData.name} plantada en la parcela {plot.gridCoordinates}");
    }


    // Private Methods
    private void HandleWeatherEvent()
    {
        // Pasar al evento meteorológico siguiente y generar uno nuevo
        currentWeather = WeatherManager.Instance.PassDay();
    }


    private void DailyUpdatePlotsAndPlants()
    {
        plotsManager.DailyUpdate(currentWeather);
    }


    private void DistributeDailyResources()
    {
        int amount = CalculateResourcesAmount();    // Calcular la cantidad que le tienen que dar

        EarnMoney(amount);  // Sumar abono
        EarnWater(amount);  // Sumar agua
        EarnFertilizer(amount*5); // Sumar dinero
    }

    private void GenerateWinCondition()
    {
        // De momento fija
        winCondition = 5;
    }

    private void CheckWinCondition()
    {
        // Comparar el objetivo con el estado actual
        if (biodiversity == winCondition)
        {
            Debug.Log("Has llegado al objetivo de biodiversidad. Enhorabuena");
            SceneManager.LoadScene("GameOver");
        }

    }

    private int CalculateResourcesAmount()
    {
        // Calcular según la biodiversidad y la salud del jardin
        return 10;
    }
    private void EarnMoney(int amount)
    {
        CurrentMoney += amount;
    }

    private void EarnWater(int amount)
    {
        CurrentWater+= amount;
    }

    private void EarnFertilizer(int amount)
    {
        CurrentFertilizer += amount;
    }

}
