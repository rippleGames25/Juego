using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance; //Singleton

    public PlantType selectedPlantType = null;
    public bool isPlantingMode = false;

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

    public void SelectPlantToBuy(PlantType plant) 
    {
        if (GameManager.Instance.CurrentMoney < plant.price) return;

        selectedPlantType = plant;
        isPlantingMode= true;
        GameManager.Instance.CurrentTool = ToolType.Plant;
    }


}
