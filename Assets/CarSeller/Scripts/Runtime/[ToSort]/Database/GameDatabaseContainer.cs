using NorskaLib.Spreadsheets;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

//TODO check the database for missing levels, not found identifiers, etc. and log warnings at load time

[Serializable]
public class AreaBalancingContent
{
    public List<AreaCollection> AreaBalancingByAreaName;

    [SpreadsheetPage("Areas")]
    public List<AreasDefinition> AreasDefinitions;
    [SpreadsheetPage("Area1")]
    public List<LevelBalancing> Area1;
    [SpreadsheetPage("Area2")]
    public List<LevelBalancing> Area2;
    [SpreadsheetPage("Area3")]
    public List<LevelBalancing> Area3;

    [SpreadsheetPage("CarTypes")]
    public List<CarTypeConfig> CarTypeConfigs;
    [SpreadsheetPage("CarRarities")]
    public List<CarRarityConfig> CarRarityConfigs;

    [SpreadsheetPage("Global")]
    public GlobalConfig GlobalConfig;


    static Dictionary<string, FieldInfo> LevelListFieldByAreaId;

    public void RebuildAreaBalancingByAreaName()
    {
        if (AreaBalancingByAreaName == null)
        {
            AreaBalancingByAreaName = new List<AreaCollection>();
        }
        else
        {
            AreaBalancingByAreaName.Clear();
        }

        if (AreasDefinitions == null || AreasDefinitions.Count == 0)
        {
            Debug.LogWarning($"{nameof(AreaBalancingContent)}: {nameof(AreasDefinitions)} is empty; cannot rebuild {nameof(AreaBalancingByAreaName)}.");
            return;
        }

        var fieldByAreaId = GetLevelListFieldByAreaId();

        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < AreasDefinitions.Count; i++)
        {
            var areaId = AreasDefinitions[i].AreaID;

            if (string.IsNullOrEmpty(areaId))
            {
                Debug.LogWarning($"{nameof(AreaBalancingContent)}: Empty {nameof(AreasDefinition.AreaID)} at index {i}.");
                continue;
            }

            if (!seen.Add(areaId))
            {
                Debug.LogWarning($"{nameof(AreaBalancingContent)}: Duplicate area id '{areaId}' in {nameof(AreasDefinitions)}.");
                continue;
            }

            if (!fieldByAreaId.TryGetValue(areaId, out var field))
            {
                Debug.LogWarning($"{nameof(AreaBalancingContent)}: No level list field found for area id '{areaId}'. Expected a field named '{areaId}' (e.g. {nameof(Area1)}).");
                continue;
            }

            var levels = field.GetValue(this) as List<LevelBalancing> ?? new List<LevelBalancing>();

            AreaBalancingByAreaName.Add(new AreaCollection
            {
                Id = areaId,
                DisplayName = AreasDefinitions[i].DisplayName,
                Levels = levels
            });
        }
    }

    static Dictionary<string, FieldInfo> GetLevelListFieldByAreaId()
    {
        if (LevelListFieldByAreaId != null)
        {
            return LevelListFieldByAreaId;
        }

        var dict = new Dictionary<string, FieldInfo>(StringComparer.Ordinal);

        var fields = typeof(AreaBalancingContent).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            if (field.FieldType == typeof(List<LevelBalancing>))
            {
                dict[field.Name] = field;
            }
        }

        LevelListFieldByAreaId = dict;
        return LevelListFieldByAreaId;
    }
}

[Serializable]
public class AreaCollection
{
    public string Id;
    public string DisplayName;
    public List<LevelBalancing> Levels;
}

[Serializable]
public struct AreasDefinition
{
    public string AreaID;
    public string DisplayName;
}

[Serializable]
public struct LevelBalancing
{
    public int AreaLevel;

    public float Small;
    public float Sedan;
    public float Truck;
    public float Bike;
    public float Super;

    public float Common;
    public float Uncommon;
    public float Epic;

    public float RequiredXP;
    public float IncomeMultiplier;
    public int BuyerCount;

    public float BuyerSmall;
    public float BuyerSedan;
    public float BuyerTruck;
    public float BuyerBike;
    public float BuyerSuper;
    public float BuyerAny;


