using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public event Action<Plot> OnPlotClicked;
    public event Action<ToolItem> OnToolClicked;
    public event Action OnBackgroundClicked;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ProcessClick();
        }
    }

    private void ProcessClick()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent<ToolItem>(out ToolItem tool))
            {
                OnToolClicked?.Invoke(tool);
                return;
            }

            if (hit.collider.TryGetComponent<Plot>(out Plot plot))
            {
                OnPlotClicked?.Invoke(plot);
                return;
            }
        }
        else
        {
            OnBackgroundClicked?.Invoke();
        }

    }

}
