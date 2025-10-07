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


    private const int PLOT_LIMIT = 10;

    // Estado
    [Range(0, PLOT_LIMIT)] [SerializeField] private int currentWater;
    [Range(0, PLOT_LIMIT)] [SerializeField] private int currentFertility;
    [SerializeField] private SolarExposure currentSolarExposure;
    [SerializeField] public bool isPlanted;

    // Constructor
    public Plot(int _currentWater, int _currentFertility, int _currentSolarExposure, bool isPlanted)
    {
        this.currentWater = _currentWater;
        this.currentFertility = _currentFertility;
        this.currentSolarExposure = (SolarExposure)_currentSolarExposure;
        this.isPlanted = isPlanted;
    }

    void Start()
    {
        UpdatePlotVisuals();
    }


    // Metodos privados
    public void OnMouseDown()
    {
        if (GameManager.Instance == null) return;

        switch (GameManager.Instance.CurrentTool) 
        {
            case ToolType.None:
                // Seleccionar parcela...
                Debug.Log($"Parcela {this.gridCoordinates} seleccionada.");
                break;

            case ToolType.WateringCan:
                //Seleccionar parcela...

                if (GameManager.Instance.CurrentWater > 0 && currentWater < PLOT_LIMIT) // Tiene agua y la parcela no esta llena
                {
                    GameManager.Instance.CurrentWater--; // Cosume agua del deposito
                    this.currentWater++;
                    this.UpdatePlotVisuals();
                    Debug.Log($"Parcela {this.gridCoordinates} regada -> {currentWater} de agua");
                } else if(GameManager.Instance.CurrentWater > 0 && currentWater >= PLOT_LIMIT) // Tiene agua pero la parcela etá llena
                {
                    Debug.Log($"No se puede regar, la parcela {gridCoordinates} está al máximo de agua.");
                } else // No queda agua
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
                    Debug.Log($"Parcela {this.gridCoordinates} abonada -> {currentFertility} de abono");

                } else if(GameManager.Instance.CurrentFertilizer > 0 && currentFertility > PLOT_LIMIT)
                {
                    Debug.Log($"No se puede abonar, la parcela {gridCoordinates} está al máximo de abono.");
                } else
                {
                    Debug.Log("No te queda abono");
                }

                break;

            case ToolType.Plant:
                if (!this.isPlanted)
                {
                    GameManager.Instance.PlantSeed(this);

                } else 
                {
                    Debug.Log($"Parcela {gridCoordinates} ocupada.");
                }
                break;
        }
    }



    // Public methods
    public void InitializePlot(int x, int y)
    {
        gridCoordinates= new Vector2Int(x, y);
        currentFertility = Random.Range(5, 8);
        currentWater = Random.Range(5, 8);
        currentSolarExposure = SolarExposure.Fullsun; // Temporalmente todas sol
        isPlanted = false; // Se inicializa vacia
        currentPlant = null; // No hay planta

        Debug.Log(this.ToString());
    }

    public void ChangeWater (int weatherWater)
    {
        int waterChange = weatherWater;

        if (this.isPlanted)
        {
            waterChange = waterChange - currentPlant.plantData.waterDemand; // Restamos agua consumida por la planta
        }

        this.currentWater  += waterChange; // Agua aportada por evento meteorologico
        
    }
    public void ChangeFertility(int newCurrentFertility)
    {
        this.currentFertility += newCurrentFertility;
    }
    public void ChangeSolarExposure(int newSolarExposure)
    {
        this.currentSolarExposure = (SolarExposure) newSolarExposure;

    }
    public void ChangePlantState(bool newIsPlanted)
    {
        this.isPlanted = newIsPlanted;
    }

    public override string ToString()
    {
        return $"Parcela {gridCoordinates} -> Agua: {currentWater}, Fertilidad: {currentFertility}, Exposicion Solar: {currentSolarExposure}";
    }

    // Metodos visualización
    public void UpdatePlotVisuals()
    {
        SpriteRenderer sr = this.gameObject.GetComponent<SpriteRenderer>();
        Color newColor = sr.color;
        newColor.r = 1f - Mathf.Clamp01((float) this.currentWater/(PLOT_LIMIT*2));

        sr.color = newColor;
    }

}
