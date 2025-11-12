using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;

public struct DailyBonusData
{
    public int madurityBonus;
    public int diversityBonus;
    public int solarExposureBonus;
    public int maturePlantCount;
}

public class PlotsManager : MonoBehaviour
{
    public static PlotsManager Instance; // Singleton

    [SerializeField] private GameObject plotPrefab;
    [SerializeField] private int rows = 6;
    [SerializeField] private int columns = 5;
    [SerializeField] private float spacing = 0.5f; // Espacio entre parcelas
    

    private Plot[,] plotGrid;

    public Plot currentSelectedPlot;

    // Events
    public event Action<Plot> OnPlotSelected;
    public event Action OnPlotUnselected;

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
        plotGrid = new Plot[rows, columns]; // Inicializar Array
    }

    #region Métodos publicos

    // Metodo que crea e instancia las parcelas
    public void CreatePlots()
    {
        Vector3 startPos = transform.position;

        for(int x=0; x<rows; x++)
        {
            for (int y=0; y<columns; y++)
            {
                Vector3 position = new Vector3(startPos.x + x * spacing, startPos.y + y * spacing, 0);

                GameObject newPlot= Instantiate(plotPrefab, position, Quaternion.identity, this.transform);
                newPlot.name = $"Plot ({x},{y})";

                // Iniciliza Script Plot
                Plot plot = newPlot.GetComponent<Plot>();
                if(plot != null)
                {
                    plot.InitializePlot(x,y);
                    plotGrid[x,y] = plot;
                }
            }
        }
    }

    public void PlantsDeath(Plot plotToDeath)
    {
        SFXManager.Instance?.PlayMuerte();
        Debug.Log($"La planta {plotToDeath.currentPlant.plantData.plantName} ha muerto porque no has cubierto sus necesidades");

        // Retirar efectos de la planta
        if (plotToDeath.currentPlant.plantData.category == PlantCategory.ProvidesShade) RemoveShade(plotToDeath.gridCoordinates);

        GameManager.Instance.CurrentMoney -= 1;
        Debug.Log("Se ha restado 1 pétalo de tu economía total");

        GameManager.Instance.ReportPlantDeath();

        GameManager.Instance.CurrentBiodiversity--;
    }

    public void RemovePlant(Plot plotToRemove)
    {
        Plant plant = plotToRemove.currentPlant;

        // Economía
        if (plant.isDeath)
        {
            // Restar Economia
        }
        else
        {
            GameManager.Instance.CurrentMoney += (plant.plantData.price) / 2; // Gana la mitad de lo que vale la planta
        }

        // Restar número de plantas
        GameManager.Instance.CurrentBiodiversity--;

        // Variables parcela
        plotToRemove.currentPlant = null;
        plotToRemove.isPlanted = false;

    }

    public void PlotSelected(Plot _plot)
    {
        // Si hace click en la parcela ya seleccionada
        if(currentSelectedPlot!= null && currentSelectedPlot == _plot)
        {
            PlotUnselected(currentSelectedPlot);
            return;
        }

        // Si hace click en una parcela que no esta seleccionada
        if(currentSelectedPlot!=null) PlotUnselected(currentSelectedPlot); // Deselecciona la que haya seleccionada
        currentSelectedPlot = _plot;
        currentSelectedPlot.selectionBorder.SetActive(true);

        OnPlotSelected?.Invoke(currentSelectedPlot); // Invocar evento 
    }

    public void PlotUnselected(Plot plot)
    {
        if(currentSelectedPlot!= null && currentSelectedPlot==plot) {
            currentSelectedPlot.selectionBorder.SetActive(false);
            currentSelectedPlot = null;
            OnPlotUnselected?.Invoke(); // Invocar evento
        }
    }

    public DailyBonusData GetDailyBonusData()
    {
        int maturePlants = 0;
        int totalPlanted = 0;
        bool allPlantsInCorrectSun = true;
        HashSet<PlantCategory> categoriesPresent = new HashSet<PlantCategory>();

        if (plotGrid == null) return new DailyBonusData(); // Devuelve vacío

        foreach (Plot plot in plotGrid)
        {
            if (plot != null && plot.isPlanted && plot.currentPlant != null)
            {
                totalPlanted++;
                if (!plot.currentPlant.CheckSolarExposure())
                {
                    allPlantsInCorrectSun = false;
                }
                if (plot.currentPlant.currentGrowth == GrowthState.madura)
                {
                    maturePlants++;
                    categoriesPresent.Add(plot.currentPlant.plantData.category);
                }
            }
        }

        // Preparamos los datos para devolver
        DailyBonusData data = new DailyBonusData();
        data.maturePlantCount = maturePlants; // Para las stats de Game Over

        // +1 de dinero por cada 2 plantas maduras
        data.madurityBonus = maturePlants / 2;

        // +1 de dinero si tienes las 4 categorías maduras
        if (categoriesPresent.Count == 4)
        {
            data.diversityBonus = 1;
            Debug.Log("¡Bono de Diversidad conseguido!");
        }

        // +1 si todas las plantas están bien puestas
        if (totalPlanted > 0 && allPlantsInCorrectSun)
        {
            data.solarExposureBonus = 1;
            Debug.Log("¡Bono de Exposición Solar conseguido!");
        }

        return data;
    }

    #endregion

    #region Producers Methods

    #endregion

    #region Provides Shade Methods

    public void GenerateShade(Vector2Int _plantCoordinates)
    {
        int x = _plantCoordinates.x;
        int y = _plantCoordinates.y;
        int sizeX = plotGrid.GetLength(0);
        int sizeY = plotGrid.GetLength(1);

        List<Vector2Int> neighbors = new List<Vector2Int>
        {
            new Vector2Int(x - 1, y), // Izquierda
            new Vector2Int(x + 1, y), // Derecha
            new Vector2Int(x, y - 1), // Abajo
            new Vector2Int(x, y + 1)  // Arriba
        };

        foreach (var coord in neighbors)
        {
            if (coord.x >= 0 && coord.x < sizeX && coord.y >= 0 && coord.y < sizeY)
            {
                plotGrid[coord.x, coord.y].AddShadeSource(); 
            }
        }

    }

    public void RemoveShade(Vector2Int _plantCoordinates)
    {
        int x = _plantCoordinates.x;
        int y = _plantCoordinates.y;
        int sizeX = plotGrid.GetLength(0);
        int sizeY = plotGrid.GetLength(1);

        List<Vector2Int> neighbors = new List<Vector2Int>
        {
            new Vector2Int(x - 1, y), // Izquierda
            new Vector2Int(x + 1, y), // Derecha
            new Vector2Int(x, y - 1), // Abajo
            new Vector2Int(x, y + 1)  // Arriba
        };

        foreach (var coord in neighbors)
        {
            if (coord.x >= 0 && coord.x < sizeX && coord.y >= 0 && coord.y < sizeY)
            {
                plotGrid[coord.x, coord.y].RemoveShadeSource(); 
            }
        }
    }

    #endregion

    #region Métodos privados

    public IEnumerator AnimateDailyConsumptionAndConsume()
    {
        List<Coroutine> consumptionAnimations = new List<Coroutine>();

        // 1. Iterar e INICIAR la Coroutine de animación y consumo en cada parcela plantada
        foreach (Plot plot in plotGrid)
        {
            if (plot.isPlanted && plot.currentPlant != null && !plot.currentPlant.isDeath)
            {
                // Iniciamos la Coroutine y la guardamos para esperar
                Coroutine animation = StartCoroutine(plot.AnimateDailyConsumptionAndChange());
                consumptionAnimations.Add(animation);
            }
        }

        // 2. Esperar a que todas las Coroutines terminen
        foreach (Coroutine anim in consumptionAnimations)
        {
            yield return anim;
        }

        // 3. Actualizar Visuales de Parcela (Agua/Abono) después del consumo
        foreach (Plot plot in plotGrid)
        {
            if (plot.isPlanted)
            {
                plot.UpdatePlotWaterVisuals();
                plot.UpdatePlotFertilizerVisuals();
                plot.UpdatePlotSolarExposureVisuals();
            }
        }

        // El consumo ha terminado, pero aún falta la actualización de salud y crecimiento.
        Debug.Log("Consumo de agua y abono diario de plantas y animaciones hecho.");
    }

    public void DailyUpdatePlantsGrowthAndEffects()
    {
        foreach (Plot plot in plotGrid)
        {
            if (plot.isPlanted && plot.currentPlant != null)
            {
                plot.currentPlant.UpdateLifeDays();
                plot.currentPlant.ApplyDailyEffect();
            }
        }
        Debug.Log("Actualización de crecimiento y efectos (Inicio de día) hecha.");
    }

    public void DailyUpdatePlantsHealth()
    {
        foreach (Plot plot in plotGrid)
        {
            if(plot.isPlanted && plot.currentPlant != null && !plot.currentPlant.isDeath) plot.UpdatePlantDaily();
        }

        Debug.Log("Actualización de salud de plantas (Fin de día) hecha.");
    }

    public void DailyUpdateWeatherWater(DailyWeather currentWeather)
    {
        if (currentWeather.type != WeatherType.Soleado)
        {
            foreach (Plot plot in plotGrid)
            {
                plot.ChangeWater(currentWeather.waterChange);
                plot.UpdatePlotWaterVisuals();
            }
        }
        else
        {
            // Recorre cada parcela y "tira el dado"
            Debug.Log($"[PlotsManager] Aplicando Sequía Probabilística (Intensidad: {currentWeather.intensity})");
            foreach (Plot plot in plotGrid)
            {
                ApplyProbabilisticDrought(plot, currentWeather.intensity);
                plot.UpdatePlotWaterVisuals(); // Actualiza visual SÍ O SÍ
            }
        }

        Debug.Log("Cambio de agua por evento meteorologico.");
    }

    private void ApplyProbabilisticDrought(Plot plot, int intensity)
    {
        float roll = UnityEngine.Random.Range(0f, 1f); // Tira el dado (0.0 a 1.0)
        int waterLoss = 0;

        if (intensity == 1)
        {
            // 70% de perder 1
            if (roll <= 0.70f) waterLoss = -1;
            // 30% de no perder nada
        }
        else if (intensity == 2)
        {
            // 40% (pierde 1), 30% (pierde 2), 30% (nada)
            if (roll <= 0.40f) waterLoss = -1;
            else if (roll <= 0.70f) waterLoss = -2; // 0.40 + 0.30 = 0.70
        }
        else // Intensidad 3
        {
            // 33% (pierde 1), 33% (pierde 2), 33% (pierde 3)
            if (roll <= 0.33f) waterLoss = -1;
            else if (roll <= 0.66f) waterLoss = -2;
            else waterLoss = -3;
        }

        if (waterLoss < 0)
        {
            plot.ChangeWater(waterLoss);
        }
    }

    private void DailyUpdatePlotsFertilizer()
    {
        foreach (Plot plot in plotGrid)
        {
            if (plot.isPlanted)
            {
                plot.ChangeFertility(-plot.currentPlant.GetFertilizerDemand()); // Agua que consume la planta
                plot.UpdatePlotFertilizerVisuals() ;
            }
        }
        Debug.Log("Consumo de fertilizante diario hecho.");
    }

    #endregion
}
