using UnityEngine;

public class WildlifeRefugePlant : Plant
{
    private GameObject currentRefugeVisual;

    public override void ApplyDailyEffect()
    {
        if (currentGrowth == GrowthState.madura && !hasAppliedEnvironmentEffect)
        {
            parentPlot.UpdateEnviroment(plantData.category);
            hasAppliedEnvironmentEffect = true;

            // Instanciar el animal
            if (plantData.refugeVisualPrefab != null && currentRefugeVisual == null)
            {
                currentRefugeVisual = Instantiate(plantData.refugeVisualPrefab, transform.position, Quaternion.identity, this.transform);
            }
        }
    }
}
