using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ShopItemUI : MonoBehaviour
{
    public GameObject UIManager;
    
    public PlantType plantData;
    public TextMeshProUGUI nameText;
    public Image plantImage;
    public TextMeshProUGUI priceText;

    [SerializeField] private Button buyButton;

    void Start()
    {
        GameManager.Instance.OnMoneyChanged += UpdateItemAvailability;

        if (plantData != null)
        {
            nameText.text = plantData.plantName;
            priceText.text = plantData.price.ToString();
            plantImage.sprite = plantData.plantSprites[3];
        }
    }

    // Metodo para Actualizar la disponibilidad del Item en la tienda
    private void UpdateItemAvailability(int _currentmoney)
    {
        if(_currentmoney < plantData.price)
        {
            buyButton.interactable = false;

        } else
        {
            buyButton.interactable = true;
        }
    }

    // Metodo paar seleccionar un tipo de planta en la tienda
    public void SelectItem()
    {
        if(plantData != null)
        {
            Debug.Log($"Planta seleccionada {plantData.plantName}");
            ShopManager.Instance.SelectPlantToBuy(plantData);   
        }
    }

    public void ShowInfoPanel()
    {
        GameManager.Instance.ShowPlantTypePanel(plantData);
    }
}
