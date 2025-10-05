using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    //Variables
    public static GameManager Instance; // Singleton

    // Events
    public event Action<int> OnMoneyChanged;
    public event Action<int> OnWaterChanged;
    public event Action<int> OnFertilizerChanged;
    public event Action<int> OnDayChanged;


    private int currentDay = 1;

    // Resources
    private int currentMoney = 100;
    private int currentWater = 10;
    private int currentFertilizer = 10;


    // Garden State
    // Lista de parcelas
    // Lista de plantas

    // Weather
    private int currentWeather;

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
        // Generar nivel
        // Generar planta inicial
        // Generar proximos eventos meteorologicos
        // Generar objetivo
    }

    // Update is called once per frame
    void Update()
    {

    }



    // Public Methods
    public void PassDay()
    {
        CurrentDay++; // Pasar al dia siguiente
        Debug.Log("Dia " + currentDay);

        HandleWeatherEvent(); // Evento meteorol�gico
        UpdatePlotAndPlants();// Actualizar estado de las plantas y parcelas
        DistributeDailyResources();// Sumar recursos
        CheckWinCondition();// Condici�n de victoria

    }


    // Private Methods
    private void HandleWeatherEvent()
    {
        // Pasar al evento meteorol�gico siguiente y generar uno nuevo
        WeatherManager.Instance.PassDay();
    }

    private void UpdatePlotAndPlants()
    {
        // Evento meteorol�gico
        // Actualizar parcelas

        // Actualizar plantas
        // Comprobar muertes

    }

    private void DistributeDailyResources()
    {
        int amount = CalculateResourcesAmount();    // Calcular la cantidad que le tienen que dar

        EarnMoney(amount);  // Sumar abono
        EarnWater(amount);  // Sumar agua
        EarnFertilizer(amount*5); // Sumar dinero
    }

    private void CheckWinCondition()
    {
        // Comparar el objetivo con el estado actual
    }

    private int CalculateResourcesAmount()
    {
        // Calcular seg�n la biodiversidad y la salud del jardin
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
