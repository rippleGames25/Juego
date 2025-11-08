using UnityEngine;

public class ShaderProviderPlant : Plant
{
    public override void ApplyDailyEffect()
    {
        if (currentGrowth == GrowthState.madura && !hasAppliedEnvironmentEffect)
        {
            parentPlot.UpdateEnviroment(plantData.category);
            hasAppliedEnvironmentEffect = true;
        }
    }
}
