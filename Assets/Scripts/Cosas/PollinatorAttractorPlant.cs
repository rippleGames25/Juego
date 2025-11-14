using UnityEngine;

public class PollinatorAttractorPlant : Plant
{
    public override void ApplyDailyEffect()
    {
        if (currentGrowth == GrowthState.madura && !hasAppliedEnvironmentEffect)
        {
            // Llama a la lógica ambiental (Atraer Polinizadores) una sola vez al madurar
            parentPlot.UpdateEnviroment(plantData.category);
            parentPlot.AddPollinatorSource();
            hasAppliedEnvironmentEffect = true;
        }
    }
}
