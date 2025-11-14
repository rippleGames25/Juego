using UnityEngine;

public enum PlantCategory
{
    PollinatorAttractor,
    ProvidesShade,
    Producer,
    WildlifeRefuge
}

[CreateAssetMenu(fileName = "NewPlantType", menuName = "Plant/Plant Type Config", order = 1)]
public class PlantType : ScriptableObject
{
    [Header("Info")]
    public int idx = 0;
    public string plantName = "Plantita";
    public string scientificName = "Lorem Ipsum";
    public string description = "Lorem Ipsum";

    [Header("Características")]
    public int waterDemand = 2;
    public int fertilizerDemand = 2;
    public SolarExposure solarExposureDemand = SolarExposure.Soleado;

    [Header("Tiempos de crecimiento (días)")]
    public int timeToSprout = 3;
    public int timeToGrow = 6;
    public int timeToMature = 9;

    [Header("Categoria")]
    public PlantCategory category = PlantCategory.PollinatorAttractor;

    [Header("Características")]
    // Atractores de polinizadores
    // Tipo de polinizador

    // Proporcionan sombra
    public int shadeSize = 0;

    // Productores
    public int timeToProduce = 0;

    // Refugio de fauna
    // Tipo de fauna
    

    [Header("Precio")]
    public int price = 1; // Precio en la tienda

    [Header("Sprites")]
    [SerializeField] public Sprite shopSprite;
    [SerializeField] public Sprite[] plantSprites;
    [SerializeField] public Sprite deathSprite;

    public GameObject refugeVisualPrefab;
}
