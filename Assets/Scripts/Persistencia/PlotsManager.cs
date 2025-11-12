/*
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;

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

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                Vector3 position = new Vector3(startPos.x + x * spacing, startPos.y + y * spacing, 0);

                GameObject newPlot = Instantiate(plotPrefab, position, Quaternion.identity, this.transform);
                newPlot.name = $"Plot ({x},{y})";

                // Iniciliza Script Plot
                Plot plot = newPlot.GetComponent<Plot>();
                if (plot != null)
                {
                    plot.InitializePlot(x, y);
                    plotGrid[x, y] = plot;
                }
            }
        }
    }

    public void PlantsDeath(Plot plotToDeath)
    {
        if (plotToDeath.currentPlant != null &&
            plotToDeath.currentPlant.plantData.category == PlantCategory.ProvidesShade)
        {
            RemoveShade(plotToDeath.gridCoordinates);
        }

        GameManager.Instance.PlantsDeath(plotToDeath.currentPlant);
        plotToDeath.currentPlant = null;
        plotToDeath.isPlanted = false;
    }

    public void PlotSelected(Plot _plot)
    {
        // Si hace click en la parcela ya seleccionada
        if (currentSelectedPlot != null && currentSelectedPlot == _plot)
        {
            PlotUnselected(currentSelectedPlot);
            return;
        }

        // Si hace click en una parcela que no esta seleccionada
        if (currentSelectedPlot != null) PlotUnselected(currentSelectedPlot); // Deselecciona la que haya seleccionada
        currentSelectedPlot = _plot;
        currentSelectedPlot.selectionBorder.SetActive(true);

        OnPlotSelected?.Invoke(currentSelectedPlot); // Invocar evento 
    }

    public void PlotUnselected(Plot plot)
    {
        if (currentSelectedPlot != null && currentSelectedPlot == plot)
        {
            currentSelectedPlot.selectionBorder.SetActive(false);
            currentSelectedPlot = null;
            OnPlotUnselected?.Invoke(); // Invocar evento
        }
    }

    public int GetDailyBiodiversityIncome()
    {
        int maturePlants = 0;
        int totalPlanted = 0;
        bool allPlantsInCorrectSun = true;

        // hashSet para diversidad
        HashSet<PlantCategory> categoriesPresent = new HashSet<PlantCategory>();

        if (plotGrid == null) return 0;

        foreach (Plot plot in plotGrid)
        {
            if (plot != null && plot.isPlanted && plot.currentPlant != null)
            {
                totalPlanted++;

                // exposición solar
                if (!plot.currentPlant.CheckSolarExposure())
                {
                    allPlantsInCorrectSun = false; // si una planta esta mal, perdemos el bono
                }

                // madurez y diversidad
                if (plot.currentPlant.currentGrowth == GrowthState.madura)
                {
                    maturePlants++;
                    categoriesPresent.Add(plot.currentPlant.plantData.category);
                }
            }
        }

        int income = 0;

        // +1 de dinero por cada 2 plantas maduras
        income += maturePlants / 2;

        // +1 de dinero si tienes las 4 categorías maduras
        if (categoriesPresent.Count == 4)
        {
            income += 1;
            Debug.Log("¡Bono de Diversidad conseguido!");
        }

        // +1 si TODAS las plantas están bien puestas
        if (totalPlanted > 0 && allPlantsInCorrectSun)
        {
            income += 1;
            Debug.Log("¡Bono de Exposición Solar conseguido!");
        }

        return income;
    }

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

    #region Métodos privados (diarios)

    public IEnumerator AnimateDailyConsumptionAndConsume()
    {
        List<Coroutine> consumptionAnimations = new List<Coroutine>();

        // 1. Animar consumo en cada parcela plantada
        foreach (Plot plot in plotGrid)
        {
            if (plot.isPlanted && plot.currentPlant != null)
            {
                Coroutine animation = StartCoroutine(plot.AnimateDailyConsumptionAndChange());
                consumptionAnimations.Add(animation);
            }
        }

        // 2. Esperar a que TODAS terminen
        foreach (Coroutine anim in consumptionAnimations)
        {
            yield return anim;
        }

        // 3. Refrescar visuals
        foreach (Plot plot in plotGrid)
        {
            if (plot.isPlanted)
            {
                plot.UpdatePlotWaterVisuals();
                plot.UpdatePlotFertilizerVisuals();
                plot.UpdatePlotSolarExposureVisuals();
            }
        }

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
        int waterDemand;
        int fertilizerDemand;

        foreach (Plot plot in plotGrid)
        {
            if (plot.isPlanted && plot.currentPlant != null)
            {
                waterDemand = plot.currentPlant.GetWaterDemand();
                fertilizerDemand = plot.currentPlant.GetFertilizerDemand();

                // SALUD
                if (waterDemand > plot.currentWater || fertilizerDemand > plot.currentFertility)
                {
                    Debug.Log($"La parcela {plot.gridCoordinates} no puede cubrir las necesidades de su planta");
                    if (plot.currentPlant.DecreaseHealth()) // true si la planta ha muerto
                    {
                        PlantsDeath(plot);
                    }
                }
                else
                {
                    plot.currentPlant.IncreaseHealth();
                }
            }
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
            // Sequía probabilística
            Debug.Log($"[PlotsManager] Aplicando Sequía Probabilística (Intensidad: {currentWeather.intensity})");
            foreach (Plot plot in plotGrid)
            {
                ApplyProbabilisticDrought(plot, currentWeather.intensity);
                plot.UpdatePlotWaterVisuals();
            }
        }

        Debug.Log("Cambio de agua por evento meteorologico.");
    }

    private void ApplyProbabilisticDrought(Plot plot, int intensity)
    {
        float roll = UnityEngine.Random.Range(0f, 1f);
        int waterLoss = 0;

        if (intensity == 1)
        {
            if (roll <= 0.70f) waterLoss = -1;
        }
        else if (intensity == 2)
        {
            if (roll <= 0.40f) waterLoss = -1;
            else if (roll <= 0.70f) waterLoss = -2;
        }
        else // Intensidad 3
        {
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
                plot.ChangeFertility(-plot.currentPlant.GetFertilizerDemand());
                plot.UpdatePlotFertilizerVisuals();
            }
        }
        Debug.Log("Consumo de fertilizante diario hecho.");
    }

    #endregion

    #region Helpers para guardado/carga

    public IEnumerable<Plot> GetAllPlots()
    {
        foreach (var p in plotGrid) yield return p;
    }

    public Plot GetPlotAt(int x, int y)
    {
        if (x < 0 || x >= rows || y < 0 || y >= columns) return null;
        return plotGrid[x, y];
    }

    public void ClearAllPlots()
    {
        foreach (var p in plotGrid)
        {
            if (p == null) continue;
            if (p.currentPlant != null) Destroy(p.currentPlant.gameObject);
            p.currentPlant = null;
            p.isPlanted = false;
        }
        currentSelectedPlot = null;
    }

    // Reconstruye efectos de entorno (sombras) a partir de plantas ProvidesShade
    public void RebuildEnvironmentFromPlants()
    {
        // Primero limpia todas las sombras previas
        foreach (var plot in plotGrid)
        {
            if (plot != null)
            {
                plot.ResetShade(); // deberás añadir este método a Plot si no existe
            }
        }

        // Luego aplica solo las plantas que deben dar sombra
        foreach (var plot in plotGrid)
        {
            if (plot != null && plot.isPlanted && plot.currentPlant != null)
            {
                Plant p = plot.currentPlant;

                if (p.plantData.category == PlantCategory.ProvidesShade &&
                    p.GetGrowth() == GrowthState.madura) // 👈 solo si está madura
                {
                    GenerateShade(plot.gridCoordinates);
                }
            }
        }

        Debug.Log("[PlotsManager] RebuildEnvironmentFromPlants: sombras recalculadas correctamente.");
    }


    #endregion
}
*/