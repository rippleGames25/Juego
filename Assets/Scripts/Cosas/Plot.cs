using UnityEngine;


enum SolarExposure
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
    public Plant plantedPlant { get; set; }

    // Estado
    [Range(0, 10)] [SerializeField] private int currentWater;
    [Range(0, 10)] [SerializeField] private int currentFertility;
    [SerializeField] private SolarExposure currentSolarExposure;
    [SerializeField] private bool isPlanted;
    


    // Constructor
    public Plot(int _currentWater, int _currentFertility, int _currentSolarExposure, bool isPlanted)
    {
        this.currentWater = _currentWater;
        this.currentFertility = _currentFertility;
        this.currentSolarExposure = (SolarExposure)_currentSolarExposure;
        this.isPlanted = isPlanted;
    }

    public void InitializePlot(int x, int y)
    {
        gridCoordinates= new Vector2Int(x, y);
        currentFertility = Random.Range(5, 8);
        currentWater = Random.Range(5, 8);
        currentSolarExposure = SolarExposure.Fullsun; // Temporalmente todas sol
        isPlanted = false; // Se inicializa vacia

        Debug.Log(this.ToString());
    }



    // Public methods
    public void ChangeWater (int newCurrentWater)
    {
        this.currentWater  += newCurrentWater;
    }
    public void ChangeFertily(int newCurrentFertility)
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

}
