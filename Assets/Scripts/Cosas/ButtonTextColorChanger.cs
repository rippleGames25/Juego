using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; 

public class ButtonTextColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public TextMeshProUGUI buttonText;

    // Colores
    public Color normalColor = new Color32(255, 255, 207, 255);
    public Color highlightedColor = new Color32(255, 255, 207, 255);
    public Color pressedColor = new Color32(86, 77, 69, 255);

    void Start()
    {
        if (buttonText != null)
        {
            buttonText.color = normalColor;
        }
    }

    // Highlighted
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.color = highlightedColor;
    }

    // Normal
    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.color = normalColor;
    }

    // Pressed
    public void OnPointerDown(PointerEventData eventData)
    {
        buttonText.color = pressedColor;
    }

    // Normal o Highlighted
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject == gameObject)
        {
            buttonText.color = highlightedColor;
        }
        else
        {
            buttonText.color = normalColor;
        }
    }
}