using UnityEngine;

enum GrowthState
{
    seed,
    sprouted,
    mature,
    death
}

enum Health
{
    bad,
    moderate, 
    good
}

public class Plant : MonoBehaviour
{
    // Variables
    [SerializeField] public PlantType plantData;

    private Vector2 plotCoordinates;
    private GrowthState currentGrowth;
    private Health currentHealth;
    private int lifeDays;
    

    //Constructor
    public Plant (Vector2 _plot, PlantType _plantData)
    {
        this.plotCoordinates = _plot;
        this.plantData = _plantData;
        
        this.currentHealth= Health.good; // good
        this.currentGrowth = GrowthState.seed; // seed
        lifeDays= 0;
    }

    // Public methods
    public void CalculateHealth()
    {

    }

    public void InitializePlant(Vector2 _plotCoordinates, PlantType _type)
    {
        plotCoordinates = _plotCoordinates;
        plantData = _type;
        lifeDays = 0;
        currentGrowth = GrowthState.seed; // Comienza como semilla
        currentHealth = Health.good; // Comienza con salud perfecta
    }

    public int ConsumeWater()
    {
        return plantData.waterDemand;
    }

    public int ConsumeFertilizer()
    {
        return plantData.fertilizerDemand;
    }

}
