/*
using System;
using System.Collections.Generic;

[Serializable]
public class SaveGameData
{
    public string version = "1.0.0";
    // Meta para el menú
    public string savedAt;

    // Estado global
    public int day, biodiversity, money, water, fertilizer, winCondition;
    public ToolType currentTool;

    // Clima
    public DailyWeatherData currentWeather;
    public List<DailyWeatherData> forecast = new();

    // Parcela por parcela
    public List<PlotData> plots = new();
}

[Serializable]
public class DailyWeatherData
{
    public string type;
    public int waterChange;
}

[Serializable]
public class PlotData
{
    public int x, y;
    public int currentWater, currentFertility, solarExposure;
    public bool isPlanted;
    public PlantSaveData plant; // null si no hay
}

[Serializable]
public class PlantSaveData
{
    public int plantTypeIdx;
    public string category, growth, health;
    public int lifeDays;
    public bool hasProduct;
}
*/