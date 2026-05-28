using UnityEngine;

public interface IFertilizerSource
{
    bool TryConsumeFertilizer(int amount);
}
