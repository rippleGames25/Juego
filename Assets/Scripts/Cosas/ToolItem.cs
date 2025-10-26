using System.Collections.Generic;
using UnityEngine;

public class ToolItem : MonoBehaviour
{
    [SerializeField] public ToolType type;
    [SerializeField] private List<Sprite> itemSprites;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (GameManager.Instance!= null)
        {
            GameManager.Instance.OnToolChanged += UpdateTool;
        }
    }

    private void UpdateTool(ToolType _type)
    {
        if (this.type == _type) { spriteRenderer.sprite = itemSprites[1]; } // Activar seleccion
        else { spriteRenderer.sprite = itemSprites[0]; }
    }
}
