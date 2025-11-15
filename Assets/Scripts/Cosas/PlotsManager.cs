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

    [Header("Plagas")]
    [SerializeField] [Range(0f, 1f)] private float dailyPlagueOutbreakChance = 0.1f; // 10% de probabilidad de un brote nuevo cada día
    [SerializeField] private GameObject plaguePrefab;

    // Events
    public event Action<Plot> OnPlotSelected;
    public event Action OnPlotUnselected;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            plotGrid = new Plot[rows, columns]; // Inicializar Array
        }
        else
        {
            Destroy(gameObject);
        }
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

    public int CalculateCurrentBiodiversity()
    {
        HashSet<PlantType> uniquePlantSpecies = new HashSet<PlantType>();
        bool hasPollinators = false;
        bool hasWildlife = false;

        if (plotGrid == null) return 0;

        foreach (Plot plot in plotGrid)
        {
            if (plot == null) continue;

            // 1. Contar especies de plantas únicas
            if (plot.isPlanted && plot.currentPlant != null &&
                !plot.currentPlant.isDeath &&
                plot.currentPlant.currentGrowth >= GrowthState.brote) // Contamos desde que es brote
            {
                uniquePlantSpecies.Add(plot.currentPlant.plantData);
            }

            // 2. Comprobar si hay fauna presente
            if (plot.IsPollinated)
            {
                hasPollinators = true;
            }
            if (plot.IsProtected) // IsProtected usa refugeSourceCount
            {
                hasWildlife = true;
            }
        }

        // Calcular puntuación final
        int biodiversityScore = uniquePlantSpecies.Count;

        if (hasPollinators)
        {
            biodiversityScore++;
            Debug.Log("Biodiversidad: +1 por Polinizadores");
        }

        if (hasWildlife)
        {
            biodiversityScore++;
            Debug.Log("Biodiversidad: +1 por Refugio de Fauna");
        }

        return biodiversityScore;
    }

    private List<Plot> GetNeighborPlots(Vector2Int coords)
    {
        List<Plot> neighbors = new List<Plot>();
        int x = coords.x;
        int y = coords.y;
        int sizeX = plotGrid.GetLength(0);
        int sizeY = plotGrid.GetLength(1);

        Vector2Int[] neighborCoords = new Vector2Int[]
        {
            new Vector2Int(x - 1, y), // Izquierda
            new Vector2Int(x + 1, y), // Derecha
            new Vector2Int(x, y - 1), // Abajo
            new Vector2Int(x, y + 1)  // Arriba
        };

        foreach (var coord in neighborCoords)
        {
            if (coord.x >= 0 && coord.x < sizeX && coord.y >= 0 && coord.y < sizeY)
            {
                if (plotGrid[coord.x, coord.y] != null)
                {
                    neighbors.Add(plotGrid[coord.x, coord.y]);
                }
            }
        }
        return neighbors;
    }

    public void PlantsDeath(Plot plotToDeath)
    {
        SFXManager.Instance?.PlayMuerte();
        Debug.Log($"La planta {plotToDeath.currentPlant.plantData.plantName} ha muerto porque no has cubierto sus necesidades");

        // Retirar efectos de la planta
        if (plotToDeath.currentPlant.plantData.category == PlantCategory.ProvidesShade) RemoveShade(plotToDeath.gridCoordinates);
        if (plotToDeath.currentPlant.plantData.category == PlantCategory.PollinatorAttractor) RemovePollination(plotToDeath.gridCoordinates);
        if (plotToDeath.currentPlant.plantData.category == PlantCategory.WildlifeRefuge) RemoveRefuge(plotToDeath.gridCoordinates);

        GameManager.Instance.AddPenalty(1);
        Debug.Log("Se ha restado 1 pétalo de tu economía total");

        GameManager.Instance.ReportPlantDeath();

        GameManager.Instance.UpdateBiodiversityScore();
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

    #region Pollinator Methods
    public void GeneratePollination(Vector2Int _plantCoordinates)
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
                plotGrid[coord.x, coord.y].AddPollinatorSource();
            }
        }
    }

    public void RemovePollination(Vector2Int _plantCoordinates)
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
                plotGrid[coord.x, coord.y].RemovePollinatorSource();
            }
        }
    }

    #endregion

    #region Wildlife Refuge Methods
    public void GenerateRefuge(Vector2Int _plantCoordinates)
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
                plotGrid[coord.x, coord.y].AddRefugeSource();
            }
        }
    }

    public void RemoveRefuge(Vector2Int _plantCoordinates)
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
                plotGrid[coord.x, coord.y].RemoveRefugeSource();
            }
        }
    }

    #endregion

    #region Plague Methods
    public void DailyPlagueUpdate()
    {
        // 1. Vemos si alguna planta se
        foreach (Plot plot in plotGrid)
        {
            if (!plot.isPlanted || plot.currentPlant == null) continue;

            if (plot.IsProtected && plot.currentPlant.isPlagued)
            {
                plot.currentPlant.CurePlague();
            }
        }

        // 2. Extender plagas existentes
        HashSet<Plant> plantsToInfect = new HashSet<Plant>();

        foreach (Plot plot in plotGrid)
        {
            // Buscamos una planta que sea un foco de infección
            if (plot != null && plot.isPlanted && plot.currentPlant != null &&
                plot.currentPlant.isPlagued && !plot.currentPlant.isDeath)
            {
                // Revisamos sus vecinos
                List<Plot> neighbors = GetNeighborPlots(plot.gridCoordinates);

                foreach (Plot neighborPlot in neighbors)
                {
                    if (neighborPlot.isPlanted && neighborPlot.currentPlant != null &&      // 1. Hay planta
                        !neighborPlot.currentPlant.isDeath &&                               // 2. No está muerta
                        !neighborPlot.currentPlant.isPlagued &&                             // 3. No está ya infectada
                        !neighborPlot.IsProtected &&                                        // 4. No esta protegida
                        neighborPlot.currentPlant.plantData == plot.currentPlant.plantData) // 5. Es de la misma especie
                    {
                        Debug.Log($"Plaga: Propagación con éxitp de {plot.gridCoordinates} a {neighborPlot.gridCoordinates}");
                        plantsToInfect.Add(neighborPlot.currentPlant);
                    }
                }
            }
        }

        // Infectamos plantas de la lista
        foreach (Plant plant in plantsToInfect)
        {
            plant.InfectWithPlague(plaguePrefab);
        }

        // 3. Posible nuevo brote (aleatorio)
        if (UnityEngine.Random.Range(0f, 1f) <= dailyPlagueOutbreakChance)
        {
            // Buscamos plantas que se puedan infectar
            List<Plot> possibleTargets = new List<Plot>(); 
            foreach (Plot plot in plotGrid)
            {
                // Debe tener una planta, no estar muerta, no estar infectada Y no estar protegida
                if (plot != null && plot.isPlanted && plot.currentPlant != null &&
                    !plot.currentPlant.isDeath &&
                    !plot.currentPlant.isPlagued &&
                    !plot.IsProtected)
                {
                    possibleTargets.Add(plot);
                }
            }

            if (possibleTargets.Count > 0)
            {
                // Infectar una planta aleatoria de la lista de objetivos
                Plot target = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
                target.currentPlant.InfectWithPlague(plaguePrefab);
                Debug.LogWarning($"Ha surgido un nuevo brote de plaga en la parcela {target.gridCoordinates}");
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
        float baseDeathProb = currentWeather.deathProbability; // ej. 0.2, 0.4, o 0.6

        if (currentWeather.type == WeatherType.Soleado)
        {
            Debug.Log($"[PlotsManager] Aplicando Sequía Probabilística (Intensidad: {currentWeather.intensity})");
            foreach (Plot plot in plotGrid)
            {
                if (plot == null) continue;
                ApplyProbabilisticDrought(plot, currentWeather.intensity);
                plot.UpdatePlotWaterVisuals();
            }
        }
        else
        {
            // esto para lluvia y granizo
            foreach (Plot plot in plotGrid)
            {
                if (plot == null) continue;
                plot.ChangeWater(currentWeather.waterChange);
                plot.UpdatePlotWaterVisuals();
            }
        }

        if (baseDeathProb > 0)
        {
            Debug.Log($"[PlotsManager] ¡Evento de muerte detectado! Tipo: {currentWeather.type}, Prob. Base: {baseDeathProb * 100}%");

            foreach (Plot plot in plotGrid.Cast<Plot>().Where(p => p != null).ToList())
            {
                if (plot.isPlanted && plot.currentPlant != null && !plot.currentPlant.isDeath)
                {
                    Plant plant = plot.currentPlant;
                    float finalDeathProb = 0f;

                    if (currentWeather.type == WeatherType.Granizo)
                    {
                        switch (plant.currentHealth)
                        {
                            case Health.buena:
                                finalDeathProb = 0f; // Si tiene buena salud no puede matar
                                break;
                            case Health.moderada:
                                finalDeathProb = baseDeathProb * 0.5f; // 50% de la prob. base
                                break;
                            case Health.mala:
                                finalDeathProb = baseDeathProb * 1.5f; // 150% de la prob. base
                                break;
                        }
                    } else if(currentWeather.type == WeatherType.Lluvia && currentWeather.intensity == 3)
                    {
                        switch (plant.currentHealth)
                        {
                            case Health.buena:
                                finalDeathProb = 0f; // Si tiene buena salud no puede matar
                                break;
                            case Health.moderada:
                                finalDeathProb = baseDeathProb * 0.5f; // 50% de la prob. base
                                break;
                            case Health.mala:
                                finalDeathProb = baseDeathProb;
                                break;
                        }
                    }

                    // probabilidad no mayor al 95%
                    finalDeathProb = Mathf.Clamp(finalDeathProb, 0f, 0.95f);

                    if (finalDeathProb > 0)
                    {
                        float roll = UnityEngine.Random.Range(0f, 1f); // (0.0 a 1.0)

                        if (roll <= finalDeathProb)
                        {
                            // ¡Muerte!
                            Debug.LogWarning($"¡Planta {plant.plantData.plantName} ({plant.currentHealth}) ha muerto por {currentWeather.type}! (Tirada: {roll} <= Prob: {finalDeathProb})");

                            plant.ForceKill(); // Usa el método de muerte instantánea
                            PlantsDeath(plot); // Aplica penalizaciones [cite: PlotsManager.cs]
                        }
                    }
                }
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
