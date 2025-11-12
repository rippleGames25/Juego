using NUnit.Framework.Constraints;
using System.Linq;
using UnityEngine;

public enum GrowthState
{
    semilla,
    brote,
    joven,
    madura
}

public enum Health
{
    buena,
    moderada,
    mala
}

public class Plant : MonoBehaviour
{
    // Variables
    [SerializeField] public PlantType plantData;

    public GrowthState currentGrowth;
    public Health currentHealth;
    public int lifeDays;
    public bool hasAppliedEnvironmentEffect = false;
    public bool isDeath = false;

    protected Plot parentPlot;

    // Producer Type
    [SerializeField] protected int produceDays = 0;
    [SerializeField] public bool hasProduct = false;

    // Visuals
    protected SpriteRenderer spriteRenderer;
    [SerializeField] protected SpriteRenderer productSR;


    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null )
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
    public void InitializePlant(PlantType _type, Plot _parentPlot)
    {
        plantData = _type;
        lifeDays = 0;
        currentGrowth = GrowthState.semilla; // Comienza como semilla
        parentPlot = _parentPlot;
        currentHealth = Health.buena;
        isDeath = false;

        spriteRenderer.sprite = plantData.plantSprites[0];
    }

    // Metodo que Incrementa el estado de salud
    public void IncreaseHealth()
    {
        if (currentHealth == Health.mala)
        {
            currentHealth = Health.moderada;
        }
        else if (currentHealth == Health.moderada)
        {
            if (CheckSolarExposure()) 
            {
                currentHealth = Health.buena;
            }
            else
            {
                // Salud moderada porque la exposición no es la ideal
                Debug.Log($"La planta {plantData.plantName} tiene salud moderada por la exposición solar.");
            }
        }
        else
        {
            if (!CheckSolarExposure())
            {
                currentHealth = Health.moderada;
                // Salud moderada porque la exposición no es la ideal
                Debug.Log($"La planta {plantData.plantName} tiene salud moderada por la exposición solar.");
            }
        }

        UpdatePlantSprite();
    }

    // Metodo que Decrementa el estado de salud
    public bool DecreaseHealth()
    {
        if(currentHealth== Health.buena)
        {
            currentHealth= Health.moderada;
        } else if(currentHealth== Health.moderada) 
        {
            currentHealth= Health.mala;
        } else
        {
            isDeath = true;
            SFXManager.Instance?.PlayMarchita();
            UpdatePlantSprite();
            return true; // La planta ha muerto
        }

        SFXManager.Instance?.PlayMarchita();
        UpdatePlantSprite();
        return false;
    }

    // Metodo que actualiza la vida y el estado de crecimiento de la planta
    public void UpdateLifeDays()
    {
        lifeDays++;

        if(currentGrowth == GrowthState.madura)
        {
            return;
        } 
        else if (currentGrowth == GrowthState.semilla && lifeDays >= plantData.timeToSprout) 
        {
            currentGrowth++; // Brotar
            GameManager.Instance.CurrentBiodiversity++; // Sumamos uno a la biodiversidad
            SFXManager.Instance?.PlayCrece();
            Debug.Log($"La planta {plantData.plantName} ha brotado.");
        } 
        else if (currentGrowth == GrowthState.brote && lifeDays >= plantData.timeToGrow)
        {
            currentGrowth++; // Crecer hasta Joven
            SFXManager.Instance?.PlayCrece();
            Debug.Log($"La planta {plantData.plantName} ha crecido.");
        }
        else if (currentGrowth == GrowthState.joven && lifeDays >= plantData.timeToMature)
        {
            currentGrowth++; // Crecer hasta madura
            SFXManager.Instance?.PlayCrece();
            Debug.Log($"La planta {plantData.plantName} ha madurado.");

        }

        UpdatePlantSprite();
    }

    public bool CheckSolarExposure()
    {
        SolarExposure required = plantData.solarExposureDemand;
        SolarExposure current = parentPlot.GetCurrentSolarExposure();

        return current == required;
    }

    public virtual void ApplyDailyEffect()
    {

    }

    public void ForceKill()
    {
        isDeath = true;
        currentHealth = Health.mala;
        UpdatePlantSprite();
        SFXManager.Instance?.PlayMarchita();
    }

    #endregion



    // Metodos de visualización

    protected void UpdatePlantSprite()
    {
        if (isDeath) // Planta muerta
        {
            spriteRenderer.sprite = plantData.deathSprite;
            return;
        }

        if (currentGrowth == GrowthState.semilla) // Semilla
        {
            spriteRenderer.sprite = plantData.plantSprites[0];
            return;
        }

        // Logica para calcular el idx del sprite
        int growthBaseIndex = 0;
        int growthInt = (int)currentGrowth; //Crecimiento

        if (currentGrowth == GrowthState.brote)
        {
            growthBaseIndex = 1;
        }
        else if (currentGrowth == GrowthState.joven)
        {
            growthBaseIndex = 4;
        }
        else if (currentGrowth == GrowthState.madura)
        {
            growthBaseIndex = 7;
        }

        int healthOffset = (int) currentHealth; //Salud

        int finalIndex = growthBaseIndex + healthOffset;

        if (finalIndex < plantData.plantSprites.Length)
        {
            spriteRenderer.sprite = plantData.plantSprites[finalIndex];
        }
        else
        {
            Debug.LogError($"Plant: Índice de sprite calculado fuera de límites: {finalIndex}.");
        }
    }



}
