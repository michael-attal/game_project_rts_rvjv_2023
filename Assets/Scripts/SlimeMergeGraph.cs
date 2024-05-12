using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct FusionInfo : IComparable<FusionInfo>
{
    public FusionInfo(int earth, int fire, int water, int air)
    {
        EarthAmount = earth;
        FireAmount = fire;
        WaterAmount = water;
        AirAmount = air;
    }
    
    public int EarthAmount;
    public int FireAmount;
    public int WaterAmount;
    public int AirAmount;

    public static FusionInfo operator +(FusionInfo info1, FusionInfo info2)
        => new(info1.EarthAmount + info2.EarthAmount,
            info1.FireAmount + info2.FireAmount,
            info1.WaterAmount + info2.WaterAmount,
            info1.AirAmount + info2.AirAmount);
    
    public static FusionInfo operator -(FusionInfo info1, FusionInfo info2)
        => new(info1.EarthAmount - info2.EarthAmount,
            info1.FireAmount - info2.FireAmount,
            info1.WaterAmount - info2.WaterAmount,
            info1.AirAmount - info2.AirAmount);

    public static bool operator <=(FusionInfo info1, FusionInfo info2)
        => info1.EarthAmount <= info2.EarthAmount
            && info1.FireAmount <= info2.FireAmount
            && info1.WaterAmount <= info2.WaterAmount
            && info1.AirAmount <= info2.AirAmount;

    public static bool operator >=(FusionInfo info1, FusionInfo info2)
        => info1.EarthAmount >= info2.EarthAmount
            && info1.FireAmount >= info2.FireAmount
            && info1.WaterAmount >= info2.WaterAmount
            && info1.AirAmount >= info2.AirAmount;

    public int CompareTo(FusionInfo other)
    {
        if (this <= other)
            return -1;

        return 1;
    }
}

[Serializable]
public struct FusionRecipe
{
    public string entityPrefab;
    public FusionInfo cost;
}

public struct FusionRecipeData
{
    public int PrefabId;
    public FusionInfo Cost;
}

[CreateAssetMenu(menuName = "Rust Vs Goo/Slimes/SlimeMergeGraph", fileName = "SlimeMergeGraph")]
public class SlimeMergeGraph : ScriptableObject
{
    [SerializeField] private List<FusionRecipe> recipes;

    public List<FusionRecipe> GetRecipes()
        => recipes;
    
    private class Baker : Baker<UnitAuthoring>
    {
        public override void Bake(UnitAuthoring authoring)
        {}
    }
}
