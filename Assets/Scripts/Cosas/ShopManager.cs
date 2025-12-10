using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;              
using UnityEngine.Localization.Settings;
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance; 

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

    IEnumerator Start()
    {
        if (shopItemPrefab == null)
        {
            Debug.LogError("ShopManager: No has asignado el Prefab del item de la tienda");
            yield break;
        }

        yield return LocalizationSettings.InitializationOperation;

        yield return null;

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

        StartCoroutine(UpdateLayoutParams());
    }

    IEnumerator UpdateLayoutParams()
    {
        yield return new WaitForEndOfFrame();
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(contentGO.GetComponent<RectTransform>());
    }

    // Metodo para equipar el tipo de planta
    public void SelectPlantToBuy(PlantType plant) 
    {
        selectedPlantType = plant;
        GameManager.Instance.CurrentTool = ToolType.Plant;
    }
}
