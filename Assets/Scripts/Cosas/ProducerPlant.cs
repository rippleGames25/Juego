using System.Linq;
using UnityEngine;

public class ProducerPlant : Plant
{

    public override void ApplyDailyEffect()
    {
        if (currentGrowth == GrowthState.madura)
        {
            ProduceCycle();
        }

        if (plantData.category == PlantCategory.ProvidesShade && !hasAppliedEnvironmentEffect)
        {
            parentPlot.UpdateEnviroment(plantData.category);
            hasAppliedEnvironmentEffect = true;
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
        GameManager.Instance.CurrentMoney++; // De momento suma 1 petalo
        hasProduct = false;
        UpdateProductVisuals();
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
