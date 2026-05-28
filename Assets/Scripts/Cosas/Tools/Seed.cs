using UnityEngine;

public class Seed : ITool
{
    public Seed() { }

    public void Usar(Plot plot)
    {
        if (!plot.isPlanted)
        {
            int plantType = ShopManager.Instance.selectedPlantType.idx;
            GameManager.Instance.PlantSeed(plot, plantType);
        }
        else
        {
            SFXManager.Instance?.PlayDenegar();

            Debug.Log($"Parcela {plot. gridCoordinates} ocupada.");
        }
    }
}
