using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance; // Singleton

    [NonSerialized] public PlantType selectedPlantType = null;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private GameObject contentGO;

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

    void Start()
    {
        if (shopItemPrefab == null)
        {
            Debug.LogError("ShopManager: No has asignado el Prefab del item de la tienda");
            return;
        }

        PopulateShop();
    }

    void PopulateShop()
    {
        List<PlantType> plantsToSell = GameManager.Instance.plantsList;

        List<PlantType> sortedPlants = plantsToSell.OrderBy(plant => plant.price).ToList();

        foreach (PlantType plant in sortedPlants)
        {
            GameObject newItemGO = Instantiate(shopItemPrefab, contentGO.transform, false);

            ShopItemUI itemScript = newItemGO.GetComponent<ShopItemUI>();
            if (itemScript != null)
            {
                itemScript.Setup(plant);
            }
        }
    }

    // Metodo para equipar el tipo de planta
    public void SelectPlantToBuy(PlantType plant) 
    {
        selectedPlantType = plant;
        GameManager.Instance.CurrentTool = ToolType.Plant;
    }
}
