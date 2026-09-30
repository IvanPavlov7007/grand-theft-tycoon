using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Pixelplacement;
using System;

using Random = UnityEngine.Random;

//Based on Daniel Mullins' one
public class AudioController : Singleton<AudioController>
{
    private List<AudioClip> sfx = new List<AudioClip>();
    private List<AudioClip> loops = new List<AudioClip>();
    private AudioMixer currentAudioMixer;
    private List<AudioSource> ActiveSFXSources
    {
        get
        {
            activeSFX.RemoveAll(x => x == null || ReferenceEquals(x, null));
            return activeSFX;
        }
    }
    private List<AudioSource> activeSFX = new List<AudioSource>();

    private Dictionary<string, float> limitedFrequencySounds = new Dictionary<string, float>();
    private Dictionary<string, int> lastPlayedSounds = new Dictionary<string, int>();

    // Track no-repeat state for SerializedAudio clip lists
    private readonly Dictionary<SerializedAudio, int> _lastPlayedByEntry = new Dictionary<SerializedAudio, int>();

    private float DEFAULT_SPATIAL_BLEND = 0.75f;

    private void Awake()
    {
        foreach (object o in Resources.LoadAll("Audio/SFX"))
        {
            sfx.Add((AudioClip)o);
        }
        foreach (object o in Resources.LoadAll("Audio/Loops"))
        {
            loops.Add((AudioClip)o);
        }

        currentAudioMixer = Resources.Load<AudioMixer>("Audio/AudioMixer");
    }

    public void SetMasterVolume(float volume)
    {
        currentAudioMixer.SetFloat("Volume",getDecibels(volume));
    }

    public float GetMasterVolume()
    {
        float dbs;
        if (currentAudioMixer.GetFloat("Volume", out dbs))
        {
            return getlerpedVolume(dbs);
        }

        throw new UnityException("Audio Mixer doesn't have a Volume exposure value");
    }

    float getDecibels(float t)
    {
        return (Mathf.Log10(Mathf.Clamp(t, 0.0001f, 1f)) * 20f);
    }

    float getlerpedVolume(float dbVolume)
    {
        return Mathf.Pow(10f, dbVolume / 20f);
    }

    public AudioClip GetLoopClip(string loopId)
    {
        return loops.Find(x => x.name.ToLowerInvariant() == loopId.ToLowerInvariant());
    }

    public AudioClip GetAudioClip(string soundId)
    {
        return sfx.Find(x => x.name.ToLowerInvariant() == soundId.ToLowerInvariant());
    }

    private AudioSource CreateAudioSourceForSound(string soundId, Vector3 position, bool looping)
    {
        if (!string.IsNullOrEmpty(soundId))
        {
            AudioClip sound = GetAudioClip(soundId);

            if (sound != null)
            {
                return InstantiateAudioObject(sound, position, looping);
            }
        }

        return null;
    }

    private AudioSource InstantiateAudioObject(AudioClip clip, Vector3 pos, bool looping)
    {
        GameObject tempGO = new GameObject("Audio_" + clip.name);
        tempGO.transform.position = pos;

        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        aSource.clip = clip;
        aSource.outputAudioMixerGroup = currentAudioMixer.FindMatchingGroups(looping? "Music" : "FX")[0];
        aSource.spatialBlend = DEFAULT_SPATIAL_BLEND;

        aSource.Play();
        if (looping)
        {
            aSource.loop = true;
        }
        else
        {
            Destroy(tempGO, clip.length * 3f);
        }
        return aSource;
    }

    public AudioSource PlaySoundFlat(string soundId, float volume = 1f, float skipToTime = 0f, AudioParams.Pitch pitch = null,
        AudioParams.Repetition repetition = null, AudioParams.Randomization randomization = null, AudioParams.Distortion distortion = null, bool looping = false)
    {
        var source = PlaySound3D(soundId, Vector3.zero, volume, skipToTime, pitch, repetition, randomization, distortion, looping);

        if(source != null)
        {
            source.spatialBlend = 0f;
            DontDestroyOnLoad(source.gameObject);
        }
        return source;
    }

