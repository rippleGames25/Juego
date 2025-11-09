using UnityEngine;
using System.Collections.Generic;
using System.Linq; // ¡Muy importante para ordenar!

public class ShopPopulator : MonoBehaviour
{
    [SerializeField]
    private GameObject shopItemPrefab;

    void Start()
    {
        if (shopItemPrefab == null)
        {
            Debug.LogError("¡No has asignado el Prefab del item de la tienda en el ShopPopulator!");
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
            GameObject newItemGO = Instantiate(shopItemPrefab, transform);

            ShopItemUI itemScript = newItemGO.GetComponent<ShopItemUI>();
            if (itemScript != null)
            {
                itemScript.Setup(plant);
            }
        }
    }
}