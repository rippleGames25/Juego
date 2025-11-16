/*
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SaveSlotsUI : MonoBehaviour
{
    [System.Serializable]
    class SlotWidgets
    {
        public Button mainButton;
        public TMP_Text title;
        public TMP_Text subtitle;
        public Button deleteButton;
    }

    [SerializeField] private SlotWidgets[] slots = new SlotWidgets[3];

    void OnEnable()
    {
        // Bootstrap del SaveManager si aún no existe
        if (SaveManager.Instance == null)
        {
            var existing = FindObjectOfType<SaveManager>();
            if (existing == null)
            {
                var go = new GameObject("~SaveManager");
                go.AddComponent<SaveManager>(); 
            }
        }

        Refresh();
    }

    public void Refresh()
    {
        // Si algo sigue sin estar listo, evitar NullRef
        if (SaveManager.Instance == null) return;
        if (slots == null || slots.Length < 3) return;

        for (int i = 0; i < 3; i++)
        {
            int slot = i + 1;

            var sw = slots[i];
            if (sw == null) continue; // por si algún elemento no está configurado

            bool exists = SaveManager.Instance.Exists(slot);
            var data = exists ? SaveManager.Instance.LoadSlotData(slot) : null;

            if (sw.title) sw.title.text = exists ? $"Partida {slot}" : "Nueva partida";
            if (sw.subtitle) sw.subtitle.text = exists && data != null ? $"Día {data.day} · {PrettyDate(data.savedAt)}" : "";

            if (sw.deleteButton) sw.deleteButton.gameObject.SetActive(exists);

            if (sw.mainButton)
            {
                sw.mainButton.onClick.RemoveAllListeners();
                sw.mainButton.onClick.AddListener(() => OnSlotPressed(slot, exists));
            }

            if (sw.deleteButton)
            {
                sw.deleteButton.onClick.RemoveAllListeners();
                sw.deleteButton.onClick.AddListener(() => OnDeletePressed(slot));
            }
        }
    }

    void OnSlotPressed(int slot, bool exists)
    {
        SFXManager.Instance?.PlayClick();
        PlayerPrefs.SetInt("lastSlot", slot);

        if (exists)
        {
            // Cargar partida al entrar a GameScene
            SceneManager.sceneLoaded += OnGameLoadedThenApply;
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            // Nueva partida, entrar limpio
            SceneManager.LoadScene("GameScene");
        }
    }

    void OnDeletePressed(int slot)
    {
        SFXManager.Instance?.PlayClick();
        SaveManager.Instance.Delete(slot);
        Refresh();
    }

    void OnGameLoadedThenApply(Scene sc, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnGameLoadedThenApply;
        var slot = PlayerPrefs.GetInt("lastSlot", 1);
        if (SaveManager.Instance.Exists(slot))
            SaveManager.Instance.BeginLoad(slot);
    }


    string PrettyDate(string iso)
    {
        if (System.DateTime.TryParse(iso, out var dt))
            return dt.ToString("HH:mm dd/MM");
        return "";
    }
}
*/