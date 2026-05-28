using System;
using UnityEngine;

public class FertilizerTank : IFertilizerSource
{
    private int currentFertilizer = 8;

    public event Action<int> OnFertilizerChanged;

    public FertilizerTank(int initialAmount)
    {
        currentFertilizer = initialAmount;
    }
    public bool TryConsumeFertilizer(int amount)
    {
        if (currentFertilizer >= amount)
        {
            SetFertilizer(currentFertilizer - amount);
            return true;
        }

        return false;
    }

    private void SetFertilizer(int amount)
    {
        currentFertilizer = Math.Max(0, amount);
        OnFertilizerChanged?.Invoke(currentFertilizer);
    }

    public void RefillFertilizer (int amount)
    {
        SetFertilizer(currentFertilizer +  amount);
    }

    public int GetFertilizer() => currentFertilizer;
}
