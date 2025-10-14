using System;
using UnityEngine;

public enum SolarExposure
{
    Fullsun,
    PartShade,
    Shade
}

// This class represent a Plot in the game
public class Plot : MonoBehaviour
{
    // Variables
    // Posicion
    public Vector2Int gridCoordinates { get; set; }
    public Plant currentPlant { get; set; }

    public GameObject selectionBorder;


    private const int PLOT_LIMIT = 10;

    // Estado
    [Range(0, PLOT_LIMIT)] [SerializeField] public int currentWater;
    [Range(0, PLOT_LIMIT)] [SerializeField] public int currentFertility;
    [SerializeField] private SolarExposure currentSolarExposure;
    [SerializeField] public bool isPlanted;

    private SpriteRenderer sr;


    // Constructor
    public Plot(int _currentWater, int _currentFertility, int _currentSolarExposure, bool isPlanted)
    {
        this.currentWater = _currentWater;
        this.currentFertility = _currentFertility;
        this.currentSolarExposure = (SolarExposure)_currentSolarExposure;
        this.isPlanted = isPlanted;
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        selectionBorder = this.transform.GetChild(0).gameObject; // Obtenemos el contorno de seleccion

        UpdatePlotWaterVisuals();
        UpdatePlotFertilizerVisuals();
    }


    // Metodos privados
    


    // Public methods
    public void InitializePlot(int x, int y)
    {
        gridCoordinates= new Vector2Int(x, y);
        currentFertility = UnityEngine.Random.Range(5, 8);
        currentWater = UnityEngine.Random.Range(5, 8);
        currentSolarExposure = SolarExposure.Fullsun; // Temporalmente todas sol
        isPlanted = false; // Se inicializa vacia
        currentPlant = null; // No hay planta

        Debug.Log(this.ToString());
    }

    public void UpdatePlant()
    {
        // Calcular muertes

        currentPlant.UpdateLifeDays(); // Actualizar dias de vida

    }

    public void ChangeWater (int weatherWater)
    {
        int waterChange = weatherWater;

        if (this.isPlanted)
        {
            waterChange = waterChange - currentPlant.plantData.waterDemand; // Restamos agua consumida por la planta
        }

        this.currentWater  += waterChange; // Agua aportada por evento meteorologico

        this.currentWater = Mathf.Clamp(this.currentWater, 0, PLOT_LIMIT);

    }

    public void ChangeFertility(int newCurrentFertility)
    {
        this.currentFertility += newCurrentFertility;
        this.currentFertility = Mathf.Clamp(this.currentFertility, 0, PLOT_LIMIT);
    }

    public void ChangeSolarExposure(int newSolarExposure)
    {
        this.currentSolarExposure = (SolarExposure) newSolarExposure;

    }

    public void ChangePlantState(bool newIsPlanted)
    {
        this.isPlanted = newIsPlanted;
    }


    public void SelectPlot()
    {

        switch (GameManager.Instance.CurrentTool)
        {
            case ToolType.None:
                
                Debug.Log($"Parcela {this.gridCoordinates} seleccionada.");
                PlotsManager.Instance.PlotSelected(this);
                break;

            case ToolType.WateringCan:
                //Seleccionar parcela...

                if (GameManager.Instance.CurrentWater > 0 && currentWater < PLOT_LIMIT) // Tiene agua y la parcela no esta llena
                {
                    GameManager.Instance.CurrentWater--; // Cosume agua del deposito
                    this.currentWater++;
                    this.UpdatePlotWaterVisuals();
                    Debug.Log($"Parcela {this.gridCoordinates} regada -> {currentWater} de agua");
                }
                else if (GameManager.Instance.CurrentWater > 0 && currentWater >= PLOT_LIMIT) // Tiene agua pero la parcela etá llena
                {
                    Debug.Log($"No se puede regar, la parcela {gridCoordinates} está al máximo de agua.");
                }
                else // No queda agua
                {
                    Debug.Log("No te queda agua.");
                }
                break;

            case ToolType.FertilizerBag:
                //Seleccionar parcela...

                if (GameManager.Instance.CurrentFertilizer > 0 && currentFertility < PLOT_LIMIT)
                {
                    GameManager.Instance.CurrentFertilizer--; // Consume abono del deposito
                    this.currentFertility++;
                    this.UpdatePlotFertilizerVisuals();
                    Debug.Log($"Parcela {this.gridCoordinates} abonada -> {currentFertility} de abono");

                }
                else if (GameManager.Instance.CurrentFertilizer > 0 && currentFertility >= PLOT_LIMIT)
                {
                    Debug.Log($"No se puede abonar, la parcela {gridCoordinates} está al máximo de abono.");
                }
                else
                {
                    Debug.Log("No te queda abono");
                }

                break;

            case ToolType.Plant:
                if (!this.isPlanted)
                {
                    int plantType = ShopManager.Instance.selectedPlantType.idx;
                    GameManager.Instance.PlantSeed(this, plantType);

                }
                else
                {
                    Debug.Log($"Parcela {gridCoordinates} ocupada.");
                }
                break;
        }
    }



    // Metodos visualización
    public void UpdatePlotWaterVisuals()
    {
        Color newColor = sr.color;
        newColor.r = 1f - Mathf.Clamp01((float)this.currentWater / (PLOT_LIMIT * 2));

        sr.color = newColor;
    }
    public void UpdatePlotFertilizerVisuals()
    {
        Color newColor = sr.color;
        newColor.g = 1f - Mathf.Clamp01((float)this.currentFertility / (PLOT_LIMIT * 2));

        sr.color = newColor;
    }


    public override string ToString()
    {
        return $"Parcela {gridCoordinates} -> Agua: {currentWater}, Fertilidad: {currentFertility}, Exposicion Solar: {currentSolarExposure}";
    }
}
