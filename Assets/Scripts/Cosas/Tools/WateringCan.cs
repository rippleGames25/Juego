using UnityEngine;

public class WateringCan : ITool
{
    private int wateredSize = 1;

    int PLOT_LIMIT = 5; // PROVISIONAL

    private IWaterSource waterSource;   

    public WateringCan(IWaterSource _waterSource)   // Constructor
    {
        waterSource = _waterSource;
    }

    public void Usar(Plot plot)
    {
        if (plot.currentWater < PLOT_LIMIT) // Tiene agua y la parcela no esta llena
        {
            bool canConsumeWater = waterSource.TryConsumeWater(wateredSize);                                // Cosume agua del deposito

            if (canConsumeWater)
            {
                plot.AddWater(wateredSize);
                SFXManager.Instance?.PlayRegar();
                Debug.Log($"Parcela {plot.gridCoordinates} regada -> {plot.currentWater} de agua");

            } else
            {
                SFXManager.Instance?.PlayDenegar();
                Debug.Log("No te queda agua.");
            }

            // Notificar al tutorial
            if (TutorialManager.Instance != null &&
                TutorialManager.Instance.initialTutorialActive)
            {
                TutorialManager.Instance.hasWateredOnce = true;
                TutorialManager.Instance.CheckBasicCareCompleted();
            }

            
        }
        else // Tiene agua pero la parcela está llena
        {
            plot.ChangePlotAnimation("Lleno", 0, true);

            SFXManager.Instance?.PlayDenegar();

            Debug.Log($"No se puede regar, la parcela {plot.gridCoordinates} está al máximo de agua.");
        }
    }
}
