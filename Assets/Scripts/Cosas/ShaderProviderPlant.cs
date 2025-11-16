using UnityEngine;

public class ShaderProviderPlant : Plant
{
    public override void UpdateEnvironmentEffect()
    {
        bool canBeActive = (currentGrowth == GrowthState.madura &&
                            !isDeath);

        if (canBeActive && !isEffectActive)
        {
            // Activar efecto 
            PlotsManager.Instance.GenerateShade(parentPlot.gridCoordinates);
            isEffectActive = true;
            Debug.Log($"Planta {plantData.plantName} AÑADE sombra");
        }
        else if (!canBeActive && isEffectActive)
        {
            // Desactivar efecto
            PlotsManager.Instance.RemoveShade(parentPlot.gridCoordinates);
            isEffectActive = false;
            Debug.Log($"Planta {plantData.plantName} QUITA sombra");
        }
    }
}
