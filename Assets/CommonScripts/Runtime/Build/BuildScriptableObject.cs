using UnityEngine;

public class BuildScriptableObject : ScriptableObject
{
    public int Version = 0;
    public int BuildNumber = 0;

    public BuildScriptableObject IncrementBuildNumber()
    {
        BuildNumber++;
        return this;
    }

    public string GetVersionString()
    {
        return $"{Version}.{BuildNumber}";
    }
}
