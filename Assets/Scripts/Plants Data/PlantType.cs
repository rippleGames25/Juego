using UnityEngine;

public enum PlantCategory
{
    None,
    PollinatorAttractor,
    ProvidesShade,
    Producer,
    WildlifeRefuge
}


[CreateAssetMenu(fileName = "NewPlantType", menuName = "Plant/Plant Type Config", order = 1)]
public class PlantType : ScriptableObject
{
    // Variables
    public string name = "Plantita";
    public string description = "Lorem Ipsun";

    [Header("Características")]
    public int waterDemand = 2;
    public int fertilizerDemand = 2;
    public SolarExposure solarExposureDemand = SolarExposure.Fullsun;

    [Header("Tiempos de crecimiento (días)")]
    public int timeToSprout = 3;
    public int timeToGrow = 6;
    public int timeToMature = 9;

    [Header("Categoria")]
    public PlantCategory category = PlantCategory.PollinatorAttractor;

    [Header("Precio")]
    public int price = 50; // Precio en la tienda

    [Header("Sprites")]
    [SerializeField] public Sprite[] plantSprites;

    public GameObject seedPrefab;



    // Sensibilidad..
    // Característica

}
