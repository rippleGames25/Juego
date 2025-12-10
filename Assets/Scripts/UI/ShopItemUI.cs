using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ShopItemUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private Image plantImage;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image solarExposure;
    [SerializeField] private List<Sprite> solarSprites = new List<Sprite>();


    [SerializeField] private Button buyButton;

    private PlantType plantData;

    public void Setup(PlantType dataToSetup)
    {
        plantData = dataToSetup;
        if (plantData == null) return;

        nameText.text = "...";

        var nameOperation = plantData.plantName.GetLocalizedStringAsync();

        if (nameOperation.IsDone)
        {
            nameText.text = nameOperation.Result;
        }
        else
        {
            nameOperation.Completed += (AsyncOperationHandle<string> obj) =>
            {
                if (this != null && obj.Status == AsyncOperationStatus.Succeeded)
                {
                    nameText.text = obj.Result;
                }
            };
        }

        try
        {
            typeText.text = plantData.TypeToString();
        }
        catch
        {
            typeText.text = "Type Error";
        }

        priceText.text = plantData.price.ToString();

        if (plantData.solarExposureDemand >= 0 && (int)plantData.solarExposureDemand < solarSprites.Count)
        {
            solarExposure.sprite = solarSprites[(int)plantData.solarExposureDemand];
        }

        if (plantData.shopSprite != null)
        {
            plantImage.sprite = plantData.shopSprite;
        }

        // Lógica del botón
        GameManager.Instance.OnMoneyChanged -= UpdateItemAvailability; // Prevenir duplicados
        GameManager.Instance.OnMoneyChanged += UpdateItemAvailability;
        UpdateItemAvailability(GameManager.Instance.CurrentMoney);

        buyButton.onClick.RemoveAllListeners();
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
        if (TutorialManager.Instance != null && TutorialManager.Instance.isDialogActive)
        {
            return;
        }

        if (GameManager.Instance.CurrentTool != ToolType.None)
        {
            GameManager.Instance.CurrentTool = ToolType.None;
            SFXManager.Instance?.PlayClick();
        }

        if (plantData != null)
        {
            SFXManager.Instance?.PlayClick();
            Debug.Log($"Planta seleccionada {plantData.plantName}");
            ShopManager.Instance.SelectPlantToBuy(plantData);
        }
    }


    public void ShowInfoPanel()
    {
        if (TutorialManager.Instance != null && TutorialManager.Instance.isDialogActive)
        {
            return;
        }

        if (GameManager.Instance.CurrentTool != ToolType.None)
        {
            GameManager.Instance.CurrentTool = ToolType.None;
            SFXManager.Instance?.PlayClick();
        }

        if (plantData != null)
        {
            SFXManager.Instance?.PlayInfoPlanta();
            GameManager.Instance.ShowPlantTypePanel(plantData);
        }
    }

}