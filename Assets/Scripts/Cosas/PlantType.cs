using UnityEngine;


public class PlantType : MonoBehaviour
{
    // Variables
    private int waterDemand;
    private SolarExposure solarExposureDemand;
    private bool isTall; // 0 = small, 1 = tall
    private int seedTime;
    private int sprotTime;
    // Sensibilidad..
    // Atraccion...
    // Da frutos...

    // Constructor
    public PlantType (int _waterDemand, int _solarExposureDemand, bool _isTall, int _seedTime, int sproutTime)
    {
        this.waterDemand = _waterDemand;
        this.solarExposureDemand = (SolarExposure) _solarExposureDemand;
        this.isTall = _isTall;
        this.seedTime = _seedTime;
        this.sprotTime= sproutTime;
    }



}
