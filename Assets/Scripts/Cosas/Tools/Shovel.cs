using UnityEngine;

public class Shovel : ITool
{
    public Shovel() { }

    public void Usar(Plot plot)
    {
        if (plot.isPlanted)
        {
            bool hadPlague = (plot.currentPlant != null && plot.currentPlant.isPlagued);

            SFXManager.Instance?.PlayDesplantar();

            // Avisar al tutorial
            if (hadPlague && TutorialManager.Instance != null)
            {
                TutorialManager.Instance.NotifyPlaguedPlantRemovedWithShovel(plot.currentPlant);
            }

            PlotsManager.Instance.RemovePlant(plot);
        }
        else
        {
            SFXManager.Instance?.PlayDenegar();

            Debug.Log($"En la parcela {plot.gridCoordinates} no hay ninguna planta.");
        }
    }

}