    public AudioSource PlaySound3D(string soundId, Vector3 position, float volume = 1f, float skipToTime = 0f, AudioParams.Pitch pitch = null,
        AudioParams.Repetition repetition = null, AudioParams.Randomization randomization = null, AudioParams.Distortion distortion = null, bool looping = false)
    {
        if (repetition != null)
        {
            if (RepetitionIsTooFrequent(soundId, repetition.minRepetitionFrequency, repetition.entryId))
            {
                return null;
            }
        }

        string randomVariationId = soundId;
        if (randomization != null)
        {
            randomVariationId = GetRandomVariationOfSound(soundId, randomization.noRepeating);
        }

        var source = CreateAudioSourceForSound(randomVariationId, position, looping);
        if (source != null)
        {
            source.volume = volume;
            source.time = source.clip.length * skipToTime;

            if (pitch != null)
            {
                source.pitch = pitch.pitch;
            }

            if (distortion != null)
            {
                if (distortion.muffled)
                {
                    MuffleSource(source);
                }
            }

            activeSFX.Add(source);
        }
        return source;
    }

    // New: SerializedAudio overloads to support Editor-assigned clips and parameters
    public AudioSource PlaySoundFlat(SerializedAudio entry, float volume = 1f, float skipToTime = 0f, bool looping = false, AudioParams.Pitch pitch = null)
    {
        var source = PlaySound3D(entry, Vector3.zero, volume, skipToTime, looping, pitch);
        if (source != null)
        {
            source.spatialBlend = 0f;
            DontDestroyOnLoad(source.gameObject);
        }
        return source;
    }

    public AudioSource PlaySound3D(SerializedAudio entry, Vector3 position, float volume = 1f, float skipToTime = 0f, bool looping = false, AudioParams.Pitch pitch = null)
    {
        if (entry == null) return null;

        // Repetition throttling (respect Enabled for SerializedAudio path)
        if (entry.repetition != null && entry.repetition.Enabled)
        {
            var keyBase = !string.IsNullOrEmpty(entry.name) ? entry.name : ("entry_" + entry.GetHashCode());
            var suffix = string.IsNullOrEmpty(entry.repetition.entryId) ? "" : entry.repetition.entryId;
            if (RepetitionIsTooFrequent(keyBase, entry.repetition.minRepetitionFrequency, suffix))
            {
                return null;
            }
        }

        // Determine clip
        AudioClip chosenClip = null;
        string soundIdForLookup = null;

        if (entry.useClips && entry.clips != null && entry.clips.Length > 0)
        {
            if (entry.clips.Length == 1)
            {
                chosenClip = entry.clips[0];
            }
            else
            {
                int idx = Random.Range(0, entry.clips.Length);
                bool noRepeating = entry.randomization != null && entry.randomization.Enabled && entry.randomization.noRepeating;
                if (noRepeating)
                {
                    int lastIdx;
                    if (_lastPlayedByEntry.TryGetValue(entry, out lastIdx))
                    {
                        const int BREAK_OUT_THRESHOLD = 100;
                        int guard = 0;
                        while (idx == lastIdx && guard < BREAK_OUT_THRESHOLD)
                        {
                            idx = Random.Range(0, entry.clips.Length);
                            guard++;
                        }
                    }
                    _lastPlayedByEntry[entry] = idx;
                }
                chosenClip = entry.clips[idx];
            }
        }
        else
        {
            // Name-based path, preserving existing convention and optional anti-repeat
            if (string.IsNullOrEmpty(entry.name)) return null;
            bool noRepeating = entry.randomization != null && entry.randomization.Enabled && entry.randomization.noRepeating;
            soundIdForLookup = noRepeating ? GetRandomVariationOfSound(entry.name, true) : entry.name;
        }

        AudioSource source = null;
        if (chosenClip != null)
        {
            source = InstantiateAudioObject(chosenClip, position, looping);
        }
        else
        {
            source = CreateAudioSourceForSound(soundIdForLookup, position, looping);
        }

        if (source == null) return null;

        source.volume = volume;
        source.time = source.clip.length * skipToTime;

        // Apply pitch: explicit pitch param overrides pitchVariation
        if (pitch != null)
        {
            source.pitch = pitch.pitch;
        }
        else if (entry.pitchVariation != null && entry.pitchVariation.Enabled)
        {
            var p = new AudioParams.Pitch(entry.pitchVariation.variation);
            source.pitch = p.pitch;
        }

        if (entry.distortion != null && entry.distortion.Enabled && entry.distortion.muffled)
        {
            MuffleSource(source);
        }

        activeSFX.Add(source);
        return source;
    }

