using System;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance; //Singleton

    [NonSerialized] public PlantType selectedPlantType = null;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Metodo para equipar el tipo de planta
    public void SelectPlantToBuy(PlantType plant) 
    {
        selectedPlantType = plant;
        GameManager.Instance.CurrentTool = ToolType.Plant;
    }
}
