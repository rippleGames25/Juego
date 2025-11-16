using System.Linq;
using UnityEngine;

public class ProducerPlant : Plant
{

    public override void ApplyDailyEffect()
    {
        if (currentGrowth == GrowthState.madura && !isDeath && currentHealth!=Health.mala && !hasProduct && !isPlagued)
        {
            if (parentPlot.IsPollinated)
            {
                ProduceCycle();
            }
        }
    }

    #region Producers Methods
    public void ProduceCycle()
    {
        if (hasProduct == true) return;

        produceDays++;

        if (produceDays == plantData.timeToProduce)
        {
            hasProduct = true;
            produceDays = 0;
            UpdatePlantSprite();
        }
    }

    public void CollectProduct()
    {
        GameManager.Instance.CurrentMoney += plantData.price; // Suma el valor de la planta
        hasProduct = false;
        UpdatePlantSprite();
        SFXManager.Instance?.PlayComprar();
    }

    protected override void UpdatePlantSprite()
    {
        if (isDeath) // Planta muerta
        {
            spriteRenderer.sprite = plantData.deathSprite;

            if (plagueVisualInstance != null)
            {
                Destroy(plagueVisualInstance);
            }
            return;
        }

        if (currentGrowth == GrowthState.semilla) // Semilla
        {
            spriteRenderer.sprite = plantData.plantSprites[0];
            return;
        }

        // Logica para calcular el idx del sprite
        int growthBaseIndex = 0;
        int healthOffset = (int)currentHealth; 

        if (currentGrowth == GrowthState.brote)
        {
            growthBaseIndex = 1; 
        }
        else if (currentGrowth == GrowthState.joven)
        {
            growthBaseIndex = 4; 
        }
        else if (currentGrowth == GrowthState.madura)
        {
            int baseMatureIndex = GameManager.IDX_PLANT_SPRITE; // 7

            if (hasProduct && currentHealth != Health.mala) // Tiene frutos y salud no es mala
            {
                int fruitOffset = 3; 

                int fruitSpriteIndex = baseMatureIndex + healthOffset + fruitOffset;

                if (fruitSpriteIndex < plantData.plantSprites.Length)
                {
                    spriteRenderer.sprite = plantData.plantSprites[fruitSpriteIndex];
                }
                else
                {
                    Debug.LogError($"ProducerPlant: No se encontró el sprite CON FRUTO para salud {currentHealth}. Índice: {fruitSpriteIndex}");
                    spriteRenderer.sprite = plantData.plantSprites.Last(); // Usa el último como emergencia
                }
                return; 
            }

            growthBaseIndex = baseMatureIndex; 
        }

        int finalIndex = growthBaseIndex + healthOffset;

        if (finalIndex < plantData.plantSprites.Length)
        {
            spriteRenderer.sprite = plantData.plantSprites[finalIndex];
        }
        else
        {
            Debug.LogError($"Plant: Índice de sprite calculado fuera de límites: {finalIndex}.");
        }
    }

    #endregion
}
