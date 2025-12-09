using UnityEngine;
using System;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    #region Campos y propiedades

    [Header("Estado Día 1")]
    public bool hasChosenAtStart = false;     // El jugador ya ha elegido si quiere ayuda
    public bool wantsDay1Tutorial = true;     // Eligió "Guíame, Leo"

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
    public LeoDialogUI leoDialogUI;           // Panel de diálogos de Leo

    // Paso actual de la intro del Día 1 (0 = "Buenos días", 1 = tienda, etc.)
    private int day1IntroStep = 0;
    private int firstPlantStep = 0;

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

    #endregion

    #region Día 1: elección inicial e introducción

    public void StartDay1Tutorial()
    {
        hasChosenAtStart = true;
        wantsDay1Tutorial = true;

        // Activa el bloque lineal del día 1
        initialTutorialActive = true;

        Debug.Log("[Tutorial] Día 1: el jugador quiere tutorial.");
    }

    public void SkipDay1Tutorial()
    {
        hasChosenAtStart = true;
        wantsDay1Tutorial = false;

        // Desactiva el bloque lineal del día 1
        initialTutorialActive = false;

        // Día 1
        day1_IntroShown = true;
        day1_FirstPlantPlaced = true;
        day1_PlotInfoShown = true;
        hasWateredOnce = true;
        hasFertilizedOnce = true;
        shownBasicCareComplete = true;

        // Resumen de día
        hasSeenDaySummaryTutorial = true;

        // Climas
        hasSeenSunTutorial = true;
        hasSeenCloudyTutorial = true;
        hasSeenRainTutorial = true;
        hasSeenHailTutorial = true;

        // Strikes
        hasShownNormalStrikeTutorial = true;
        hasShownPermanentStrikeTutorial = true;
        hasShownStrikeRemovedTutorial = true;

        // Plagas
        hasShownFirstPlagueTutorial = true;
        hasShownPlagueCuredByFaunaTutorial = true;
        hasShownPlaguedPlantRemovedWithShovelTutorial = true;

        // Tipos de planta
        hasExplainedProducer = true;
        hasExplainedShadeProvider = true;
        hasExplainedPollinator = true;
        hasExplainedRefuge = true;

        Debug.Log("[Tutorial] El jugador ha elegido jugar sin tutoriales. Todos los tutoriales quedan desactivados.");
    }


    public void ShowDay1Intro()
    {
        if (!wantsDay1Tutorial || !hasChosenAtStart)
        {
            Debug.Log("[Tutorial] ShowDay1Intro llamado pero el jugador no quiere tutorial o no ha elegido.");
            return;
        }

        if (day1_IntroShown)
        {
            Debug.Log("[Tutorial] ShowDay1Intro llamado pero la intro ya se mostró.");
            return;
        }

        if (leoDialogUI == null)
        {
            Debug.LogWarning("[Tutorial] LeoDialogUI no asignado.");
            return;
        }

        day1_IntroShown = true;
        day1IntroStep = 0;
        isDialogActive = true;

        ShowCurrentDay1IntroStep();
    }

    private void ShowCurrentDay1IntroStep()
    {
        string text;
        LeoEmotion emotion = LeoEmotion.Normal;
        float fontSize = -1f;

        if (day1IntroStep == 0)
        {
            text = "Buenos días, Curador.\n" +
                   "Soy Leo, y estaré contigo para ayudarte a despertar este santuario.";
            fontSize = 4f;
        }
        else if (day1IntroStep == 1)
        {
            text =
                "A tu izquierda tienes la tienda.\n" +
                "Aquí puedes elegir semillas para plantar.\n" +
                "También puedes comprar agua y abono, pero cuidado con no quedarte sin pétalos.";
            fontSize = 3.5f;
        }
        else if (day1IntroStep == 2)
        {
            text =
                "Ah, y no olvides mirar el pronóstico del clima, Curador.\n" +
                "Arriba a la derecha verás el tiempo de los próximos días.\n" +
                "El clima puede ayudarte… o sorprenderte, así que consúltalo a menudo.";
            fontSize = 3f;
        }
        else if (day1IntroStep == 3)
        {
            text =
                "Curador… antes de seguir, hay algo importante.\n" +
                "La biodiversidad es la vida del Santuario.\n" +
                "Aumenta cada vez que logras hacer crecer distintas especies,\n" +
                "y baja si alguna planta muere.\n" +
                "Cuanta más biodiversidad tengas, más pétalos ganarás cada día…\n" +
                "y si alcanzas el objetivo, ¡restaurarás el Santuario!";

            fontSize = 2.5f;
            emotion = LeoEmotion.Normal;
        }
        else
        {
            isDialogActive = false;
            leoDialogUI.Hide();
            Debug.Log("[Tutorial] Intro Día 1 terminada.");
            return;
        }

        if (leoDialogUI != null)
        {
            Debug.Log($"[Tutorial] ShowCurrentDay1IntroStep step={day1IntroStep}, " +
                      $"panelRoot activo antes: {leoDialogUI.panelRoot.activeSelf}");

            if (leoDialogUI.panelRoot != null)
                leoDialogUI.panelRoot.SetActive(true);
        }

        leoDialogUI.Show(
            text,
            emotion,
            OnDay1IntroContinueClicked,
            fontSize
        );
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
        string text;
        float fontSize;
        LeoEmotion emotion = LeoEmotion.Normal;

        if (firstPlantStep == 0)
        {
            text =
                "¡Perfecto, Curador!\n" +
                "Has plantado tu primera especie en el Santuario.";

            emotion = LeoEmotion.Happy;
            fontSize = 5;
        }
        else if (firstPlantStep == 1)
        {
            text =
                "Ahora toca cuidarla bien.\n" +
                "Usa la regadera para darle agua y el abono para enriquecer el suelo.\n" +
                "También puedes hacer uso de la pala para quitar plantas, recuperarás parte de la inversión.\n" +
                "Cuanto mejor atendida esté la parcela, más fuerte crecerá la planta.";

            emotion = LeoEmotion.Normal;
            fontSize = 2.6f;
        }
        else
        {
            isDialogActive = false;
            leoDialogUI.Hide();
            return;
        }

        leoDialogUI.Show(
            text,
            emotion,
            OnContinue_FirstPlant, fontSize
        );
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

        float fontSize = 3f;
        day1_PlotInfoShown = true;
        isDialogActive = true;

        string text =
            "Esta es la ficha de la parcela.\n" +
            "Aquí ves cuánta agua y abono tiene el suelo y qué exposición solar recibe.\n" +
            "Si hay una planta, también verás su estado y sus necesidades.";

        leoDialogUI.Show(text, LeoEmotion.Normal, () =>
        {
            if (leoDialogUI != null)
                leoDialogUI.Hide();

            isDialogActive = false;
            Debug.Log("[Tutorial] Paso 'info de parcela' completado.");
        }, fontSize);
    }

    public void CheckBasicCareCompleted()
    {
        if (!wantsDay1Tutorial || !hasChosenAtStart) return;
        if (shownBasicCareComplete) return;

        if (hasWateredOnce && hasFertilizedOnce)
        {
            shownBasicCareComplete = true;
            ShowBasicCareCompletedDialog();
        }
    }

    private void ShowBasicCareCompletedDialog()
    {
        isDialogActive = true;
        float fontSize = 3f;

        string text =
            "¡Vaya, Curador!\n" +
            "Veo que ya dominas los cuidados básicos de una planta.\n" +
            "Si alguna vez se te olvida algo, puedes consultar la guía rápida con el botón de la interrogación arriba.";

        leoDialogUI.Show(text, LeoEmotion.Happy, () =>
        {
            isDialogActive = false;
            leoDialogUI.Hide();
        }, fontSize);
    }

    #endregion

    #region Resumen de día y clima

    public void NotifyFirstDaySummaryShown()
    {
        float fontSize = 2.5f;

        if (!wantsDay1Tutorial || !hasChosenAtStart)
            return;

        if (hasSeenDaySummaryTutorial)
            return;

        if (leoDialogUI == null)
        {
            Debug.LogWarning("[Tutorial] LeoDialogUI no asignado para el resumen de día.");
            return;
        }

        hasSeenDaySummaryTutorial = true;
        isDialogActive = true;

        string text =
            "Este es el resumen del día, Curador.\n" +
            "Aquí ves cuántos pétalos has ganado, los bonos por tu jardín\n" +
            "y las pérdidas por plantas muertas o usar la pala.\n\n" +
            "También verás si recibes agua o abono extra.\n" +
            "Échale un vistazo cada noche para entender cómo evoluciona el Santuario.";

        leoDialogUI.Show(
            text,
            LeoEmotion.Normal,
            () =>
            {
                if (leoDialogUI != null)
                    leoDialogUI.Hide();

                isDialogActive = false;
                Debug.Log("[Tutorial] Tutorial de resumen de día mostrado por primera vez.");
            }, fontSize);
    }

    private IEnumerator WaitAndShowWeatherTutorial(System.Action showCallback)
    {
        // Espera aproximada a que termine la transición de día
        yield return new WaitForSecondsRealtime(1.2f);

        // Espera a que no haya otro diálogo activo
        yield return new WaitUntil(() => !isDialogActive);

        showCallback?.Invoke();
    }

    public void NotifyDailyWeather(DailyWeather weather)
    {
        if (!wantsDay1Tutorial || !hasChosenAtStart)
            return;

        if (GameManager.Instance != null && GameManager.Instance.CurrentDay <= 1)
            return;

        if (leoDialogUI == null)
            return;

        switch (weather.type)
        {
            case WeatherType.Soleado:
                if (!hasSeenSunTutorial)
                {
                    hasSeenSunTutorial = true;
                    StartCoroutine(WaitAndShowWeatherTutorial(ShowSunnyWeatherTutorial));
                }
                break;

            case WeatherType.Nublado:
                if (!hasSeenCloudyTutorial)
                {
                    hasSeenCloudyTutorial = true;
                    StartCoroutine(WaitAndShowWeatherTutorial(ShowCloudyWeatherTutorial));
                }
                break;

            case WeatherType.Lluvia:
                if (!hasSeenRainTutorial)
                {
                    hasSeenRainTutorial = true;
                    StartCoroutine(WaitAndShowWeatherTutorial(ShowRainWeatherTutorial));
                }
                break;

            case WeatherType.Granizo:
                if (!hasSeenHailTutorial)
                {
                    hasSeenHailTutorial = true;
                    StartCoroutine(WaitAndShowWeatherTutorial(ShowHailWeatherTutorial));
                }
                break;
        }
    }

    private void ShowSunnyWeatherTutorial()
    {
        isDialogActive = true;

        string t1 =
            "Vaya, Curador… hoy tenemos un día soleado.\n" +
            "El sol no riega el suelo, al contrario: poco a poco lo va secando.";

        string t2 =
            "Fíjate bien en el pronóstico de arriba a la derecha:\n" +
            "el sol puede aparecer con tres intensidades.\n" +
            "• Intensidad 1: a veces seca 1 punto de agua de las parcelas.\n" +
            "• Intensidad 2: puede secar 1 o 2 puntos.\n" +
            "• Intensidad 3: puede llegar a secar hasta 3 puntos de agua.";

        string t3 =
            "El sol por sí solo no mata plantas,\n" +
            "pero si dejas el suelo demasiado seco, tus plantas sufrirán luego\n" +
            "por falta de recursos.\n" +
            "Ve revisando el agua de tus parcelas y riega cuando lo necesiten.";

        leoDialogUI.Show(t1, LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(t2, LeoEmotion.Normal, () =>
            {
                leoDialogUI.Show(t3, LeoEmotion.Normal, () =>
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

        string t1 =
            "Hoy el cielo está cubierto, Curador.\n" +
            "Este clima es muy tranquilo: no riega ni seca el suelo, " +
            "da igual la intensidad.";

        string t2 =
            "Piensa en él como un respiro para el jardín:\n" +
            "puedes aprovechar para planificar, reorganizar tus plantas\n" +
            "y prepararte para días más duros.";

        leoDialogUI.Show(t1, LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(t2, LeoEmotion.Normal, () =>
            {
                isDialogActive = false;
                leoDialogUI.Hide();
            }, 3.5f);
        }, 3.5f);
    }

    private void ShowRainWeatherTutorial()
    {
        isDialogActive = true;

        string t1 =
            "Vaya, Curador… hoy llueve en el Santuario.\n" +
            "La lluvia siempre aumenta el agua del suelo de todas las parcelas.";

        string t2 =
            "Fíjate bien en el pronóstico, porque la lluvia también tiene " +
            "tres intensidades:\n" +
            "• Intensidad 1: lluvia suave, añade +2 de agua.\n" +
            "• Intensidad 2: lluvia moderada, añade +4 de agua.\n" +
            "• Intensidad 3: lluvia torrencial, añade +6 de agua " +
            "y puede poner en peligro a las plantas débiles.";

        string t3 =
            "La lluvia suave y moderada son perfectas para recuperar parcelas secas.\n" +
            "Pero con lluvia torrencial, las plantas con mala salud pueden llegar a morir.\n" +
            "Si ves una lluvia muy fuerte acercarse en el pronóstico,\n" +
            "intenta que tus plantas estén lo más sanas posible antes de que llegue.";

        leoDialogUI.Show(t1, LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(t2, LeoEmotion.Normal, () =>
            {
                leoDialogUI.Show(t3, LeoEmotion.Normal, () =>
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

        string t1 =
            "Vaya, Curador… hoy cae granizo.\n" +
            "Es uno de los climas más peligrosos para tu jardín.";

        string t2 =
            "Al igual que la lluvia, el granizo puede caer con tres intensidades\n" +
            "y también añade algo de agua al suelo:\n" +
            "• Intensidad 1: granizo ligero, +1 de agua.\n" +
            "• Intensidad 2: granizo moderado, +2 de agua.\n" +
            "• Intensidad 3: granizo fuerte, +3 de agua y un golpe muy duro.";

        string t3 =
            "Lo importante del granizo no es el agua, sino el daño:\n" +
            "en cualquiera de sus intensidades puede matar plantas que estén en mala salud,\n" +
            "y cuanto más fuerte es, más sufren incluso las que están sólo moderadas.\n" +
            "Si en el pronóstico ves granizo acercarse,\n" +
            "intenta que tus plantas lleguen a ese día lo más sanas posible.";

        leoDialogUI.Show(t1, LeoEmotion.Scared, () =>
        {
            leoDialogUI.Show(t2, LeoEmotion.Scared, () =>
            {
                leoDialogUI.Show(t3, LeoEmotion.Normal, () =>
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

    // Espera a que empiece el siguiente día y termine la transición,
    // para sincronizar con el momento en que aparece el popup de strike.
    private IEnumerator WaitUntilStrikePopupMoment()
    {
        // Esperar a que comience el siguiente día
        if (GameManager.Instance != null)
        {
            int dayWhenStrikeWasComputed = GameManager.Instance.CurrentDay;

            yield return new WaitUntil(() =>
                GameManager.Instance != null &&
                GameManager.Instance.CurrentDay > dayWhenStrikeWasComputed);
        }

        // Una vez en el nuevo día, esperar a que termine la transición de HUDUI
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
        ShowNormalStrike_Step1();
    }

    private void ShowNormalStrike_Step1()
    {
        string t1 =
            "Curador… acabas de recibir un *strike*.\n" +
            "Son avisos de que el Santuario está en peligro.\n" +
            "Si llegas a 5 strikes (amarillos o rojos),\n" +
            "el Santuario colapsará y perderás la partida.";

        leoDialogUI.Show(
            t1,
            LeoEmotion.Normal,
            OnNormalStrikeStep1Finished,
            3f
        );
    }

    private void OnNormalStrikeStep1Finished()
    {
        string t2 =
            "Los strikes normales aparecen cuando descuidas el jardín:\n" +
            "• Cada 2 plantas que mueren suma 1 strike.\n" +
            "• Y también si pasas varios días sin plantar\n" +
            "  teniendo pétalos suficientes para hacerlo.";

        leoDialogUI.Show(
            t2,
            LeoEmotion.Normal,
            OnNormalStrikeStep2Finished,
            3f
        );
    }

    private void OnNormalStrikeStep2Finished()
    {
        string t3 =
            "La parte buena es que los strikes normales se pueden quitar.\n" +
            "Si mantienes al menos 3 plantas vivas\n" +
            "y pasas 5 días seguidos sin que muera ninguna,\n" +
            "el Santuario perdonará uno de tus strikes.";

        leoDialogUI.Show(
            t3,
            LeoEmotion.Normal,
            OnNormalStrikeTutorialFinished,
            3f
        );
    }

    private void OnNormalStrikeTutorialFinished()
    {
        isDialogActive = false;
        if (leoDialogUI != null)
            leoDialogUI.Hide();
    }

    private IEnumerator ShowPermanentStrikeTutorialCoroutine()
    {
        yield return WaitUntilNoDialog();
        yield return WaitUntilStrikePopupMoment();

        if (leoDialogUI == null) yield break;

        isDialogActive = true;
        ShowPermanentStrike_Step1();
    }

    private void ShowPermanentStrike_Step1()
    {
        string t1 =
            "Esta vez es más grave, Curador…\n" +
            "Te has quedado sin plantas y sin pétalos suficientes\n" +
            "para volver a plantar.\n\n" +
            "El Santuario te ha dado algunos pétalos para ayudarte,\n" +
            "pero a cambio has recibido un *strike permanente* rojo.";

        leoDialogUI.Show(
            t1,
            LeoEmotion.Normal,
            OnPermanentStrikeStep1Finished,
            3f
        );
    }

    private void OnPermanentStrikeStep1Finished()
    {
        string t2 =
            "Los strikes permanentes no se pueden limpiar nunca.\n" +
            "Cuentan igual para el límite de 5 strikes,\n" +
            "así que intenta no volver a quedarte sin jardín.";

        leoDialogUI.Show(
            t2,
            LeoEmotion.Normal,
            OnPermanentStrikeTutorialFinished,
            3.5f
        );
    }

    private void OnPermanentStrikeTutorialFinished()
    {
        isDialogActive = false;
        if (leoDialogUI != null)
            leoDialogUI.Hide();
    }

    private IEnumerator ShowStrikeRemovedTutorialCoroutine()
    {
        yield return WaitUntilNoDialog();
        yield return WaitUntilStrikePopupMoment();

        if (leoDialogUI == null) yield break;

        isDialogActive = true;

        string t =
            "¡Muy bien, Curador!\n" +
            "Has cuidado tan bien de tu jardín durante varios días\n" +
            "que el Santuario ha decidido perdonarte un strike normal.\n" +
            "Si sigues manteniendo tus plantas sanas,\n" +
            "podrás limpiar más avisos.";

        leoDialogUI.Show(
            t,
            LeoEmotion.Happy,
            OnStrikeRemovedTutorialFinished,
            3f
        );
    }

    private void OnStrikeRemovedTutorialFinished()
    {
        isDialogActive = false;
        if (leoDialogUI != null)
            leoDialogUI.Hide();
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
        ShowFirstPlague_Step1();
    }

    private void ShowFirstPlague_Step1()
    {
        string t1 =
            "Curador… veo que una de tus plantas ha sido atacada por una plaga.\n" +
            "La reconocerás por el icono que aparece sobre ella.\n" +
            "No la mata al instante, pero es una mala señal para el Santuario.";

        leoDialogUI.Show(
            t1,
            LeoEmotion.Normal,
            OnFirstPlague_Step1Finished,
            3f
        );
    }

    private void OnFirstPlague_Step1Finished()
    {
        string t2 =
            "Mientras tenga plaga, esa planta no podrá recuperar salud,\n" +
            "aunque la riegues y abones correctamente.\n" +
            "Y si es una productora, dejará de darte frutos\n" +
            "aunque tenga polinizadores cerca.";

        leoDialogUI.Show(
            t2,
            LeoEmotion.Normal,
            OnFirstPlague_Step2Finished,
            3f
        );
    }

    private void OnFirstPlague_Step2Finished()
    {
        string t3 =
            "Para librarte de las plagas tienes dos caminos:\n" +
            "• Plantar refugios de fauna cerca: protegen las parcelas adyacentes\n" +
            "  y con los días la fauna puede limpiar la plaga.\n" +
            "• Usar la pala sobre la planta infectada: eliminas la planta\n" +
            "  y también la plaga de esa parcela, pero pierdes parte de lo invertido.\n" +
            "Combina refugios, productoras y polinizadores para que tu jardín\n" +
            "sea resistente a futuras plagas.";

        leoDialogUI.Show(
            t3,
            LeoEmotion.Normal,
            OnFirstPlagueTutorialFinished,
            2.5f
        );
    }

    private void OnFirstPlagueTutorialFinished()
    {
        isDialogActive = false;
        if (leoDialogUI != null)
            leoDialogUI.Hide();
    }

    private IEnumerator ShowPlagueCuredByFaunaTutorialCoroutine()
    {
        yield return WaitUntilNoDialog();

        if (leoDialogUI == null) yield break;

        isDialogActive = true;

        string t1 =
            "¿Lo ves, Curador?\n" +
            "La fauna del refugio ha limpiado la plaga de esa planta.\n" +
            "Las plantas de refugio protegen las parcelas cercanas\n" +
            "y con el tiempo pueden curar las plagas que haya en su área.";

        string t2 =
            "Si tienes muchas productoras importantes,\n" +
            "compensa rodearlas de refugios de fauna.\n" +
            "Así evitas nuevas plagas y, si aparece alguna,\n" +
            "la fauna tendrá dónde vivir para poder limpiarla.";

        leoDialogUI.Show(
            t1,
            LeoEmotion.Happy,
            () =>
            {
                leoDialogUI.Show(
                    t2,
                    LeoEmotion.Normal,
                    () =>
                    {
                        isDialogActive = false;
                        if (leoDialogUI != null)
                            leoDialogUI.Hide();
                    },
                    3f
                );
            },
            3.2f
        );
    }

    private IEnumerator ShowPlaguedPlantRemovedWithShovelTutorialCoroutine()
    {
        yield return WaitUntilNoDialog();

        if (leoDialogUI == null) yield break;

        isDialogActive = true;

        string t =
            "Cuando usas la pala sobre una planta con plaga,\n" +
            "eliminas la planta y también la plaga de esa parcela.\n" +
            "Es una solución rápida cuando la infección está fuera de control,\n" +
            "pero perderás parte de los pétalos que invertiste.\n" +
            "Úsala para cortar plagas difíciles mientras rediseñas el jardín\n" +
            "con más refugios de fauna alrededor.";

        leoDialogUI.Show(
            t,
            LeoEmotion.Normal,
            () =>
            {
                isDialogActive = false;
                if (leoDialogUI != null)
                    leoDialogUI.Hide();
            },
            2.5f
        );
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

        string t1 =
            "Has plantado una productora, Curador.\n" +
            "Son las plantas que generan pétalos, pero necesitan un buen entorno para hacerlo.";

        string t2 =
            "Para producir frutos necesitan cumplir tres condiciones:\n" +
            "• Estar en buena o moderada salud.\n" +
            "• Estar libres de plagas.\n" +
            "• Tener polinizadores cerca (de una atractora sana).";

        string t3 =
            "Piensa en ellas como el corazón económico del Santuario.\n" +
            "Protégelas con refugios de fauna y sitúalas en la exposición solar adecuada\n" +
            "para que den lo mejor de sí.";

        leoDialogUI.Show(t1, LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(t2, LeoEmotion.Normal, () =>
            {
                leoDialogUI.Show(t3, LeoEmotion.Normal, () =>
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

        string t1 =
            "Has plantado una planta de sombra.\n" +
            "Su función es modificar la luz de las parcelas cercanas cuando crece lo suficiente.";

        string t2 =
            "La sombra puede transformar una zona soleada en semisombra o sombra.\n" +
            "Esto ayuda a plantas que sufren con demasiado sol,\n" +
            "pero puede perjudicar a las que necesitan luz directa.";

        string t3 =
            "Si colocas varias plantas de sombra alrededor de una parcela,\n" +
            "la acumulación puede generar zonas muy umbrías.\n" +
            "Úsalas para ajustar la exposición solar al tipo exacto que cada especie necesita.";

        leoDialogUI.Show(t1, LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(t2, LeoEmotion.Normal, () =>
            {
                leoDialogUI.Show(t3, LeoEmotion.Normal, () =>
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

        string t1 =
            "Acabas de plantar una atractora de polinizadores.\n" +
            "Estas plantas atraen la vida necesaria para que tus productoras puedan dar frutos.";

        string t2 =
            "Las parcelas cercanas recibirán polinizadores.\n" +
            "Sin ellos, las productoras no generan pétalos aunque estén sanas.\n" +
            "Colócalas estratégicamente para alimentar varias productoras a la vez.";

        string t3 =
            "Recuerda: polinizadores + productoras sanas = economía fuerte.\n" +
            "Cuida también de las atractoras, porque si enferman o mueren,\n" +
            "las productoras dejarán de recibir polinizadores.";

        leoDialogUI.Show(t1, LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(t2, LeoEmotion.Normal, () =>
            {
                leoDialogUI.Show(t3, LeoEmotion.Normal, () =>
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

        string t1 =
            "Has plantado un refugio de fauna.\n" +
            "Estas plantas atraen animales que protegen el Santuario.";

        string t2 =
            "La fauna evita que aparezcan plagas en las parcelas cercanas\n" +
            "y además puede llegar a curar plagas existentes con el paso de los días.";

        string t3 =
            "Son esenciales para que tus productoras y atractoras no queden inutilizadas.\n" +
            "Distribuye los refugios alrededor de las zonas importantes\n" +
            "para mantener a raya las plagas.";

        leoDialogUI.Show(t1, LeoEmotion.Normal, () =>
        {
            leoDialogUI.Show(t2, LeoEmotion.Normal, () =>
            {
                leoDialogUI.Show(t3, LeoEmotion.Normal, () =>
                {
                    isDialogActive = false;
                    leoDialogUI.Hide();
                }, 3f);
            }, 4f);
        }, 4f);
    }

    #endregion
}
