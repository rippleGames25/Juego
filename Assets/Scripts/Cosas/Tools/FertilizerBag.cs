using UnityEngine;

public class FertilizerBag : ITool
{

    private int fertilizationSize = 1;
    int PLOT_LIMIT = 5; // PROVISIONAL

    private IFertilizerSource fertilizerSource;

    public FertilizerBag(IFertilizerSource _fertilizerSource)   // Constructor
    {
        fertilizerSource = _fertilizerSource;
    }

    public void Usar(Plot plot)
    {
        if (plot.currentFertility < PLOT_LIMIT)    // Tiene abono y la parcela no esta llena
        {            

            bool canConsumeFertilizer = fertilizerSource.TryConsumeFertilizer(fertilizationSize); 

            if (canConsumeFertilizer)
            {
                plot.AddFertilizer(fertilizationSize);
                SFXManager.Instance?.PlayAbonar();
                Debug.Log($"Parcela {plot.gridCoordinates} abonada -> {plot.currentFertility} de abono");

            }
            else
            {
                SFXManager.Instance?.PlayDenegar();
                Debug.Log("No te queda abono");
            }
   

            // Notificar al tutorial
            if (TutorialManager.Instance != null &&
                TutorialManager.Instance.initialTutorialActive)
            {
                TutorialManager.Instance.hasFertilizedOnce = true;
                TutorialManager.Instance.CheckBasicCareCompleted();
            }

        }
        else  // Tiene abono pero la parcela está llena
        {

            plot.ChangePlotAnimation("Lleno", 1, true);
            SFXManager.Instance?.PlayDenegar();

            Debug.Log($"No se puede abonar, la parcela {plot.gridCoordinates} está al máximo de abono.");
        }
    }
}
