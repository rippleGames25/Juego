using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoftwareCursorManager : MonoBehaviour
{
    public static SoftwareCursorManager Instance;

    [Header("Componente de Imagen")]
    [SerializeField] private Image cursorImage; 

    [Header("Sprites de Herramienta")]
    [SerializeField] private Sprite normalCursor;
    [SerializeField] private Sprite wateringCanCursor;
    [SerializeField] private Sprite fertilizerBagCursor;
    [SerializeField] private Sprite seedCursor;
    [SerializeField] private Sprite shovelCursor;

    void Awake()
    {
        // --- Configuración del Singleton ---
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject); 
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        // --- Configuración Inicial ---
        if (cursorImage == null)
        {
            cursorImage = GetComponent<Image>();
        }

        if (cursorImage != null)
            cursorImage.raycastTarget = false;

        Cursor.visible = false;
        cursorImage.sprite = normalCursor;

        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void Update()
    {
        cursorImage.transform.position = Input.mousePosition;
    }

    public void SetTool(ToolType tool)
    {
        switch (tool)
        {
            case ToolType.WateringCan:
                cursorImage.sprite = wateringCanCursor;
                break;
            case ToolType.FertilizerBag:
                cursorImage.sprite = fertilizerBagCursor;
                break;
            case ToolType.Plant:
                cursorImage.sprite = seedCursor;
                break;
            case ToolType.Shovel:
                cursorImage.sprite = shovelCursor;
                break;
            case ToolType.None:
            default:
                cursorImage.sprite = normalCursor;
                break;
        }
    }
    private void OnSceneChanged(Scene current, Scene next)
    {
        if (next.name != "GameScene")
        {
            SetTool(ToolType.None);
        }
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }
}