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

    public void DailyUpdatePlotsWater(int _waterChange)
    {
        foreach (Plot plot in plotGrid) 
        {
            plot.ChangeWater(_waterChange); // Agua segun evento meteorologico

            if (plot.isPlanted)
            {
                plot.ChangeWater(plot.currentPlant.ConsumeWater()); // Agua que consume la planta
            }
            
            plot.UpdatePlotVisuals();
        }
        Debug.Log("Consumo de agua diario hecho + cambio de agua por evento meteorologico.");
    }

    public void DailyUpdatePlotsFertilizer()
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
