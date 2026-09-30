using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

[CreateAssetMenu(fileName = "CarSpawnConfig", menuName = "Configs/Economy/CarSpawnConfig")]
public class CarSpawnConfig : ScriptableObject
{
    public int CarsToSpawnMaxCount = 15;
    public int CarsToHaveMinCount = 5;
    public RegionalCarSpawnEntry[] RegionalCarSpawnEntries;

    public CarSpawnEntry GetWeightedRandomCarForRegion(string regionId)
    {
        var regionalEntry = Array.Find(RegionalCarSpawnEntries, entry => entry.RegionName == regionId);
        var weights = regionalEntry.CarSpawnEntries.Select(entry => entry.Weight).ToArray();

        return CommonTools.RandomObject(regionalEntry.CarSpawnEntries, weights);
    }

    [System.Serializable]
    public class CarSpawnEntry
    {
        public float Weight = 0.1f;
        public CarBaseConfig CarBaseConfig;
        public CarVariantConfig CarVariantConfig;
    }

    [System.Serializable]
    public class RegionalCarSpawnEntry
    {
        public string RegionName;
        public CarSpawnEntry[] CarSpawnEntries;

        [ShowInInspector, ReadOnly, PropertyOrder(-1), LabelText("Total Weight")]
        private float TotalWeight
        {
            get
            {
                if (CarSpawnEntries == null)
                    return 0f;

                float sum = 0f;
                for (int i = 0; i < CarSpawnEntries.Length; i++)
                {
                    var entry = CarSpawnEntries[i];
                    if (entry != null)
                        sum += entry.Weight;
                }

                return sum;
            }
        }
    }
}