using UnityEngine;

public class WildlifeRefugePlant : Plant
{
    private GameObject currentRefugeVisual;
    [SerializeField] private Vector3 currentWildlifePosition = new Vector3(0f, -0.4f, 0);

    public override void UpdateEnvironmentEffect()
    {
        bool canBeActive = (currentGrowth == GrowthState.madura &&
                            !isDeath &&
                            !isPlagued);

        if (canBeActive && !isEffectActive)
        {
            // Activar efecto 
            PlotsManager.Instance.GenerateRefuge(parentPlot.gridCoordinates);
            parentPlot.AddRefugeSource();
            isEffectActive = true;

            if (plantData.refugeVisualPrefab != null && currentRefugeVisual == null)
            {
                currentRefugeVisual = Instantiate(plantData.refugeVisualPrefab, transform.position + currentWildlifePosition, Quaternion.identity, this.transform);
            }
        }
        else if (!canBeActive && isEffectActive)
        {
            // Desactivar efecto
            PlotsManager.Instance.RemoveRefuge(parentPlot.gridCoordinates);
            parentPlot.RemoveRefugeSource();
            isEffectActive = false;

            if (currentRefugeVisual != null)
            {
                Destroy(currentRefugeVisual);
            }
        }
    }

}
