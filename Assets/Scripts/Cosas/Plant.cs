using UnityEngine;

enum GrowthState
{
    seed,
    sprout,
    young,
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


    //private Vector2 plotCoordinates;
    private GrowthState currentGrowth;
    private Health currentHealth;
    private int lifeDays;

    //Visuals
    private SpriteRenderer spriteRenderer;

    //Constructor
    public Plant (PlantType _plantData)
    {
        this.plantData = _plantData;
        this.currentHealth= Health.good; // good
        this.currentGrowth = GrowthState.seed; // seed
        lifeDays= 0;
    }

    void Awake()
    {
        spriteRenderer= GetComponent<SpriteRenderer>();

        if(spriteRenderer == null )
        {
            Debug.LogError("Plant: no se encontro el spriterenderer");
        }
    }

    // Public methods
    public void InitializePlant(PlantType _type)
    {
        plantData = _type;
        lifeDays = 0;
        currentGrowth = GrowthState.seed; // Comienza como semilla
        currentHealth = Health.good; // Comienza con salud perfecta

        spriteRenderer.sprite = plantData.plantSprites[0];
    }

    public int ConsumeWater()
    {
        return plantData.waterDemand;
    }

    public int ConsumeFertilizer()
    {
        return plantData.fertilizerDemand;
    }

    public bool LowerHealth()
    {
        if(currentHealth== Health.good)
        {
            currentHealth= Health.moderate;
        } else if(currentHealth== Health.moderate) 
        {
            currentHealth= Health.bad;
        } else
        {
            return true; // La planta ha muerto
        }

        return false;
    }

    public void IncreaseHealth()
    {
        if (currentHealth == Health.bad)
        {
            currentHealth = Health.moderate;
        }
        else if (currentHealth == Health.moderate)
        {
            currentHealth = Health.good;
        }
        else
        {
            Debug.Log($"Planta sana");
        }
    }

    public void UpdateLifeDays()
    {
        lifeDays++;

        if(currentGrowth == GrowthState.mature)
        {
            return;
        } 
        else if (currentGrowth == GrowthState.seed && lifeDays >= plantData.timeToSprout) 
        {
            currentGrowth++; // Brotar
            UpdatePlantVisuals((int) currentGrowth);
            Debug.Log($"La planta {plantData.name} ha brotado.");
        } 
        else if (currentGrowth == GrowthState.sprout && lifeDays >= plantData.timeToGrow)
        {
            currentGrowth++; // Crecer hasta Joven
            UpdatePlantVisuals((int)currentGrowth);
            Debug.Log($"La planta {plantData.name} ha crecido.");
        }
        else if (currentGrowth == GrowthState.young && lifeDays >= plantData.timeToMature)
        {
            currentGrowth++; // Crecer hasta madura
            UpdatePlantVisuals((int)currentGrowth);
            Debug.Log($"La planta {plantData.name} ha madurado.");
        }
    }

    // Metodos de visualización
    private void UpdatePlantVisuals(int state)
    {
        spriteRenderer.sprite = plantData.plantSprites[state];
    }

}
