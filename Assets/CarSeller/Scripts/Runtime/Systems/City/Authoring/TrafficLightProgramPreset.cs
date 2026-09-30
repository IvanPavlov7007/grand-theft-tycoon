using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TrafficLightProgramPreset", menuName = "City/Traffic Light Program Preset")]
public sealed class TrafficLightProgramPreset : ScriptableObject
{
    [Serializable]
    public sealed class ProgramStep
    {
        [MinValue(0.1f)]
        public float DurationSeconds = 5f;

        [LabelText("Go Edges")]
        [ValueDropdown("@$root.GetEdgeKeys()")]
        [ListDrawerSettings(ShowFoldout = false)]
        public List<string> GoEdgeKeys = new List<string>();
    }

    [MinValue(1)]
    public int EdgeCount = 4;

    [MinValue(0f)]
    public float PreparationTimeSeconds = 0.75f;

    [ListDrawerSettings(ShowFoldout = false)]
    public List<ProgramStep> Program = new List<ProgramStep>();

    public IEnumerable<string> GetEdgeKeys()
    {
        for (int i = 0; i < Mathf.Max(0, EdgeCount); i++)
            yield return IndexToKey(i);
    }

    public static string IndexToKey(int index)
    {
        const int alphabet = 26;
        index = Mathf.Max(0, index);

        var s = string.Empty;
        do
        {
            int r = index % alphabet;
            s = (char)('a' + r) + s;
            index = (index / alphabet) - 1;
        } while (index >= 0);

        return s;
    }
}