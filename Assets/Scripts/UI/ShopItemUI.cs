using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ShopItemUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private Image plantImage;
    [SerializeField] private TextMeshProUGUI priceText;

    [SerializeField] private Button buyButton;

    private PlantType plantData;

    public void Setup(PlantType dataToSetup)
    {
        plantData = dataToSetup;

        if (plantData == null) return;

        nameText.text = plantData.plantName;
        typeText.text = plantData.category.ToString();
        priceText.text = plantData.price.ToString();

        if (plantData.plantSprites != null && plantData.plantSprites.Length > 0)
        {
            int spriteIndex = Mathf.Min(GameManager.IDX_PLANT_SPRITE, plantData.plantSprites.Length - 1);
            plantImage.sprite = plantData.plantSprites[spriteIndex];
        }

        GameManager.Instance.OnMoneyChanged += UpdateItemAvailability;

        UpdateItemAvailability(GameManager.Instance.CurrentMoney);

        buyButton.onClick.AddListener(SelectItem);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged -= UpdateItemAvailability;
        }
    }

    private void UpdateItemAvailability(int _currentmoney)
    {
        if (plantData == null) return;

        buyButton.interactable = (_currentmoney >= plantData.price);
    }

    public void SelectItem()
    {
        if (plantData != null)
        {
            SFXManager.Instance?.PlayClick();
            Debug.Log($"Planta seleccionada {plantData.plantName}");
            ShopManager.Instance.SelectPlantToBuy(plantData);
        }
    }

    public void ShowInfoPanel()
    {
        if (plantData != null)
        {
            SFXManager.Instance?.PlayInfoPlanta();
            GameManager.Instance.ShowPlantTypePanel(plantData);
        }
    }
}