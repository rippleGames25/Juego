using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ShopItemUI : MonoBehaviour
{
    
    public PlantType plantData;
    public TextMeshProUGUI nameText;
    public Image plantImage;
    private Image itemBackground;
    public TextMeshProUGUI priceText;

    [SerializeField] private Button buyButton;

    void Start()
    {
        GameManager.Instance.OnMoneyChanged += UpdateItemAvailability;
        itemBackground = GetComponent<Image>();

        if (plantData != null)
        {
            nameText.text = plantData.plantName;
            priceText.text = plantData.price.ToString() + "€";
            plantImage.sprite = plantData.plantSprites[3];
        }
    }

    // Metodo para Actualizar la disponibilidad del Item en la tienda
    private void UpdateItemAvailability(int _currentmoney)
    {
        if(_currentmoney < plantData.price)
        {
            buyButton.interactable = false;
            itemBackground.color = new Color32(90, 90, 90, 100);

        } else
        {
            buyButton.interactable = true;
            itemBackground.color = new Color32(255, 255, 255, 100);
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
}
