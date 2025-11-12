using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceShopItem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button buyButton;

    [SerializeField] private int price = 1;
    [SerializeField] private string resource;

    void Start()
    {
        priceText.text = price.ToString();

        GameManager.Instance.OnMoneyChanged += UpdateItemAvailability;

        UpdateItemAvailability(GameManager.Instance.CurrentMoney);

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
        buyButton.interactable = (_currentmoney >= price);
    }

    public void BuyItem()
    {
        SFXManager.Instance?.PlayClick();
        
        if(resource == "agua")
        {
            ++GameManager.Instance.CurrentWater;
            --GameManager.Instance.CurrentMoney;
            Debug.Log($"Compra de {resource} realizada.");
        } else if (resource == "abono")
        {
            ++GameManager.Instance.CurrentFertilizer;
            --GameManager.Instance.CurrentMoney;
            Debug.Log($"Compra de {resource} realizada.");
        }
    }
}