    private bool RepetitionIsTooFrequent(string soundId, float frequencyMin, string entrySuffix = "")
    {
        float time = Time.unscaledTime;
        string soundKey = soundId + entrySuffix;

        if (limitedFrequencySounds.ContainsKey(soundKey))
        {
            if (time - frequencyMin > limitedFrequencySounds[soundKey])
            {
                limitedFrequencySounds[soundKey] = time;
                return false;
            }
        }
        else
        {
            limitedFrequencySounds.Add(soundKey, time);
            return false;
        }

        return true;
    }

    private string GetRandomVariationOfSound(string soundPrefix, bool noRepeating)
    {
        string soundId = "";

        if (!string.IsNullOrEmpty(soundPrefix))
        {
            List<AudioClip> variations = sfx.FindAll(x => x != null && x.name.ToLowerInvariant().StartsWith(soundPrefix.ToLowerInvariant()));

            if (variations.Count > 0)
            {
                int index = Random.Range(0, variations.Count) + 1;
                if (noRepeating) // repeating in 2 consequent draws, otherwise not controlled
                {
                    if (!lastPlayedSounds.ContainsKey(soundPrefix))
                    {
                        lastPlayedSounds.Add(soundPrefix, index);
                    }
                    else
                    {
                        int breakOutCounter = 0;
                        const int BREAK_OUT_THRESHOLD = 100;
                        while (lastPlayedSounds[soundPrefix] == index && breakOutCounter < BREAK_OUT_THRESHOLD)
                        {
                            index = Random.Range(0, variations.Count) + 1;
                            breakOutCounter++;
                        }

                        if (breakOutCounter >= BREAK_OUT_THRESHOLD - 1)
                        {
                            Debug.Log("Broke out of infinite loop! AudioController.PlayRandomSound.");
                        }

                        lastPlayedSounds[soundPrefix] = index;
                    }
                }

                soundId = soundPrefix + index;
            }
            else
            {
                soundId = soundPrefix;
            }
        }

        return soundId;
    }

    private void MuffleSource(AudioSource source, float cutoff = 300f)
    {
        var filter = source.gameObject.AddComponent<AudioLowPassFilter>();
        filter.cutoffFrequency = cutoff;
    }
}


namespace AudioParams
{
    [Serializable]
    public class BaseParam
    {
        public bool Enabled;
    }

    [System.Serializable]
    public class PitchVariation : BaseParam
    {
        public VariationSize variation;
    }

    [System.Serializable]
    public enum VariationSize
    {
        VerySmall,
        Small,
        Medium,
        Large
    }
    [System.Serializable]
    public class Pitch
    {
       
        public float pitch;

        public Pitch(float value)
        {
            pitch = value;
        }

        public Pitch(VariationSize randomVariation)
        {
            switch (randomVariation)
            {
                case VariationSize.VerySmall:
                    pitch = Random.Range(0.95f, 1.05f);
                    break;
                case VariationSize.Small:
                    pitch = Random.Range(0.9f, 1.1f);
                    break;
                case VariationSize.Medium:
                    pitch = Random.Range(0.75f, 1.25f);
                    break;
                case VariationSize.Large:
                    pitch = Random.Range(0.5f, 1.5f);
                    break;
            }
        }
    }

    [System.Serializable]
    public class Repetition : BaseParam
    {
        public float minRepetitionFrequency;
        public string entryId;

        public Repetition(float minRepetitionFrequency, string entryId = "")
        {
            this.minRepetitionFrequency = minRepetitionFrequency;
            this.entryId = entryId;
        }
    }
    [System.Serializable]
    public class Randomization : BaseParam
    {
        public bool noRepeating;

        public Randomization(bool noRepeating = true)
        {
            this.noRepeating = noRepeating;
        }
    }

    [System.Serializable]
    public class Distortion : BaseParam
    {
        public bool muffled;
    }

}