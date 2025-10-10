using UnityEngine;

[CreateAssetMenu(fileName = "NewPlantType", menuName = "Plant/Plant Type Config", order = 1)]
public class PlantType : ScriptableObject
{
    // Variables
    public string name = "Plantita";

    public GameObject seedPrefab;

    [Header("Precio")]
    public int price = 50; // Precio en la tienda
    

    [Header("Características")]
    public int waterDemand = 2;
    public int fertilizerDemand = 2;
    public SolarExposure solarExposureDemand = SolarExposure.Fullsun;
    public bool isTall = false; // false = small, true = tall

    [Header("Tiempos de crecimiento (días)")]
    public int timeToSprout = 3;
    public int timeToGrow = 6;
    public int timeToMature = 9;

    [Header("Sprites")]
    [SerializeField] public Sprite[] plantSprites;


    // Sensibilidad..
    // Atraccion...
    // Da frutos...

}
