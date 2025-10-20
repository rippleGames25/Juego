using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum SolarExposure
{
    Soleado,
    Semisombra,
    Sombra
}

// This class represent a Plot in the game
public class Plot : MonoBehaviour
{
    [Header("Variables")]
    public Vector2Int gridCoordinates;
    public Plant currentPlant;
    

    [Header("Estado parcela")]
    [Range(0, PLOT_LIMIT)] [SerializeField] public int currentWater;
    [Range(0, PLOT_LIMIT)] [SerializeField] public int currentFertility;
    [SerializeField] private SolarExposure currentSolarExposure;
    [SerializeField] public bool isPlanted;

    private SpriteRenderer sr; // SpriteRenderer ded la Parcela
    private const int PLOT_LIMIT = 10; // Limite de agua y abono
    private RectTransform rectTransform;
    private Vector3 initialPosition;

    [Header("Visual")]
    public GameObject selectionBorder;
    [SerializeField] private GameObject changeCanvas;
    [SerializeField] private TextMeshProUGUI changeText;
    [SerializeField] private Image changeImage;
    [SerializeField] private Sprite waterIcon;
    [SerializeField] private Sprite fertilizerIcon;


    // Constructor
    public Plot(int _currentWater, int _currentFertility, int _currentSolarExposure, bool isPlanted)
    {
        this.currentWater = _currentWater;
        this.currentFertility = _currentFertility;
        this.currentSolarExposure = (SolarExposure)_currentSolarExposure;
        this.isPlanted = isPlanted;
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        selectionBorder = this.transform.GetChild(0).gameObject; // Obtenemos el contorno de seleccion
        changeCanvas.SetActive(false); // Ocultamos el canvas del texto

        rectTransform = changeCanvas.GetComponent<RectTransform>();
        initialPosition = rectTransform.localPosition;

        UpdatePlotWaterVisuals();
        UpdatePlotFertilizerVisuals();
    }

    #region Public methods
    public void InitializePlot(int x, int y)
    {
        gridCoordinates= new Vector2Int(x, y);
        currentFertility = UnityEngine.Random.Range(5, 8);
        currentWater = UnityEngine.Random.Range(5, 8);
        currentSolarExposure = SolarExposure.Soleado; // Temporalmente todas sol
        isPlanted = false; // Se inicializa vacia
        currentPlant = null; // No hay planta

        Debug.Log(this.ToString());
    }

    public void UpdatePlant()
    {
        // Calcular muertes

        currentPlant.UpdateLifeDays(); // Actualizar dias de vida

        if (currentPlant.currentGrowth == GrowthState.madura)
        {
            UpdateEnviroment(currentPlant.plantData.category);
        }
    }

    public void ChangeWater (int weatherWater)
    {
        int waterChange = weatherWater;

        if (this.isPlanted)
        {
            waterChange = waterChange - currentPlant.plantData.waterDemand; // Restamos agua consumida por la planta
        }

        this.currentWater  += waterChange; // Agua aportada por evento meteorologico
        this.currentWater = Mathf.Clamp(this.currentWater, 0, PLOT_LIMIT); // Mantener el limite de la parcela
    }

    public void ChangeFertility(int fertilityChange)
    {
        this.currentFertility += fertilityChange;
        this.currentFertility = Mathf.Clamp(this.currentFertility, 0, PLOT_LIMIT); // Mantener el limite de la parcela
    }

    public void ChangeSolarExposure(int newSolarExposure)
    {
        this.currentSolarExposure = (SolarExposure) newSolarExposure;
    }

    public void ChangePlantState(bool newIsPlanted)
    {
        this.isPlanted = newIsPlanted;
    }

