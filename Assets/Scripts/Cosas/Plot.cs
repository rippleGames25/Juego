using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum SolarExposure
{
    Soleado,
    Semisombra,
    Sombra
}

public class Plot : MonoBehaviour
{
    [Header("Variables")]
    public Vector2Int gridCoordinates;
    public Plant currentPlant;
    

    [Header("Estado parcela")]
    [Range(0, PLOT_LIMIT)] [SerializeField] public int currentWater;
    [Range(0, PLOT_LIMIT)] [SerializeField] public int currentFertility;
    [SerializeField] public SolarExposure currentSolarExposure;
    [SerializeField] public bool isPlanted;

    private SpriteRenderer sr; // SpriteRenderer de la Parcela
    private const int PLOT_LIMIT = 5; // Limite de agua y abono

    [Header("Gestion de la sombra")]

    [SerializeField] private int shadeSourceCount = 0; // Contador de plantas que dan sombra a esta parcela

    [Header("Gestion de Fauna")]
    [SerializeField] private int pollinatorSourceCount = 0;
    [SerializeField] private int refugeSourceCount = 0;
    [SerializeField] private Vector3 currentPollinatorPosition = new Vector3(0, 0.65f, 0);

    public bool IsPollinated => pollinatorSourceCount > 0;
    public bool IsProtected => refugeSourceCount > 0;

    [Header("Visual")]
    [SerializeField] private List<Sprite> solarExposurePlot = new List<Sprite> ();
    [SerializeField] public GameObject selectionBorder;
    [SerializeField] private GameObject changeCanvasPrefab;
    [SerializeField] private Sprite waterIcon;
    [SerializeField] private Sprite fertilizerIcon;

    [Header("Visual Effects (Prefabs)")]
    [SerializeField] private GameObject pollinatorVisualPrefab;
    private GameObject currentPollinatorVisual;

    // Evento para actualizar información de parcela estando abierta
    public event Action<Plot> OnPlotDataUpdated;

    // Colores
    private Color colorFullWaterFullFertility;
    private Color colorNoWaterNoFertility;
    private Color colorNoWaterFullFertility;
    private Color colorFullWaterNoFertility;


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

