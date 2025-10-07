using UnityEngine;

[CreateAssetMenu(fileName = "NewPlantType", menuName = "Plant/Plant Type Config", order = 1)]
public class PlantType : ScriptableObject
{
    // Variables
    public string name = "Planta Test";

    public GameObject seedPrefab;
    

    [Header("Caracter�sticas")]
    public int waterDemand = 2;
    public int fertilizerDemand = 2;
    public SolarExposure solarExposureDemand = SolarExposure.Fullsun;
    public bool isTall = false; // false = small, true = tall

    [Header("Tiempos de crecimiento (d�as)")]
    public int timeToSprout = 3;  
    public int timeToMature = 3; 


    // Sensibilidad..
    // Atraccion...
    // Da frutos...

}
