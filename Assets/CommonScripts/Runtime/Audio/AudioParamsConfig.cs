using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[System.Serializable]
public sealed class AudioParamsConfig : Dictionary<string, SerializedAudio>
{
}

[System.Serializable]
public class SerializedAudio
{
    public bool useClips = false;
    [ShowIf("useClips")]
    public AudioClip[] clips;
    [HideIf("useClips")]
    public string name;
    public float volume = 1.0f;
    [FoldoutGroup("Parameters"),Toggle("Enabled")]
    public AudioParams.Repetition repetition = new AudioParams.Repetition(0.5f);
    [FoldoutGroup("Parameters"), Toggle("Enabled")]
    public AudioParams.PitchVariation pitchVariation = new AudioParams.PitchVariation();
    [FoldoutGroup("Parameters"), Toggle("Enabled")]
    public AudioParams.Randomization randomization = new AudioParams.Randomization();
    [FoldoutGroup("Parameters"), Toggle("Enabled")]
    public AudioParams.Distortion distortion = new AudioParams.Distortion();
}