        ColorUtility.TryParseHtmlString("#352D1B", out colorFullWaterFullFertility);
        ColorUtility.TryParseHtmlString("#7B6F39", out colorNoWaterNoFertility);
        ColorUtility.TryParseHtmlString("#5A4E1E", out colorNoWaterFullFertility);
        ColorUtility.TryParseHtmlString("#7B5239", out colorFullWaterNoFertility);
    }

    void Start()
    {
        selectionBorder = this.transform.GetChild(0).gameObject; // Obtenemos el contorno de seleccion
      
        CalculateColor(); // Calculo de diferencia de color

        UpdatePlotWaterVisuals();
        UpdatePlotFertilizerVisuals();
        UpdatePlotSolarExposureVisuals();
    }

    #region Public methods
    public void InitializePlot(int x, int y)
    {
        gridCoordinates= new Vector2Int(x, y);
        currentFertility = UnityEngine.Random.Range(0, PLOT_LIMIT);
        currentWater = UnityEngine.Random.Range(0, PLOT_LIMIT);
        currentSolarExposure = (SolarExposure)UnityEngine.Random.Range(0, 1); //Exposicion solar random
        isPlanted = false;      // Se inicializa vacia
        currentPlant = null;    // No hay planta
    }


    public SolarExposure GetCurrentSolarExposure()
    {
        return currentSolarExposure;
    }

    public void ChangeWater (int weatherWater)
    {
        int waterChange = weatherWater;

        if (this.isPlanted)
        {
            waterChange = waterChange - currentPlant.plantData.waterDemand; // Restamos agua consumida por la planta
        }

        this.currentWater  += waterChange;                                  // Agua aportada por evento meteorologico
        this.currentWater = Mathf.Clamp(this.currentWater, 0, PLOT_LIMIT);  // Mantener el limite de la parcela
    }

    public void ChangeSolarExposure(int newSolarExposure)
    {
        this.currentSolarExposure = (SolarExposure) newSolarExposure;
    }

    public void ChangePlantState(bool newIsPlanted)
    {
        this.isPlanted = newIsPlanted;
    }

    #region Plant Effects
    public void AddShadeSource()
    {
        shadeSourceCount++;

        if(shadeSourceCount == 1)
        {
            currentSolarExposure = SolarExposure.Semisombra;

        } else if(shadeSourceCount >= 2)
        {
            currentSolarExposure = SolarExposure.Sombra;
        }

        UpdatePlotSolarExposureVisuals();
    }

    public void RemoveShadeSource()
    {
        if(shadeSourceCount > 0)
        {
            shadeSourceCount--;
        }

        if (shadeSourceCount == 0)
        {
            this.currentSolarExposure= SolarExposure.Soleado;
        } 
        else if(shadeSourceCount == 1)
        {
            this.currentSolarExposure = SolarExposure.Semisombra;
        }
        else // shadeSourceCount >= 2
        {
            this.currentSolarExposure = SolarExposure.Sombra;
        }

        UpdatePlotSolarExposureVisuals();
    }

    public void AddPollinatorSource()
    {
        pollinatorSourceCount++;
        UpdatePollinatorVisual();
        GameManager.Instance?.UpdateBiodiversityScore();
    }

    public void RemovePollinatorSource()
    {
        if (pollinatorSourceCount > 0)
        {
            pollinatorSourceCount--;
        }
        UpdatePollinatorVisual();
        GameManager.Instance?.UpdateBiodiversityScore();
    }

    public void AddRefugeSource()
    {
        refugeSourceCount++;
        GameManager.Instance?.UpdateBiodiversityScore();
    }

    public void RemoveRefugeSource()
    {
        if (refugeSourceCount > 0)
        {
            refugeSourceCount--;
        }
        GameManager.Instance?.UpdateBiodiversityScore();
    }

    #endregion

    // Metodo que define que accion se realiza sobre la parcela según la herramienta equipada
    public void SelectPlot()
    {
        switch (GameManager.Instance.CurrentTool)
        {
            case ToolType.None:
                if (currentPlant!=null && currentPlant.hasProduct && currentPlant is ProducerPlant producerPlant)
                {
                    producerPlant.CollectProduct();
                    SFXManager.Instance?.PlayComprar();
                }
                else
                {
                    PlotsManager.Instance.PlotSelected(this);
                    SFXManager.Instance?.PlayClick();
                    Debug.Log($"Parcela {this.gridCoordinates} seleccionada.");
                }
                break;

            case ToolType.WateringCan:
               
                if (GameManager.Instance.CurrentWater > 0 && currentWater < PLOT_LIMIT) // Tiene agua y la parcela no esta llena
                {
                    GameManager.Instance.CurrentWater--;                                // Cosume agua del deposito
                    this.currentWater++;

                    StartCoroutine(AnimateSingleTextChange("+1", 0, Color.green));
                    this.UpdatePlotWaterVisuals();

                    SFXManager.Instance?.PlayRegar();

                    OnPlotDataUpdated?.Invoke(this);

                    Debug.Log($"Parcela {this.gridCoordinates} regada -> {currentWater} de agua");
                }
                else if (GameManager.Instance.CurrentWater > 0 && currentWater >= PLOT_LIMIT)   // Tiene agua pero la parcela está llena
                {
                    StartCoroutine(AnimateSingleTextChange("Lleno", 0, Color.green));
                    SFXManager.Instance?.PlayDenegar();
                    Debug.Log($"No se puede regar, la parcela {gridCoordinates} está al máximo de agua.");
                }
                else // No queda agua
                {
                    SFXManager.Instance?.PlayDenegar();
                    Debug.Log("No te queda agua.");
                }
                break;

            case ToolType.FertilizerBag:

                if (GameManager.Instance.CurrentFertilizer > 0 && currentFertility < PLOT_LIMIT)    // Tiene abono y la parcela no esta llena
                {
                    GameManager.Instance.CurrentFertilizer--;                                       // Consume abono del deposito
                    this.currentFertility++;

                    StartCoroutine(AnimateSingleTextChange("+1", 1, Color.green));
                    this.UpdatePlotFertilizerVisuals();

                    SFXManager.Instance?.PlayAbonar();

                    OnPlotDataUpdated?.Invoke(this);

                    Debug.Log($"Parcela {this.gridCoordinates} abonada -> {currentFertility} de abono");

                }
                else if (GameManager.Instance.CurrentFertilizer > 0 && currentFertility >= PLOT_LIMIT) // Tiene abono pero la parcela está llena
                {
                    StartCoroutine(AnimateSingleTextChange("Lleno", 1, Color.green));
                    SFXManager.Instance?.PlayDenegar();
                    Debug.Log($"No se puede abonar, la parcela {gridCoordinates} está al máximo de abono.");
                }
                else // No queda abono
                {
                    SFXManager.Instance?.PlayDenegar();
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
                    SFXManager.Instance?.PlayDesplantar();
                    RemovePlant();
                } else
                {
                    SFXManager.Instance?.PlayDenegar();
                    Debug.Log($"En la parcela {gridCoordinates} no hay ninguna planta.");
                }
                break;
        }
    }

    public void RemovePlant()
    {
        if (currentPlant == null) return;

        // Economía
        if (currentPlant.isDeath)
        {
            // Restar Economia
        }
        else
        {
            currentPlant.isDeath = true;
            currentPlant.UpdateEnvironmentEffect();

            GameManager.Instance.CurrentMoney += (currentPlant.plantData.price) / 2; // Gana la mitad de lo que vale la planta
        }

        Destroy(currentPlant.gameObject);

        // Variables parcela
        this.currentPlant = null;
        this.isPlanted = false;

        UpdatePollinatorVisual();

        GameManager.Instance.UpdateBiodiversityScore();
    }

    public void UpdateEnviroment(PlantCategory plantType)
    {
        switch (plantType)
        {
            case PlantCategory.Polinizadores:
                // Atraer polinizadores a parcelas adyacentes
                PlotsManager.Instance.GeneratePollination(this.gridCoordinates);

                break;

            case PlantCategory.Sombra:
                // Cambiar la exposicion solar de las parcelas adyacentes
                PlotsManager.Instance.GenerateShade(this.gridCoordinates);
                break;

            case PlantCategory.Productor:
                // Empezar el ciclo de produccion
  
                break;

            case PlantCategory.RefugioFauna:
                // Instanciar fauna en parcelas adyacentes
                PlotsManager.Instance.GenerateRefuge(this.gridCoordinates);
                break;
        }
    }

    private void CalculateColor()
    {
        float waterFactor = (float)currentWater / PLOT_LIMIT;
        float fertilityFactor = (float)currentFertility / PLOT_LIMIT;

    
        Color colorLerpedByWater_NoFertility = Color.Lerp(
            colorNoWaterNoFertility,
            colorFullWaterNoFertility,
            waterFactor);

        Color colorLerpedByWater_FullFertility = Color.Lerp(
            colorNoWaterFullFertility,
            colorFullWaterFullFertility,
            waterFactor);

        Color finalColor = Color.Lerp(
            colorLerpedByWater_NoFertility,
            colorLerpedByWater_FullFertility,
            fertilityFactor);

        // Aplicar el color final
        sr.color = finalColor;
    }

    public IEnumerator AnimateDailyConsumptionAndChange()
    {
        if (!isPlanted || currentPlant == null || currentPlant.isDeath)
        {
            yield break;
        }

        int waterDemand = currentPlant.GetWaterDemand();
        int fertilizerDemand = currentPlant.GetFertilizerDemand();

        // 1. Animación de agua
        if (waterDemand > 0)
        {
            string text = $"-{waterDemand}";
            // Iniciar y esperar la animación del agua antes de pasar al siguiente
            yield return StartCoroutine(AnimateSingleTextChange(text, 0, Color.blue));
        }

        // 2. Animación de abono 
        if (fertilizerDemand > 0)
        {
            string text = $"-{fertilizerDemand}";
            // Iniciar y esperar la animación del abono
            yield return StartCoroutine(AnimateSingleTextChange(text, 1, new Color(0.2f, 0.8f, 0.2f))); // Color verde oscuro para abono
        }


        // 3. Consumo real de recursos
        ChangeWaterFromConsumption(-waterDemand);
        ChangeFertility(-fertilizerDemand);
    }

    public void ChangeWaterFromConsumption(int waterChange)
    {
        // Solo aplica el consumo de la planta, no el clima
        this.currentWater += waterChange;
        this.currentWater = Mathf.Clamp(this.currentWater, 0, PLOT_LIMIT); // Mantener el limite
    }

    public void ChangeFertility(int fertilityChange)
    {
        this.currentFertility += fertilityChange;
        this.currentFertility = Mathf.Clamp(this.currentFertility, 0, PLOT_LIMIT); // Mantener el limite de la parcela
    }


    private IEnumerator AnimateSingleTextChange(string _text, int type, Color textColor)
    {
        // 1. Instanciar el prefab de la animación
        GameObject canvasInstance = Instantiate(changeCanvasPrefab, transform.position, Quaternion.identity, transform.parent);
        canvasInstance.SetActive(true);

        // 2. Obtener referencias de la NUEVA instancia
        Image changeImage = canvasInstance.GetComponentInChildren<Image>();
        TextMeshProUGUI changeText = canvasInstance.GetComponentInChildren<TextMeshProUGUI>();
        Transform textTransform = canvasInstance.transform; 
        Vector3 initialPosition = textTransform.localPosition;

        // 3. Configurar el texto y el icono
        if (type == 0 && waterIcon != null)
            changeImage.sprite = waterIcon;
        else if (type == 1 && fertilizerIcon != null)
            changeImage.sprite = fertilizerIcon;
        else
            changeImage.gameObject.SetActive(false); // Ocultar si no hay icono

        changeText.text = _text;
        changeText.color = textColor;


        // 4. Lógica de movimiento
        float duration = 1f;
        float distance = 0.6f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 targetPosition = initialPosition + Vector3.up * distance;
            textTransform.localPosition = Vector3.Lerp(initialPosition, targetPosition, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 5. Autodestruir la instancia de la animación
        Destroy(canvasInstance);
    }

    public void UpdatePlantDaily()
    {
        if (currentPlant == null || currentPlant.isDeath) return;

        // Lógica de si la planta muere por falta de recursos
        int waterDemand = currentPlant.GetWaterDemand();
        int fertilizerDemand = currentPlant.GetFertilizerDemand();

        if (waterDemand > currentWater || fertilizerDemand > currentFertility)
        {
            Debug.Log($"La parcela {gridCoordinates} no puede cubrir las necesidades de su planta");

            if (currentPlant.DecreaseHealth())              // true si la planta ha muerto
            {
                PlotsManager.Instance.PlantsDeath(this);    // Llama a la muerte
                return;                                     // Salir si la planta murió
            }
        }
        else
        {
            currentPlant.IncreaseHealth();
        }
    }


    #endregion

    #region Métodos para visualización
    public void UpdatePlotWaterVisuals()
    {
        CalculateColor();
    }

    public void UpdatePlotFertilizerVisuals()
    {
        CalculateColor();
    }

    public void UpdatePlotSolarExposureVisuals()
    {
        int max = solarExposurePlot.Count;
        sr.sprite = solarExposurePlot[Math.Min((int)this.currentSolarExposure, max)];
    }

    public void UpdatePollinatorVisual()
    {
        bool isPollinated = pollinatorSourceCount > 0;

        bool isDead = (isPlanted && currentPlant != null && currentPlant.isDeath);

        bool hasValidPlant = false;
        if (isPlanted && currentPlant != null && !isDead)
        {
            // Si es Productora, el visual es válido desde que brota
            if (currentPlant is ProducerPlant && currentPlant.currentGrowth >= GrowthState.brote)
            {
                hasValidPlant = true;
            }
            // Si es Atractora, el visual es válido cuando madura
            else if (currentPlant is PollinatorAttractorPlant && currentPlant.currentGrowth == GrowthState.madura)
            {
                hasValidPlant = true;
            }
        }

        bool isPlagued = (isPlanted && currentPlant != null && currentPlant.isPlagued);

        if (isPollinated && hasValidPlant && !isPlagued && !isDead)
        {
            if (currentPollinatorVisual == null)
            {
                currentPollinatorVisual = Instantiate(pollinatorVisualPrefab, transform.position + currentPollinatorPosition, Quaternion.identity, this.transform);
            }
        }
        else
        {
            if (currentPollinatorVisual != null)
            {
                Destroy(currentPollinatorVisual);
            }
        }
    }

    public override string ToString()
    {
        return $"Parcela {gridCoordinates} -> Agua: {currentWater}, Fertilidad: {currentFertility}, Exposicion Solar: {currentSolarExposure}";
    }

    #endregion
}
