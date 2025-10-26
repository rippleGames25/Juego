using NUnit.Framework.Constraints;
using System.Linq;
using UnityEngine;

public enum GrowthState
{
    semilla,
    brote,
    joven,
    madura,
    muerta
}

public enum Health
{
    mala,
    moderada, 
    buena
}

public class Plant : MonoBehaviour
{
    // Variables
    [SerializeField] public PlantType plantData;

    public GrowthState currentGrowth;
    public Health currentHealth;
    public int lifeDays;
    public bool hasAppliedEnvironmentEffect = false;

    // Producer Type
    [SerializeField] private int produceDays = 0;
    [SerializeField] public bool hasProduct = false;

    // Visuals
    private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer productSR;

    //Constructor
    public Plant (PlantType _plantData)
    {
        this.plantData = _plantData;
        this.currentHealth= Health.buena; // good
        this.currentGrowth = GrowthState.semilla; // seed
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
        currentGrowth = GrowthState.semilla; // Comienza como semilla
        currentHealth = Health.buena; // Comienza con salud perfecta

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
            currentHealth = Health.buena;
        }
        else
        {
            Debug.Log($"Planta sana");
        }

        UpdatePlantHealthVisuals(currentHealth);
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
            return true; // La planta ha muerto
        }

        UpdatePlantHealthVisuals(currentHealth);
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
            UpdatePlantGrowthVisuals((int) currentGrowth);
            GameManager.Instance.CurrentBiodiversity++; // Sumamos uno a la biodiversidad
            Debug.Log($"La planta {plantData.plantName} ha brotado.");
        } 
        else if (currentGrowth == GrowthState.brote && lifeDays >= plantData.timeToGrow)
        {
            currentGrowth++; // Crecer hasta Joven
            UpdatePlantGrowthVisuals((int)currentGrowth);
            Debug.Log($"La planta {plantData.plantName} ha crecido.");
        }
        else if (currentGrowth == GrowthState.joven && lifeDays >= plantData.timeToMature)
        {
            currentGrowth++; // Crecer hasta madura
            UpdatePlantGrowthVisuals((int)currentGrowth);
            Debug.Log($"La planta {plantData.plantName} ha madurado.");

        }
    }



    #endregion

    #region Producers Methods
    public void ProduceCycle()
    {
        if (hasProduct == true) return;

        produceDays++;

        if(produceDays == plantData.timeToProduce)
        {
            hasProduct = true;
            produceDays = 0;
            UpdateProductVisuals();
        } 
    }

    public void CollectProduct()
    {
        GameManager.Instance.CurrentMoney++; // De momento suma 1 petalo
        hasProduct = false;
        UpdateProductVisuals();
    }


    #endregion

    // Metodos de visualización
    private void UpdatePlantGrowthVisuals(int state)
    {
        spriteRenderer.sprite = plantData.plantSprites[state];
    }

    private void UpdateProductVisuals()
    {
        if (plantData.category == PlantCategory.Producer && hasProduct)
        {
            spriteRenderer.sprite = plantData.plantSprites.Last();
        }
        else if(plantData.category == PlantCategory.Producer && !hasProduct)
        {
            spriteRenderer.sprite = plantData.plantSprites[3];
        }
    }

    private void UpdatePlantHealthVisuals(Health _currentHealth)
    {
        switch (_currentHealth)
        {
            case Health.mala:
                spriteRenderer.color = Color.red;
                break;
            case Health.moderada:
                spriteRenderer.color = Color.yellow;
                break;
            case Health.buena:
                spriteRenderer.color = Color.white;
                break;
        }
    }

}
