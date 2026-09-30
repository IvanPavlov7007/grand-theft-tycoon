using System.Collections.Generic;
using UnityEngine;

public sealed class TrafficLightRuntimeController : MonoBehaviour
{
    public string Id { get; private set; }
    public string NodeId { get; private set; }

    private readonly Dictionary<string, string> _keyByEdgeId = new Dictionary<string, string>();
    private readonly List<string> _keys = new List<string>();
    private readonly List<CityGraphAsset.TrafficLightProgramStepData> _program = new List<CityGraphAsset.TrafficLightProgramStepData>();
    private readonly Dictionary<string, TrafficLightState> _statesByKey = new Dictionary<string, TrafficLightState>();

    private TrafficLightViewController _view;

    private int _currentStepIndex;
    private float _timeLeft;
    private float _preparationTimeSeconds;
    private bool _inPreparation;
    private bool _initialized;

    public bool TryGetStateForEdge(RoadEdge edge, out TrafficLightState state)
    {
        if (edge == null)
        {
            state = default;
            return false;
        }

        return TryGetStateForEdgeId(edge.Id, out state);
    }

    public bool TryGetStateForEdgeId(string edgeId, out TrafficLightState state)
    {
        state = default;

        if (string.IsNullOrEmpty(edgeId))
            return false;

        if (!_keyByEdgeId.TryGetValue(edgeId, out var key))
            return false;

        return _statesByKey.TryGetValue(key, out state);
    }

    public bool TryGetStateForKey(string key, out TrafficLightState state)
    {
        state = default;

        if (string.IsNullOrEmpty(key))
            return false;

        return _statesByKey.TryGetValue(key, out state);
    }

    public void Initialize(
        string id,
        string nodeId,
        TrafficLightViewController view,
        IReadOnlyDictionary<string, string> keyByEdgeId,
        IEnumerable<string> allKeys,
        IList<CityGraphAsset.TrafficLightProgramStepData> program,
        float preparationTimeSeconds,
        float initialTimeOffsetSeconds)
    {
        Id = id;
        NodeId = nodeId;

        _view = view;
        _preparationTimeSeconds = Mathf.Max(0f, preparationTimeSeconds);

        _keyByEdgeId.Clear();
        if (keyByEdgeId != null)
        {
            foreach (var kv in keyByEdgeId)
            {
                if (!string.IsNullOrEmpty(kv.Key) && !string.IsNullOrEmpty(kv.Value))
                    _keyByEdgeId[kv.Key] = kv.Value;
            }
        }

        _keys.Clear();
        if (allKeys != null)
        {
            foreach (var k in allKeys)
                _keys.Add(k);
        }

        _program.Clear();
        if (program != null)
        {
            for (int i = 0; i < program.Count; i++)
                _program.Add(program[i]);
        }

        _initialized = true;

        if (_program.Count == 0)
        {
            _currentStepIndex = 0;
            _timeLeft = 0f;
            _inPreparation = false;

            _statesByKey.Clear();
            for (int i = 0; i < _keys.Count; i++)
                _statesByKey[_keys[i]] = TrafficLightState.Stop;

            _view.SetLightState(_statesByKey);
            return;
        }

        // Apply initial offset into the cyclic program.
        float offset = Mathf.Max(0f, initialTimeOffsetSeconds);
        int guard = 0;

        _currentStepIndex = 0;
        while (guard++ < 10000)
        {
            var dur = Mathf.Max(0.01f, _program[_currentStepIndex].DurationSeconds);
            if (offset < dur)
            {
                _timeLeft = Mathf.Max(0.01f, dur - offset);
                break;
            }

            offset -= dur;
            _currentStepIndex = (_currentStepIndex + 1) % _program.Count;
        }

        var currentStep = _program[_currentStepIndex];
        var effectivePrep = Mathf.Min(_preparationTimeSeconds, Mathf.Max(0f, currentStep.DurationSeconds));

        _inPreparation = effectivePrep > 0f && _timeLeft <= effectivePrep;

        if (_inPreparation)
            ApplyPreparationForNext();
        else
            ApplyGoStatesForCurrent();
    }

    private void Update()
    {
        if (!_initialized || _view == null)
            return;

        if (_program.Count == 0)
            return;

        _timeLeft -= Time.deltaTime;

        var currentStep = _program[_currentStepIndex];
        var effectivePrep = Mathf.Min(_preparationTimeSeconds, Mathf.Max(0f, currentStep.DurationSeconds));

        // Yellow window occurs at the end of the current step, before switching to the next step.
        if (!_inPreparation && effectivePrep > 0f && _timeLeft <= effectivePrep)
        {
            ApplyPreparationForNext();
            _inPreparation = true;
        }

        if (_timeLeft > 0f)
            return;

        _currentStepIndex = (_currentStepIndex + 1) % _program.Count;
        _inPreparation = false;

        // Start next step from full duration.
        var step = _program[_currentStepIndex];
        _timeLeft = Mathf.Max(0.01f, step.DurationSeconds);

        ApplyGoStatesForCurrent();
    }

    private void ApplyGoStatesForCurrent()
    {
        var step = _program[_currentStepIndex];
        var goSet = new HashSet<string>(step.GoEdgeKeys ?? System.Array.Empty<string>());

        _statesByKey.Clear();
        for (int i = 0; i < _keys.Count; i++)
        {
            var k = _keys[i];
            _statesByKey[k] = goSet.Contains(k) ? TrafficLightState.Go : TrafficLightState.Stop;
        }

        _view.SetLightState(_statesByKey);
    }

    private void ApplyPreparationForNext()
    {
        // Keep current Go as Go, upcoming Go as Yellow.
        var currentStep = _program[_currentStepIndex];
        var currentGoSet = new HashSet<string>(currentStep.GoEdgeKeys ?? System.Array.Empty<string>());

        var nextStepIndex = (_currentStepIndex + 1) % _program.Count;
        var nextStep = _program[nextStepIndex];
        var nextGoSet = new HashSet<string>(nextStep.GoEdgeKeys ?? System.Array.Empty<string>());

        _statesByKey.Clear();
        for (int i = 0; i < _keys.Count; i++)
        {
            var k = _keys[i];

            if (currentGoSet.Contains(k))
                _statesByKey[k] = TrafficLightState.Go;
            else if (nextGoSet.Contains(k))
                _statesByKey[k] = TrafficLightState.Yellow;
            else
                _statesByKey[k] = TrafficLightState.Stop;
        }

        _view.SetLightState(_statesByKey);
    }
}