    public CarSpawnWeight[] CalculateCarSpawnWeightsForLevel()
    {
        var result = new[]
        {
            new CarSpawnWeight(new CarKind(CarType.Small,CarRarity.Common) , Small * Common),
            new CarSpawnWeight(new CarKind(CarType.Sedan,CarRarity.Common) , Sedan * Common),
            new CarSpawnWeight(new CarKind(CarType.Truck,CarRarity.Common) , Truck * Common),
            new CarSpawnWeight(new CarKind(CarType.Bike,CarRarity.Common) , Bike * Common),
            new CarSpawnWeight(new CarKind(CarType.Super,CarRarity.Common) , Super * Common),

            new CarSpawnWeight(new CarKind(CarType.Small,CarRarity.Uncommon) , Small * Uncommon),
            new CarSpawnWeight(new CarKind(CarType.Sedan,CarRarity.Uncommon) , Sedan * Uncommon),
            new CarSpawnWeight(new CarKind(CarType.Truck,CarRarity.Uncommon) , Truck * Uncommon),
            new CarSpawnWeight(new CarKind(CarType.Bike,CarRarity.Uncommon) , Bike * Uncommon),
            new CarSpawnWeight(new CarKind(CarType.Super,CarRarity.Uncommon) , Super * Uncommon),

            new CarSpawnWeight(new CarKind(CarType.Small,CarRarity.Epic) , Small * Epic),
            new CarSpawnWeight(new CarKind(CarType.Sedan,CarRarity.Epic) , Sedan * Epic),
            new CarSpawnWeight(new CarKind(CarType.Truck,CarRarity.Epic) , Truck * Epic),
            new CarSpawnWeight(new CarKind(CarType.Bike,CarRarity.Epic) , Bike * Epic),
            new CarSpawnWeight(new CarKind(CarType.Super,CarRarity.Epic) , Super * Epic),
        };
        return result;
    }

    public BuyerSpawnSpawnWeight[] CalculateBuyerSpawnWeightsForLevel()
    {
        var result = new[]
        {
            new BuyerSpawnSpawnWeight(CarType.Small, BuyerSmall),
            new BuyerSpawnSpawnWeight(CarType.Sedan, BuyerSedan),
            new BuyerSpawnSpawnWeight(CarType.Truck, BuyerTruck),
            new BuyerSpawnSpawnWeight(CarType.Bike, BuyerBike),
            new BuyerSpawnSpawnWeight(CarType.Super, BuyerSuper),
            new BuyerSpawnSpawnWeight(CarType.Any, BuyerAny), // convention: use Sedan for "any" type since it's the most common
        };
        return result;
    }

}

[CreateAssetMenu(fileName = "GameDatabaseContainer", menuName = "CarSeller/GameDatabaseContainer")]
public class GameDatabaseContainer : SpreadsheetsContainerBase
{
    [SpreadsheetContent]
    [SerializeField]
    AreaBalancingContent AreaBalancingContent;
    public AreaBalancingContent Balancing => AreaBalancingContent;

    void OnEnable()
    {
        AreaBalancingContent?.RebuildAreaBalancingByAreaName();
    }

    void OnValidate()
    {
        AreaBalancingContent?.RebuildAreaBalancingByAreaName();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            // Optional (more aggressive): UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
    }
}

public struct CarSpawnWeight
{
    public CarKind CarKind;
    public float Weight;

    public CarSpawnWeight(CarKind carKind, float weight)
    {
        this.CarKind = carKind;
        this.Weight = weight;
    }
}

public struct BuyerSpawnSpawnWeight
{
    public CarType CarType;
    public float Weight;

    public BuyerSpawnSpawnWeight(CarType carType, float weight)
    {
        this.CarType = carType;
        this.Weight = weight;
    }
}


[Serializable]
public struct CarTypeConfig
{
    public string Type;
    public CarType GetType()
    {
        if(Enum.TryParse(Type, ignoreCase: true, out CarType result))
        {
            return result;
        }
        Debug.LogError($"Failed to parse CarType from string '{Type}'. Defaulting to Sedan.");
        return CarType.Sedan;
    }
    public float BaseValue;
    public float SpecificValue;
}

[Serializable]
public struct CarRarityConfig
{
    public string Rarity;
    public CarRarity GetRarity()
    {
        if (Enum.TryParse(Rarity, ignoreCase: true, out CarRarity result))
        {
            return result;
        }
        Debug.LogError($"Failed to parse CarRarity from string '{Rarity}'. Defaulting to Common.");
        return CarRarity.Common;
    }
    public float ValueMultiplier;
}

[Serializable]
public struct GlobalConfig
{
    public float DisplayValueMultiplier;
}