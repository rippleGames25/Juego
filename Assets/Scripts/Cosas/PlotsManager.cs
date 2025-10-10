using UnityEngine;
using System.Collections.Generic;
using System;

public class PlotsManager : MonoBehaviour
{
    [SerializeField] private GameObject plotPrefab;

    [SerializeField] private int rows = 6;
    [SerializeField] private int columns = 5;
    [SerializeField] private float spacing = 1.5f; // Espacio entre parcelas

    private Plot[,] plotGrid;

    void Start()
    {
        plotGrid = new Plot[rows, columns];
    }

    // Metodos publicos
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

    // Metodos publicos
    public void DailyUpdate(DailyWeather _currentWeather)
    {
        UpdatePlants();
        DailyUpdatePlants();
        DailyUpdatePlotsFertilizer();

        DailyUpdateWeatherWater(_currentWeather.waterChange);
    }

    public void PlantsDeath(Plant plantToDeath)
    {
        GameManager.Instance.PlantsDeath(plantToDeath);
    }

    // Metodos privados

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
                waterDemand = plot.currentPlant.ConsumeWater();
                fertilizerDemand = plot.currentPlant.ConsumeFertilizer();

                if (waterDemand > plot.currentWater || fertilizerDemand > plot.currentFertility)
                {
                    Debug.Log($"La parcela {plot.gridCoordinates} no puede cubrir las necesidades de su planta");
                    if (plot.currentPlant.LowerHealth()) // true si la planta ha muerto
                    {
                        PlantsDeath(plot.currentPlant);
                        plot.currentPlant = null;
                        plot.isPlanted= false;
                    }

                } else
                {
                    plot.currentPlant.IncreaseHealth();
                }

                plot.ChangeWater(waterDemand); // Agua que consume la planta
                plot.ChangeFertility(fertilizerDemand); // Abono que consume la planta

                plot.UpdatePlotWaterVisuals();
                plot.UpdatePlotFertilizerVisuals();
            }

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
                plot.ChangeFertility(-plot.currentPlant.ConsumeFertilizer()); // Agua que consume la planta
                Debug.Log("—am—am");
            }
        }
        Debug.Log("Consumo de fertilizante diario hecho.");
    }


}
