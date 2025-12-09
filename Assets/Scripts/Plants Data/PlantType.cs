using UnityEngine;
using UnityEngine.Localization;

public enum PlantCategory
{
    PollinatorAtractor,
    ShadeProvider,
    Producer,
    WildlifeRefuge
}

[CreateAssetMenu(fileName = "NewPlantType", menuName = "Plant/Plant Type Config", order = 1)]
public class PlantType : ScriptableObject
{
    [Header("Info")]
    public int idx = 0;
    public LocalizedString plantName;
    public string scientificName;
    public LocalizedString description;
    public LocalizedString categoryInfo;
    public LocalizedString petals;


    [Header("Características")]
    public int waterDemand = 2;
    public int fertilizerDemand = 2;
    public SolarExposure solarExposureDemand = SolarExposure.Soleado;

    [Header("Tiempos de crecimiento (días)")]
    public int timeToSprout = 3;
    public int timeToGrow = 6;
    public int timeToMature = 9;

    [Header("Categoria")]
    public PlantCategory category = PlantCategory.PollinatorAtractor;
    public LocalizedString categoryText;

    [Header("Características")]
    // Productores
    public int timeToProduce = 0;

    [Header("Precio")]
    public int price = 1; // Precio en la tienda

    [Header("Sprites")]
    [SerializeField] public Sprite shopSprite;
    [SerializeField] public Sprite[] plantSprites;
    [SerializeField] public Sprite deathSprite;

    public GameObject refugeVisualPrefab;

    public string TypeToString()
    {
        return categoryText.GetLocalizedString();
    }

    public string InfoToString()
    {
        string msg = "";

        if (category == PlantCategory.Producer)
        {
            int productPrice = price / 2;
            msg = categoryInfo.GetLocalizedString() + productPrice + petals.GetLocalizedString();
        }
        else
        {
            msg = categoryInfo.GetLocalizedString();
        }

        return msg;
    }

}
