using UnityEngine;
using UnityEngine.Localization; // Necesario para LocalizedString
using System;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    #region Campos y propiedades

    [Header("Estado Día 1")]
    public bool hasChosenAtStart = false;
    public bool wantsDay1Tutorial = true;

    [Header("Estado runtime")]
    public bool isDialogActive = false;
    public bool day1_IntroShown = false;
    public bool day1_FirstPlantPlaced = false;
    public bool day1_PlotInfoShown = false;
    public bool hasWateredOnce = false;
    public bool hasFertilizedOnce = false;
    public bool shownBasicCareComplete = false;
    public bool initialTutorialActive = false;

    [Header("Estado otros tutoriales")]
    public bool hasSeenDaySummaryTutorial = false;

    [Header("Climas - tutorial contextual")]
    public bool hasSeenSunTutorial = false;
    public bool hasSeenCloudyTutorial = false;
    public bool hasSeenRainTutorial = false;
    public bool hasSeenHailTutorial = false;

    [Header("Strikes Tutorial")]
    public bool hasShownNormalStrikeTutorial = false;
    public bool hasShownPermanentStrikeTutorial = false;
    public bool hasShownStrikeRemovedTutorial = false;

    [Header("Plagas - tutorial contextual")]
    public bool hasShownFirstPlagueTutorial = false;
    public bool hasShownPlagueCuredByFaunaTutorial = false;
    public bool hasShownPlaguedPlantRemovedWithShovelTutorial = false;

    [Header("Tipos de planta - tutorial")]
    public bool hasExplainedProducer = false;
    public bool hasExplainedShadeProvider = false;
    public bool hasExplainedPollinator = false;
    public bool hasExplainedRefuge = false;

    [Header("UI")]
    public LeoDialogUI leoDialogUI;

    // Pasos internos
    private int day1IntroStep = 0;
    private int firstPlantStep = 0;

    #endregion

    #region Referencias de Localización (LocalizedString)

    [Header("Textos - Día 1 Intro")]
    public LocalizedString txt_Day1_Intro_0;
    public LocalizedString txt_Day1_Intro_1;
    public LocalizedString txt_Day1_Intro_2;
    public LocalizedString txt_Day1_Intro_3;

    [Header("Textos - Día 1 Primera Planta")]
    public LocalizedString txt_Day1_FirstPlant_0;
    public LocalizedString txt_Day1_FirstPlant_1;

    [Header("Textos - Día 1 Info Parcela")]
    public LocalizedString txt_Day1_PlotInfo;

    [Header("Textos - Día 1 Cuidados Completos")]
    public LocalizedString txt_Day1_BasicCare;

    [Header("Textos - Resumen Día")]
    public LocalizedString txt_DaySummary;

    [Header("Textos - Clima Soleado")]
    public LocalizedString txt_Weather_Sun_1;
    public LocalizedString txt_Weather_Sun_2;
    public LocalizedString txt_Weather_Sun_3;

    [Header("Textos - Clima Nublado")]
    public LocalizedString txt_Weather_Cloud_1;
    public LocalizedString txt_Weather_Cloud_2;

    [Header("Textos - Clima Lluvia")]
    public LocalizedString txt_Weather_Rain_1;
    public LocalizedString txt_Weather_Rain_2;
    public LocalizedString txt_Weather_Rain_3;

    [Header("Textos - Clima Granizo")]
    public LocalizedString txt_Weather_Hail_1;
    public LocalizedString txt_Weather_Hail_2;
    public LocalizedString txt_Weather_Hail_3;

    [Header("Textos - Strikes Normales")]
    public LocalizedString txt_StrikeNormal_1;
    public LocalizedString txt_StrikeNormal_2;
    public LocalizedString txt_StrikeNormal_3;

    [Header("Textos - Strikes Permanentes")]
    public LocalizedString txt_StrikePerm_1;
    public LocalizedString txt_StrikePerm_2;

    [Header("Textos - Strike Removido")]
    public LocalizedString txt_StrikeRemoved;

    [Header("Textos - Plagas")]
    public LocalizedString txt_Plague_Infected_1;
    public LocalizedString txt_Plague_Infected_2;
    public LocalizedString txt_Plague_Infected_3;

    [Header("Textos - Plaga Curada por Fauna")]
    public LocalizedString txt_Plague_Cured_1;
    public LocalizedString txt_Plague_Cured_2;

    [Header("Textos - Plaga Eliminada con Pala")]
    public LocalizedString txt_Plague_Shovel;

    [Header("Textos - Planta Productora")]
    public LocalizedString txt_Type_Producer_1;
    public LocalizedString txt_Type_Producer_2;
    public LocalizedString txt_Type_Producer_3;

    [Header("Textos - Planta Sombra")]
    public LocalizedString txt_Type_Shade_1;
    public LocalizedString txt_Type_Shade_2;
    public LocalizedString txt_Type_Shade_3;

    [Header("Textos - Planta Atractora")]
    public LocalizedString txt_Type_Pollinator_1;
    public LocalizedString txt_Type_Pollinator_2;
    public LocalizedString txt_Type_Pollinator_3;

    [Header("Textos - Planta Refugio")]
    public LocalizedString txt_Type_Refuge_1;
    public LocalizedString txt_Type_Refuge_2;
    public LocalizedString txt_Type_Refuge_3;

    #endregion

    #region Ciclo de vida Unity

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNewStrike += HandleNewStrikeTutorial;
            GameManager.Instance.OnStrikeRemoved += HandleStrikeRemovedTutorial;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnNewStrike -= HandleNewStrikeTutorial;
            GameManager.Instance.OnStrikeRemoved -= HandleStrikeRemovedTutorial;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #endregion

    #region Utilidades compartidas

    private IEnumerator WaitUntilNoDialog()
    {
        while (isDialogActive)
            yield return null;
    }

    // Método helper para evitar errores si la LocalizedString está vacía
    private string GetText(LocalizedString locString)
    {
        if (locString == null || locString.IsEmpty) return "MISSING LOCALE";
        return locString.GetLocalizedString();
    }

    #endregion

    #region Día 1: elección inicial e introducción

    public void StartDay1Tutorial()
    {
        hasChosenAtStart = true;
        wantsDay1Tutorial = true;
        initialTutorialActive = true;
        Debug.Log("[Tutorial] Día 1: el jugador quiere tutorial.");
    }

    public void SkipDay1Tutorial()
    {
        hasChosenAtStart = true;
        wantsDay1Tutorial = false;
        initialTutorialActive = false;

        // Marcar todo como visto
        day1_IntroShown = true;
        day1_FirstPlantPlaced = true;
        day1_PlotInfoShown = true;
        hasWateredOnce = true;
        hasFertilizedOnce = true;
        shownBasicCareComplete = true;
        hasSeenDaySummaryTutorial = true;
        hasSeenSunTutorial = true;
        hasSeenCloudyTutorial = true;
        hasSeenRainTutorial = true;
        hasSeenHailTutorial = true;
        hasShownNormalStrikeTutorial = true;
        hasShownPermanentStrikeTutorial = true;
        hasShownStrikeRemovedTutorial = true;
        hasShownFirstPlagueTutorial = true;
        hasShownPlagueCuredByFaunaTutorial = true;
        hasShownPlaguedPlantRemovedWithShovelTutorial = true;
        hasExplainedProducer = true;
        hasExplainedShadeProvider = true;
        hasExplainedPollinator = true;
        hasExplainedRefuge = true;

        Debug.Log("[Tutorial] El jugador ha elegido jugar sin tutoriales.");
    }

    public void ShowDay1Intro()
    {
        if (!wantsDay1Tutorial || !hasChosenAtStart) return;
        if (day1_IntroShown) return;
        if (leoDialogUI == null) return;

        day1_IntroShown = true;
        day1IntroStep = 0;
        isDialogActive = true;
        ShowCurrentDay1IntroStep();
    }

    private void ShowCurrentDay1IntroStep()
    {
        string text = "";
        LeoEmotion emotion = LeoEmotion.Normal;
        float fontSize = -1f;

        if (day1IntroStep == 0)
        {
            text = GetText(txt_Day1_Intro_0);
            fontSize = 4f;
        }
        else if (day1IntroStep == 1)
        {
            text = GetText(txt_Day1_Intro_1);
            fontSize = 3.5f;
        }
        else if (day1IntroStep == 2)
        {
            text = GetText(txt_Day1_Intro_2);
            fontSize = 3f;
        }
        else if (day1IntroStep == 3)
        {
            text = GetText(txt_Day1_Intro_3);
            fontSize = 2.5f;
        }
        else
        {
            isDialogActive = false;
            leoDialogUI.Hide();
            return;
        }

        if (leoDialogUI != null && leoDialogUI.panelRoot != null)
            leoDialogUI.panelRoot.SetActive(true);

        leoDialogUI.Show(text, emotion, OnDay1IntroContinueClicked, fontSize);
    }

    private void OnDay1IntroContinueClicked()
    {
        day1IntroStep++;
        ShowCurrentDay1IntroStep();
    }

    public void NotifyFirstPlantPlaced(Plot plot)
    {
        if (day1_FirstPlantPlaced) return;
        if (!wantsDay1Tutorial || !hasChosenAtStart) return;
        if (leoDialogUI == null) return;

        day1_FirstPlantPlaced = true;
        firstPlantStep = 0;
        isDialogActive = true;
        ShowFirstPlantStep();
    }

    private void ShowFirstPlantStep()
    {
        string text = "";
        float fontSize = 3f;
        LeoEmotion emotion = LeoEmotion.Normal;

        if (firstPlantStep == 0)
        {
            text = GetText(txt_Day1_FirstPlant_0);
            emotion = LeoEmotion.Happy;
            fontSize = 5;
        }
        else if (firstPlantStep == 1)
        {
            text = GetText(txt_Day1_FirstPlant_1);
            fontSize = 2.6f;
        }
        else
        {
            isDialogActive = false;
            leoDialogUI.Hide();
            return;
        }

        leoDialogUI.Show(text, emotion, OnContinue_FirstPlant, fontSize);
    }

    private void OnContinue_FirstPlant()
    {
        firstPlantStep++;
        ShowFirstPlantStep();
    }

    public void NotifyPlotSelectedDuringDay1(Plot plot)
    {
        if (day1_PlotInfoShown) return;
        if (!wantsDay1Tutorial || !hasChosenAtStart) return;
        if (plot == null) return;
        if (leoDialogUI == null) return;

        day1_PlotInfoShown = true;
        isDialogActive = true;

        leoDialogUI.Show(GetText(txt_Day1_PlotInfo), LeoEmotion.Normal, () =>
        {
            if (leoDialogUI != null) leoDialogUI.Hide();
            isDialogActive = false;
        }, 3f);
    }

    public void CheckBasicCareCompleted()
    {
        if (!wantsDay1Tutorial || !hasChosenAtStart) return;
        if (shownBasicCareComplete) return;

        if (hasWateredOnce && hasFertilizedOnce)
        {
            shownBasicCareComplete = true;
            isDialogActive = true;
            leoDialogUI.Show(GetText(txt_Day1_BasicCare), LeoEmotion.Happy, () =>
            {
                isDialogActive = false;
                leoDialogUI.Hide();
            }, 3f);
        }
    }

    #endregion

    #region Resumen de día y clima

    public void NotifyFirstDaySummaryShown()
    {
        if (!wantsDay1Tutorial || !hasChosenAtStart) return;
        if (hasSeenDaySummaryTutorial) return;
        if (leoDialogUI == null) return;

        hasSeenDaySummaryTutorial = true;
        isDialogActive = true;

        leoDialogUI.Show(GetText(txt_DaySummary), LeoEmotion.Normal, () =>
        {
            if (leoDialogUI != null) leoDialogUI.Hide();
            isDialogActive = false;
        }, 2.5f);
    }

    private IEnumerator WaitAndShowWeatherTutorial(System.Action showCallback)
    {
        yield return new WaitForSecondsRealtime(1.2f);
        yield return new WaitUntil(() => !isDialogActive);
        showCallback?.Invoke();
    }

    public void NotifyDailyWeather(DailyWeather weather)
    {
        if (!wantsDay1Tutorial || !hasChosenAtStart) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentDay <= 1) return;
        if (leoDialogUI == null) return;

        switch (weather.type)
        {
            case WeatherType.Soleado:
                if (!hasSeenSunTutorial) { hasSeenSunTutorial = true; StartCoroutine(WaitAndShowWeatherTutorial(ShowSunnyWeatherTutorial)); }
                break;
            case WeatherType.Nublado:
                if (!hasSeenCloudyTutorial) { hasSeenCloudyTutorial = true; StartCoroutine(WaitAndShowWeatherTutorial(ShowCloudyWeatherTutorial)); }
                break;
            case WeatherType.Lluvia:
                if (!hasSeenRainTutorial) { hasSeenRainTutorial = true; StartCoroutine(WaitAndShowWeatherTutorial(ShowRainWeatherTutorial)); }
                break;
            case WeatherType.Granizo:
                if (!hasSeenHailTutorial) { hasSeenHailTutorial = true; StartCoroutine(WaitAndShowWeatherTutorial(ShowHailWeatherTutorial)); }
                break;
        }
    }

    private void ShowSunnyWeatherTutorial()
    {
        isDialogActive = true;
        leoDialogUI.Show(GetText(txt_Weather_Sun_1), LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(GetText(txt_Weather_Sun_2), LeoEmotion.Normal, () =>
            {
                leoDialogUI.Show(GetText(txt_Weather_Sun_3), LeoEmotion.Normal, () =>
                {
                    isDialogActive = false;
                    leoDialogUI.Hide();
                }, 2.5f);
            }, 2.5f);
        }, 4f);
    }

    private void ShowCloudyWeatherTutorial()
    {
        isDialogActive = true;
        leoDialogUI.Show(GetText(txt_Weather_Cloud_1), LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(GetText(txt_Weather_Cloud_2), LeoEmotion.Normal, () =>
            {
                isDialogActive = false;
                leoDialogUI.Hide();
            }, 3.5f);
        }, 3.5f);
    }

    private void ShowRainWeatherTutorial()
    {
        isDialogActive = true;
        leoDialogUI.Show(GetText(txt_Weather_Rain_1), LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(GetText(txt_Weather_Rain_2), LeoEmotion.Normal, () =>
            {
                leoDialogUI.Show(GetText(txt_Weather_Rain_3), LeoEmotion.Normal, () =>
                {
                    isDialogActive = false;
                    leoDialogUI.Hide();
                }, 2.5f);
            }, 2.5f);
        }, 4f);
    }

    private void ShowHailWeatherTutorial()
    {
        isDialogActive = true;
        leoDialogUI.Show(GetText(txt_Weather_Hail_1), LeoEmotion.Scared, () =>
        {
            leoDialogUI.Show(GetText(txt_Weather_Hail_2), LeoEmotion.Scared, () =>
            {
                leoDialogUI.Show(GetText(txt_Weather_Hail_3), LeoEmotion.Normal, () =>
                {
                    isDialogActive = false;
                    leoDialogUI.Hide();
                }, 2.3f);
            }, 2.5f);
        }, 4f);
    }

    #endregion

    #region Strikes

    private void HandleNewStrikeTutorial(int normalStrikes, int permanentStrikes, StrikeReason reason)
    {
        if (!wantsDay1Tutorial || !hasChosenAtStart) return;
        if (leoDialogUI == null) return;

        if (reason == StrikeReason.Bankruptcy)
        {
            if (hasShownPermanentStrikeTutorial) return;
            hasShownPermanentStrikeTutorial = true;
            StartCoroutine(ShowPermanentStrikeTutorialCoroutine());
        }
        else
        {
            if (hasShownNormalStrikeTutorial) return;
            hasShownNormalStrikeTutorial = true;
            StartCoroutine(ShowNormalStrikeTutorialCoroutine());
        }
    }

    private void HandleStrikeRemovedTutorial(int normalStrikes, int permanentStrikes, StrikeRemovedReason reason)
    {
        if (!wantsDay1Tutorial || !hasChosenAtStart) return;
        if (leoDialogUI == null) return;

        if (hasShownStrikeRemovedTutorial) return;
        hasShownStrikeRemovedTutorial = true;
        StartCoroutine(ShowStrikeRemovedTutorialCoroutine());
    }

    private IEnumerator WaitUntilStrikePopupMoment()
    {
        if (GameManager.Instance != null)
        {
            int dayWhenStrikeWasComputed = GameManager.Instance.CurrentDay;
            yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.CurrentDay > dayWhenStrikeWasComputed);
        }
        HUDUI hud = FindObjectOfType<HUDUI>();
        if (hud != null)
        {
            float total = hud.DayFadeDuration + hud.DayHoldDuration + 0.05f;
            yield return new WaitForSecondsRealtime(total);
        }
    }

    private IEnumerator ShowNormalStrikeTutorialCoroutine()
    {
        yield return WaitUntilNoDialog();
        yield return WaitUntilStrikePopupMoment();
        if (leoDialogUI == null) yield break;

        isDialogActive = true;
        leoDialogUI.Show(GetText(txt_StrikeNormal_1), LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(GetText(txt_StrikeNormal_2), LeoEmotion.Normal, () =>
            {
                leoDialogUI.Show(GetText(txt_StrikeNormal_3), LeoEmotion.Normal, () =>
                {
                    isDialogActive = false;
                    leoDialogUI.Hide();
                }, 3f);
            }, 3f);
        }, 3f);
    }

    private IEnumerator ShowPermanentStrikeTutorialCoroutine()
    {
        yield return WaitUntilNoDialog();
        yield return WaitUntilStrikePopupMoment();
        if (leoDialogUI == null) yield break;

        isDialogActive = true;
        leoDialogUI.Show(GetText(txt_StrikePerm_1), LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(GetText(txt_StrikePerm_2), LeoEmotion.Normal, () =>
            {
                isDialogActive = false;
                leoDialogUI.Hide();
            }, 3.5f);
        }, 3f);
    }

    private IEnumerator ShowStrikeRemovedTutorialCoroutine()
    {
        yield return WaitUntilNoDialog();
        yield return WaitUntilStrikePopupMoment();
        if (leoDialogUI == null) yield break;

        isDialogActive = true;
        leoDialogUI.Show(GetText(txt_StrikeRemoved), LeoEmotion.Happy, () =>
        {
            isDialogActive = false;
            leoDialogUI.Hide();
        }, 3f);
    }

    #endregion

    #region Plagas

    public void NotifyPlantInfected(Plant plant)
    {
        if (!wantsDay1Tutorial || !hasChosenAtStart) return;
        if (hasShownFirstPlagueTutorial) return;
        if (leoDialogUI == null) return;

        hasShownFirstPlagueTutorial = true;
        StartCoroutine(ShowFirstPlagueTutorialCoroutine());
    }

    public void NotifyPlagueCuredByFauna(Plant plant)
    {
        if (!wantsDay1Tutorial || !hasChosenAtStart) return;
        if (hasShownPlagueCuredByFaunaTutorial) return;
        if (leoDialogUI == null) return;

        hasShownPlagueCuredByFaunaTutorial = true;
        StartCoroutine(ShowPlagueCuredByFaunaTutorialCoroutine());
    }

    public void NotifyPlaguedPlantRemovedWithShovel(Plant plant)
    {
        if (!wantsDay1Tutorial || !hasChosenAtStart) return;
        if (hasShownPlaguedPlantRemovedWithShovelTutorial) return;
        if (leoDialogUI == null) return;

        hasShownPlaguedPlantRemovedWithShovelTutorial = true;
        StartCoroutine(ShowPlaguedPlantRemovedWithShovelTutorialCoroutine());
    }

    private IEnumerator ShowFirstPlagueTutorialCoroutine()
    {
        yield return WaitUntilNoDialog();
        if (leoDialogUI == null) yield break;

        isDialogActive = true;
        leoDialogUI.Show(GetText(txt_Plague_Infected_1), LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(GetText(txt_Plague_Infected_2), LeoEmotion.Normal, () =>
            {
                leoDialogUI.Show(GetText(txt_Plague_Infected_3), LeoEmotion.Normal, () =>
                {
                    isDialogActive = false;
                    leoDialogUI.Hide();
                }, 2.5f);
            }, 3f);
        }, 3f);
    }

    private IEnumerator ShowPlagueCuredByFaunaTutorialCoroutine()
    {
        yield return WaitUntilNoDialog();
        if (leoDialogUI == null) yield break;

        isDialogActive = true;
        leoDialogUI.Show(GetText(txt_Plague_Cured_1), LeoEmotion.Happy, () =>
        {
            leoDialogUI.Show(GetText(txt_Plague_Cured_2), LeoEmotion.Normal, () =>
            {
                isDialogActive = false;
                leoDialogUI.Hide();
            }, 3f);
        }, 3.2f);
    }

    private IEnumerator ShowPlaguedPlantRemovedWithShovelTutorialCoroutine()
    {
        yield return WaitUntilNoDialog();
        if (leoDialogUI == null) yield break;

        isDialogActive = true;
        leoDialogUI.Show(GetText(txt_Plague_Shovel), LeoEmotion.Normal, () =>
        {
            isDialogActive = false;
            leoDialogUI.Hide();
        }, 2.5f);
    }

    #endregion

    #region Tipos de planta

    public void NotifyPlantCategoryPlanted(PlantType plantType)
    {
        if (!wantsDay1Tutorial || !hasChosenAtStart) return;
        if (plantType == null) return;
        if (leoDialogUI == null) return;

        switch (plantType.category)
        {
            case PlantCategory.Producer:
                if (hasExplainedProducer) return;
                hasExplainedProducer = true;
                StartCoroutine(ShowProducerTutorialCoroutine());
                break;
            case PlantCategory.ShadeProvider:
                if (hasExplainedShadeProvider) return;
                hasExplainedShadeProvider = true;
                StartCoroutine(ShowShadeProviderTutorialCoroutine());
                break;
            case PlantCategory.PollinatorAtractor:
                if (hasExplainedPollinator) return;
                hasExplainedPollinator = true;
                StartCoroutine(ShowPollinatorTutorialCoroutine());
                break;
            case PlantCategory.WildlifeRefuge:
                if (hasExplainedRefuge) return;
                hasExplainedRefuge = true;
                StartCoroutine(ShowRefugeTutorialCoroutine());
                break;
        }
    }

    private IEnumerator ShowProducerTutorialCoroutine()
    {
        yield return WaitUntilNoDialog();
        if (leoDialogUI == null) yield break;
        isDialogActive = true;

        leoDialogUI.Show(GetText(txt_Type_Producer_1), LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(GetText(txt_Type_Producer_2), LeoEmotion.Normal, () =>
            {
                leoDialogUI.Show(GetText(txt_Type_Producer_3), LeoEmotion.Normal, () =>
                {
                    isDialogActive = false;
                    leoDialogUI.Hide();
                }, 3f);
            }, 2.5f);
        }, 4f);
    }

    private IEnumerator ShowShadeProviderTutorialCoroutine()
    {
        yield return WaitUntilNoDialog();
        if (leoDialogUI == null) yield break;
        isDialogActive = true;

        leoDialogUI.Show(GetText(txt_Type_Shade_1), LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(GetText(txt_Type_Shade_2), LeoEmotion.Normal, () =>
            {
                leoDialogUI.Show(GetText(txt_Type_Shade_3), LeoEmotion.Normal, () =>
                {
                    isDialogActive = false;
                    leoDialogUI.Hide();
                }, 3f);
            }, 3f);
        }, 4f);
    }

    private IEnumerator ShowPollinatorTutorialCoroutine()
    {
        yield return WaitUntilNoDialog();
        if (leoDialogUI == null) yield break;
        isDialogActive = true;

        leoDialogUI.Show(GetText(txt_Type_Pollinator_1), LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(GetText(txt_Type_Pollinator_2), LeoEmotion.Normal, () =>
            {
                leoDialogUI.Show(GetText(txt_Type_Pollinator_3), LeoEmotion.Normal, () =>
                {
                    isDialogActive = false;
                    leoDialogUI.Hide();
                }, 3f);
            }, 3f);
        }, 4f);
    }

    private IEnumerator ShowRefugeTutorialCoroutine()
    {
        yield return WaitUntilNoDialog();
        if (leoDialogUI == null) yield break;
        isDialogActive = true;

        leoDialogUI.Show(GetText(txt_Type_Refuge_1), LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(GetText(txt_Type_Refuge_2), LeoEmotion.Normal, () =>
            {
                leoDialogUI.Show(GetText(txt_Type_Refuge_3), LeoEmotion.Normal, () =>
                {
                    isDialogActive = false;
                    leoDialogUI.Hide();
                }, 3f);
            }, 4f);
        }, 4f);
    }

    #endregion
}