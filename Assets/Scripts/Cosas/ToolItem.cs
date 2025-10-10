using UnityEngine;

public class ToolItem : MonoBehaviour
{
    [SerializeField] private ToolType type;
    [SerializeField] private GameObject selectionOn;

    void Start()
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
        if (this.type == _type) { selectionOn.SetActive(true); } // Activar fondo de seleccion
        else { selectionOn.SetActive(false); }
    }


}
