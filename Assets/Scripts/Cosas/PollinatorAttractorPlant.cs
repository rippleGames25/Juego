using UnityEngine;

public class PollinatorAttractorPlant : Plant
{
    public override void UpdateEnvironmentEffect()
    {
        // Estar madura, no estar muerta, salud buena o moderada, y no tener plaga.
        bool canBeActive = (currentGrowth == GrowthState.madura &&  // madura
                            !isDeath &&                             // viva 
                            !isPlagued);                            // sin plagas

        if (canBeActive && !isEffectActive)
        {
            // Activar efecto
            PlotsManager.Instance.GeneratePollination(parentPlot.gridCoordinates);
            parentPlot.AddPollinatorSource();
            isEffectActive = true;
        }
        else if (!canBeActive && isEffectActive)
        {
            // Desactivar efecto
            PlotsManager.Instance.RemovePollination(parentPlot.gridCoordinates);
            parentPlot.RemovePollinatorSource();
            isEffectActive = false;
        }
    }

}
