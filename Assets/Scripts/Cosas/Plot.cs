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
    private int currentWater;
    private int currentFertility;
    private SolarExposure currentSolarExposure;
    private bool isPlanted;


    // Constructor
    public Plot(int _currentWater, int _currentFertility, int _currentSolarExposure, bool isPlanted)
    {
        this.currentWater = _currentWater;
        this.currentFertility = _currentFertility;
        this.currentSolarExposure = (SolarExposure)_currentSolarExposure;
        this.isPlanted = isPlanted;
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

}
