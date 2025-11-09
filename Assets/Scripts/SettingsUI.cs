using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private GameObject hudPanel;        // vuestro HUD
    [SerializeField] private GameObject settingsPanel;   // vuestro SettingsPanel

    public void Open()
    {
        hudPanel.SetActive(false);
        settingsPanel.SetActive(true);
        GameManager.Instance.SetInputLocked(true);
    }

    public void Close()
    {
        settingsPanel.SetActive(false);
        hudPanel.SetActive(true);
        GameManager.Instance.SetInputLocked(false);
    }
}
