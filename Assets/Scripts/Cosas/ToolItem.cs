using UnityEngine;

public class ToolItem : MonoBehaviour
{
    [SerializeField] public ToolType type;
    [SerializeField] private GameObject selectionOn;

    void Start()
    {
        if(GameManager.Instance!= null)
        {
            GameManager.Instance.OnToolChanged += UpdateTool;
        }
    }

    private void UpdateTool(ToolType _type)
    {
        if (this.type == _type) { selectionOn.SetActive(true); } // Activar fondo de seleccion
        else { selectionOn.SetActive(false); }
    }
}
