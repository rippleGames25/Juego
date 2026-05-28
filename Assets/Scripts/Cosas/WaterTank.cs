using System;
using UnityEngine;

public class WaterTank : IWaterSource
{
    private int currentWater;

    public event Action<int> OnWaterChanged;

    public WaterTank(int initialAmount)
    {
        currentWater = initialAmount;
    }

    public bool TryConsumeWater(int amount)
    {
        if (currentWater >= amount)
        {
            SetWater(currentWater - amount);
            return true;
        } 
        
        return false;
    }

    public void RefillWater(int amount)
    {
        SetWater(currentWater + amount);
    }

    private void SetWater(int amount)
    {
        currentWater = Mathf.Max(0, amount);
        OnWaterChanged?.Invoke(currentWater);
    }

    public int GetCurrentWater() => currentWater;

}
