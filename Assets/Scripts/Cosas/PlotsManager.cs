using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;

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

    public void DailyUpdate(DailyWeather _currentWeather)
    {
        UpdatePlants();
        DailyUpdatePlants();
        DailyUpdatePlotsFertilizer();

        DailyUpdateWeatherWater(_currentWeather.waterChange);
    }

    public void PlantsDeath(Plot plotToDeath)
    {
        if (plotToDeath.currentPlant.plantData.category == PlantCategory.ProvidesShade) RemoveShade(plotToDeath.gridCoordinates);

        GameManager.Instance.PlantsDeath(plotToDeath.currentPlant);
        plotToDeath.currentPlant = null;
        plotToDeath.isPlanted = false;
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
    private void UpdatePlants()
    {
        foreach (Plot plot in plotGrid)
        {
            if (plot.isPlanted) // Solo actualizar las parcelas con planta
            {
                plot.UpdatePlant();
            }
        }
    }

    private void DailyUpdatePlants()
    {
        int waterDemand;
        int fertilizerDemand;

        foreach (Plot plot in plotGrid)
        {
            if (plot.isPlanted) // Si esta plantada
            {
                waterDemand = plot.currentPlant.GetWaterDemand();
                fertilizerDemand = plot.currentPlant.GetFertilizerDemand();

                if (waterDemand > plot.currentWater || fertilizerDemand > plot.currentFertility)
                {
                    Debug.Log($"La parcela {plot.gridCoordinates} no puede cubrir las necesidades de su planta");
                    if (plot.currentPlant.DecreaseHealth()) // true si la planta ha muerto
                    {
                        PlantsDeath(plot);
                    }

                } else
                {
                    plot.currentPlant.IncreaseHealth();
                }

                plot.ChangeWater(-waterDemand); // Agua que consume la planta
                plot.ChangeFertility(-fertilizerDemand); // Abono que consume la planta

            }

            plot.UpdatePlotWaterVisuals();
            plot.UpdatePlotFertilizerVisuals();
            plot.UpdatePlotSolarExposureVisuals();
        }

        Debug.Log("Consumo de agua y abono diario de plantas hecho.");
    }

    private void DailyUpdateWeatherWater(int _waterChange)
    {
        foreach (Plot plot in plotGrid) 
        {
            plot.ChangeWater(_waterChange); // Agua segun evento meteorologico
            
            plot.UpdatePlotWaterVisuals();
        }

        Debug.Log("Cambio de agua por evento meteorologico.");
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
