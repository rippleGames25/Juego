/*
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public bool IsLoading { get; private set; } = false;
    private int queuedSlot = -1;            // slot encolado para cargar
    private SaveGameData queuedData = null; // datos ya leídos del disco

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    string PathForSlot(int slot) => Path.Combine(Application.persistentDataPath, $"save_slot{slot}.json");
    string KeyForSlot(int slot) => $"save_slot{slot}";

    public bool Exists(int slot)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    return PlayerPrefs.HasKey(KeyForSlot(slot));
#else
        return File.Exists(PathForSlot(slot));
#endif
    }

    public SaveGameData LoadSlotData(int slot)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    if (!PlayerPrefs.HasKey(KeyForSlot(slot))) return null;
    string json = PlayerPrefs.GetString(KeyForSlot(slot), "");
    return string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<SaveGameData>(json);
#else
        var p = PathForSlot(slot);
        if (!File.Exists(p)) return null;
        return JsonUtility.FromJson<SaveGameData>(File.ReadAllText(p));
#endif
    }

    public void Save(int slot)
    {
        var data = BuildSave();
        data.savedAt = System.DateTime.Now.ToString("s");
        var json = JsonUtility.ToJson(data, true);

#if UNITY_WEBGL && !UNITY_EDITOR
    PlayerPrefs.SetString(KeyForSlot(slot), json);
    PlayerPrefs.Save(); // <- imprescindible en WebGL
#else
        File.WriteAllText(PathForSlot(slot), json);
#endif
        Debug.Log($"[Save] Guardado slot {slot}");
    }

    public void Delete(int slot)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
    PlayerPrefs.DeleteKey(KeyForSlot(slot));
    PlayerPrefs.Save();
#else
        var p = PathForSlot(slot);
        if (File.Exists(p)) File.Delete(p);
#endif
    }

    // Carga
    // Llamada desde SaveSlotsUI tras cargar GameScene
    public void BeginLoad(int slot)
    {
        queuedData = LoadSlotData(slot);
        if (queuedData == null) { Debug.LogWarning("[Save] slot vacío o corrupto."); return; }
        queuedSlot = slot;
        IsLoading = true;                       // para que GameManager.Start() no cree parcelas
        StartCoroutine(ApplyQueuedWhenReady()); // aplicamos cuando todo esté listo
    }

    private IEnumerator ApplyQueuedWhenReady()
    {
        if (queuedData == null) { IsLoading = false; yield break; }

        // Espera a que existan managers de la escena
        while (GameManager.Instance == null || PlotsManager.Instance == null || WeatherManager.Instance == null)
            yield return null;

        yield return null;

        ApplySave(queuedData);

        // limpieza
        queuedData = null;
        queuedSlot = -1;
        IsLoading = false;
    }

    // Build desde el estado actual
    private SaveGameData BuildSave()
    {
        var gm = GameManager.Instance;
        var pm = PlotsManager.Instance;
        var wm = WeatherManager.Instance;

        var d = new SaveGameData
        {
            day = gm.CurrentDay,
            biodiversity = gm.CurrentBiodiversity,
            money = gm.CurrentMoney,
            water = gm.CurrentWater,
            fertilizer = gm.CurrentFertilizer,
            currentTool = gm.CurrentTool,
            winCondition = gm.winCondition,
            currentWeather = new DailyWeatherData
            {
                type = wm.GetCurrentWeather().type.ToString(),
                waterChange = wm.GetCurrentWeather().waterChange
            }
        };

        foreach (var fw in wm.GetForecastList())
            d.forecast.Add(new DailyWeatherData { type = fw.type.ToString(), waterChange = fw.waterChange });

        foreach (var plot in pm.GetAllPlots())
        {
            if (plot == null) continue;
            var p = new PlotData
            {
                x = plot.gridCoordinates.x,
                y = plot.gridCoordinates.y,
                currentWater = plot.currentWater,
                currentFertility = plot.currentFertility,
                solarExposure = (int)plot.GetCurrentSolarExposure(),
                isPlanted = plot.isPlanted
            };

            if (plot.isPlanted && plot.currentPlant != null)
            {
                var pl = plot.currentPlant;
                p.plant = new PlantSaveData
                {
                    plantTypeIdx = pl.plantData.idx,
                    category = pl.plantData.category.ToString(),
                    growth = pl.GetGrowth().ToString(),
                    health = pl.GetHealth().ToString(),
                    lifeDays = pl.lifeDays,
                    hasProduct = pl.hasProduct
                };
            }

            d.plots.Add(p);
        }

        return d;
    }

    // Reconstruir escena
    private void ApplySave(SaveGameData d)
    {
        var gm = GameManager.Instance;
        var pm = PlotsManager.Instance;
        var wm = WeatherManager.Instance;

        // 1) Preparar escena
        SFXManager.Instance?.StopAmbient();

        pm.ClearAllPlots();     // limpia lo anterior 
        pm.CreatePlots();       // crea la malla base

        // 2) Estado global
        gm.CurrentDay = d.day;
        gm.CurrentBiodiversity = d.biodiversity;
        gm.CurrentMoney = d.money;
        gm.CurrentWater = d.water;
        gm.CurrentFertilizer = d.fertilizer;
        gm.CurrentTool = d.currentTool;
        gm.winCondition = d.winCondition;

        // 3) Clima
        wm.SetCurrentWeather(ParseWeather(d.currentWeather));
        var list = new List<DailyWeather>();
        foreach (var f in d.forecast) list.Add(ParseWeather(f));
        wm.SetForecast(list);

        // 4) Parcela por parcela
        foreach (var pd in d.plots)
        {
            var plot = pm.GetPlotAt(pd.x, pd.y);
            if (plot == null) continue;

            plot.currentWater = pd.currentWater;
            plot.currentFertility = pd.currentFertility;
            plot.ChangeSolarExposure(pd.solarExposure);
            plot.ChangePlantState(pd.isPlanted);

            if (pd.isPlanted && pd.plant != null)
            {
                var pt = gm.GetPlantTypeByIdx(pd.plant.plantTypeIdx);
                var go = Object.Instantiate(gm.GetPlantPrefab(),
                    plot.transform.position + new Vector3(0, 0.1f, -0.1f),
                    Quaternion.identity, plot.transform);

                Plant newPlant = pt.category switch
                {
                    PlantCategory.Producer => go.AddComponent<ProducerPlant>(),
                    PlantCategory.ProvidesShade => go.AddComponent<ShaderProviderPlant>(),
                    PlantCategory.PollinatorAttractor => go.AddComponent<PollinatorAttractorPlant>(),
                    PlantCategory.WildlifeRefuge => go.AddComponent<WildlifeRefugePlant>(),
                    _ => go.AddComponent<Plant>()
                };

                newPlant.InitializePlant(pt, plot);
                newPlant.lifeDays = pd.plant.lifeDays;
                newPlant.hasProduct = pd.plant.hasProduct;
                newPlant.SetGrowth(ParseGrowth(pd.plant.growth));
                newPlant.SetHealth(ParseHealth(pd.plant.health));

                plot.currentPlant = newPlant;
                plot.isPlanted = true;
            }

            plot.UpdatePlotWaterVisuals();
            plot.UpdatePlotFertilizerVisuals();
            plot.UpdatePlotSolarExposureVisuals();
        }

        // 5) Reaplicar efectos de entorno y notificar UI/clima
        pm.RebuildEnvironmentFromPlants();
        wm.NotifyAll();

        // notificar UI tras restaurar todo
        gm.ForceRefreshUI();

        Debug.Log("[Save] Snapshot aplicado.");

    }

    private DailyWeather ParseWeather(DailyWeatherData x)
    {
        var w = new DailyWeather();
        w.type = (WeatherType)System.Enum.Parse(typeof(WeatherType), x.type);
        w.waterChange = x.waterChange;
        w.intensity = 1;          
        w.deathProbability = 0f; 
        return w;
    }

    private GrowthState ParseGrowth(string s) => (GrowthState)System.Enum.Parse(typeof(GrowthState), s);
    private Health ParseHealth(string s) => (Health)System.Enum.Parse(typeof(Health), s);

}
*/