    // Metodo que define que accion se realiza sobre la parcela según la herramienta equipada
    public void SelectPlot()
    {
        switch (GameManager.Instance.CurrentTool)
        {
            case ToolType.None:
                
                PlotsManager.Instance.PlotSelected(this);
                Debug.Log($"Parcela {this.gridCoordinates} seleccionada.");
                break;

            case ToolType.WateringCan:
               
                if (GameManager.Instance.CurrentWater > 0 && currentWater < PLOT_LIMIT) // Tiene agua y la parcela no esta llena
                {
                    GameManager.Instance.CurrentWater--; // Cosume agua del deposito
                    this.currentWater++;

                    TextAnimation("+1", 0, Color.green); 
                    this.UpdatePlotWaterVisuals();

                    Debug.Log($"Parcela {this.gridCoordinates} regada -> {currentWater} de agua");
                }
                else if (GameManager.Instance.CurrentWater > 0 && currentWater >= PLOT_LIMIT) // Tiene agua pero la parcela está llena
                {
                    TextAnimation("Lleno", 0, Color.green);
                    Debug.Log($"No se puede regar, la parcela {gridCoordinates} está al máximo de agua.");
                }
                else // No queda agua
                {
                    Debug.Log("No te queda agua.");
                }
                break;

            case ToolType.FertilizerBag:

                if (GameManager.Instance.CurrentFertilizer > 0 && currentFertility < PLOT_LIMIT) // Tiene abono y la parcela no esta llena
                {
                    GameManager.Instance.CurrentFertilizer--; // Consume abono del deposito
                    this.currentFertility++;

                    TextAnimation("+1", 1, Color.green);
                    this.UpdatePlotFertilizerVisuals();


                    Debug.Log($"Parcela {this.gridCoordinates} abonada -> {currentFertility} de abono");

                }
                else if (GameManager.Instance.CurrentFertilizer > 0 && currentFertility >= PLOT_LIMIT) // Tiene abono pero la parcela está llena
                {
                    TextAnimation("Lleno", 1, Color.green);
                    Debug.Log($"No se puede abonar, la parcela {gridCoordinates} está al máximo de abono.");
                }
                else // No queda abono
                {
                    Debug.Log("No te queda abono");
                }
                break;

            case ToolType.Plant:

                if (!this.isPlanted)
                {
                    int plantType = ShopManager.Instance.selectedPlantType.idx;
                    GameManager.Instance.PlantSeed(this, plantType);
                }
                else
                {
                    Debug.Log($"Parcela {gridCoordinates} ocupada.");
                }
                break;
            case ToolType.Shovel:
                if (this.isPlanted)
                {
                    GameManager.Instance.CurrentMoney+=this.currentPlant.plantData.price; // Dinero que gana

                    PlotsManager.Instance.PlantsDeath(this.currentPlant);
                    this.currentPlant = null;
                    this.isPlanted = false;                    
                }
                else
                {
                    Debug.Log($"En la parcela {gridCoordinates} no hay ninguna planta.");
                }
                break;
        }
    }

    public void UpdateEnviroment(PlantCategory plantType)
    {
        switch (plantType)
        {
            case PlantCategory.PollinatorAttractor:
                // Atraer polinizadores

                break;
            case PlantCategory.ProvidesShade:
                // Cambiar la exposicion solar de las parcelas adyacentes
                PlotsManager.Instance.GenerateShade(this.gridCoordinates);
                break;
            case PlantCategory.Producer:
                // Empezar el ciclo de produccion

                break;
            case PlantCategory.WildlifeRefuge:
                // Instanciar fauna

                break;
        }
    }

    private void TextAnimation(string _text, int type, Color textColor)
    {
        Debug.Log("Plot: Empezando Animacion");
        // Icono
        if (type == 0)
        {
            changeImage.sprite = waterIcon;
        }
        else if (type == 1)
        {
            changeImage.sprite = fertilizerIcon;
        }
        else
        {
            Debug.Log("Plot: Tipo de icono incorrecto");
            return;
        }

        changeText.text = _text;
        changeText.color = textColor;

        changeCanvas.SetActive(true);

        StartCoroutine(AnimateTextChange());
    }

    private IEnumerator AnimateTextChange()
    {
        float duration = 0.5f; // Duracion en segundos
        float distance = 0.4f;

        rectTransform.localPosition = initialPosition; // Posicion inicial

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Mover hacia arriba
            Vector3 targetPosition = initialPosition + Vector3.up * distance;
            rectTransform.localPosition = Vector3.Lerp(initialPosition, targetPosition, t);

            elapsed += Time.deltaTime;
            yield return null;
        }
        Debug.Log("Plot: Aniamcion Hecha");
        rectTransform.localPosition = initialPosition;
        changeCanvas.SetActive(false);
    }


    #endregion

    #region Métodos para visualización
    public void UpdatePlotWaterVisuals()
    {
        Color newColor = sr.color;
        newColor.r = 1f - Mathf.Clamp01((float)this.currentWater / (PLOT_LIMIT * 2));

        sr.color = newColor;
    }

    public void UpdatePlotFertilizerVisuals()
    {
        Color newColor = sr.color;
        newColor.g = 1f - Mathf.Clamp01((float)this.currentFertility / (PLOT_LIMIT * 2));

        sr.color = newColor;
    }

    public override string ToString()
    {
        return $"Parcela {gridCoordinates} -> Agua: {currentWater}, Fertilidad: {currentFertility}, Exposicion Solar: {currentSolarExposure}";
    }

    #endregion
}
