using System.Linq;
using UnityEngine;

public class ProducerPlant : Plant
{

    public override void ApplyDailyEffect()
    {
        if (currentGrowth == GrowthState.madura && !isDeath && currentHealth!=Health.mala && !hasProduct)
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
            UpdateProductVisuals();
        }
    }

    public void CollectProduct()
    {
        GameManager.Instance.CurrentMoney += plantData.price; // Suma el valor de la planta
        hasProduct = false;
        UpdateProductVisuals();
        SFXManager.Instance?.PlayComprar();
    }

    protected void UpdateProductVisuals()
    {
        if (plantData.category == PlantCategory.Producer && hasProduct)
        {
            spriteRenderer.sprite = plantData.plantSprites.Last();
        }
        else if (plantData.category == PlantCategory.Producer && !hasProduct)
        {
            spriteRenderer.sprite = plantData.plantSprites[GameManager.IDX_PLANT_SPRITE];
        }
    }


    #endregion
}
