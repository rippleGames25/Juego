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
    private Plot plot;
    private PlantType plantType;
    private GrowthState currentGrowth;
    private Health currentHealth;
    private int lifeDays;
    

    //Constructor
    public Plant (Plot _plot, PlantType _plantType)
    {
        this.plot = _plot;
        this.plantType = _plantType;
        
        this.currentHealth= Health.good; // good
        this.currentGrowth = GrowthState.seed; // seed
        lifeDays= 0;
    }

    // Public methods
    public void CalculateHealth()
    {

    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
