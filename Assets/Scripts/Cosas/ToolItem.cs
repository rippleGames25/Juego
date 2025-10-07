using UnityEngine;

public class ToolItem : MonoBehaviour
{
    [SerializeField] private ToolType type;

    public void Start()
    {
        if(GameManager.Instance!= null)
        {
            GameManager.Instance.OnToolChanged += UpdateTool;
        }
    }

    public void OnMouseDown()
    {
        if(GameManager.Instance != null)
        {
            if(GameManager.Instance.CurrentTool == type)
            {
                GameManager.Instance.CurrentTool = ToolType.None; // Si ya la tiene, la desactiva
            }
            else
            {
                GameManager.Instance.CurrentTool = type; // Si no la tiene la equipa
            }
        }
    }

    private void UpdateTool(ToolType _type)
    {
        if (this.type == _type) { this.GetComponent<SpriteRenderer>().enabled = false; } // Desactivar Imagen
        else { this.GetComponent<SpriteRenderer>().enabled = true; }
    }


}
