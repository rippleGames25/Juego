using UnityEngine;

public enum GrowthState
{
    seed,
    sprout,
    young,
    mature,
    death
}

public enum Health
{
    bad,
    moderate, 
    good
}

public class Plant : MonoBehaviour
{
    // Variables
    [SerializeField] public PlantType plantData;

    public GrowthState currentGrowth;
    public Health currentHealth;
    public int lifeDays;

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
        spriteRenderer = GetComponent<SpriteRenderer>();

        if(spriteRenderer == null )
        {
            Debug.LogError("Plant: no se encontro el spriterenderer");
        }
    }

    #region Public methods
    public int GetWaterDemand()
    {
        return plantData.waterDemand;
    }

    public int GetFertilizerDemand()
    {
        return plantData.fertilizerDemand;
    }

    // Metodo para inicializar una planta
    public void InitializePlant(PlantType _type)
    {
        plantData = _type;
        lifeDays = 0;
        currentGrowth = GrowthState.seed; // Comienza como semilla
        currentHealth = Health.good; // Comienza con salud perfecta

        spriteRenderer.sprite = plantData.plantSprites[0];
    }

    // Metodo que Incrementa el estado de salud
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

    // Metodo que Decrementa el estado de salud
    public bool DecreaseHealth()
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

    // Metodo que actualiza la vida y el estado de crecimiento de la planta
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
            Debug.Log($"La planta {plantData.plantName} ha brotado.");
        } 
        else if (currentGrowth == GrowthState.sprout && lifeDays >= plantData.timeToGrow)
        {
            currentGrowth++; // Crecer hasta Joven
            UpdatePlantVisuals((int)currentGrowth);
            Debug.Log($"La planta {plantData.plantName} ha crecido.");
        }
        else if (currentGrowth == GrowthState.young && lifeDays >= plantData.timeToMature)
        {
            currentGrowth++; // Crecer hasta madura
            UpdatePlantVisuals((int)currentGrowth);
            Debug.Log($"La planta {plantData.plantName} ha madurado.");
        }
    }



    #endregion

    // Metodos de visualización
    private void UpdatePlantVisuals(int state)
    {
        spriteRenderer.sprite = plantData.plantSprites[state];
    }

}
