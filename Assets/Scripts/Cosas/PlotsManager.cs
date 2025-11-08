using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.EventSystems;
using System.Collections;

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

    public IEnumerator AnimateDailyConsumptionAndConsume()
    {
        List<Coroutine> consumptionAnimations = new List<Coroutine>();

        // 1. Iterar e INICIAR la Coroutine de animación y consumo en cada parcela plantada
        foreach (Plot plot in plotGrid)
        {
            if (plot.isPlanted && plot.currentPlant != null)
            {
                // Iniciamos la Coroutine y la guardamos para esperar
                Coroutine animation = StartCoroutine(plot.AnimateDailyConsumptionAndChange());
                consumptionAnimations.Add(animation);
            }
        }

        // 2. Esperar a que TODAS las Coroutines terminen (animación simultánea)
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
        int waterDemand;
        int fertilizerDemand;

        foreach (Plot plot in plotGrid)
        {
            if (plot.isPlanted && plot.currentPlant != null)
            {
                waterDemand = plot.currentPlant.GetWaterDemand();
                fertilizerDemand = plot.currentPlant.GetFertilizerDemand();

                // Lógica de SALUD: Comprobar si la parcela puede cubrir las necesidades
                if (waterDemand > plot.currentWater || fertilizerDemand > plot.currentFertility)
                {
                    Debug.Log($"La parcela {plot.gridCoordinates} no puede cubrir las necesidades de su planta");
                    if (plot.currentPlant.DecreaseHealth()) // true si la planta ha muerto
                    {
                        PlantsDeath(plot); // Llama a la muerte
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

    public void DailyUpdateWeatherWater(int _waterChange)
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
