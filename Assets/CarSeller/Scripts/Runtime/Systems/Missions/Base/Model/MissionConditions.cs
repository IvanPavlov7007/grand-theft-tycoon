using System;
using UnityEngine;

[Serializable]
public class ReachTargetCondition : MissionCondition
{
    // TODO instead of coupling by marker ID, consider creating a specific config for target,
    // that can be use to link target runtime between condition and effect.
    public CityMarkerRef target = new CityMarkerRef();

    public override MissionConditionRuntime CreateRuntime(MissionRuntime missionRuntime)
    {
        if (target == null || !target.IsValid)
        {
            Debug.LogError($"ReachTargetCondition: Target marker is not set or invalid in mission '{missionRuntime.Config.MissionId}'");
        }
        if (!G.City.TryGetMarker(target.MarkerId, out var targetRuntime))
        {
            Debug.LogError($"ReachTargetCondition: Target marker with ID '{target.MarkerId}' not found in city for mission '{missionRuntime.Config.MissionId}'");
        }
        return new ReachTargetConditionRuntime(targetRuntime, missionRuntime);
    }

    public class ReachTargetConditionRuntime : MissionConditionRuntime
    {
        public City.CityMarker target;
        bool isSatisfied = false;

        public ReachTargetConditionRuntime(City.CityMarker target, MissionRuntime missionRuntime)
            : base(missionRuntime)
        {
            this.target = target;
        }
        public override void OnEvent(GameEventData data)
        {
            if (data is CityTargetReachedEventData e &&
                hasAspect(e.ReachedObject, out MarkerReferenceAspect aspect)
                && aspect.CityMarker == target)
            {
                isSatisfied = true;
            }
        }

        private bool hasAspect(CityEntity reachedEntity, out MarkerReferenceAspect aspect)
        {
            aspect = null;
            var aspects = reachedEntity.GetAspects<MarkerReferenceAspect>();
            if (aspects.Length == 0)
                return false;
            aspect = aspects[0];
            return true;
        }

        public override bool IsSatisfied()
        {
            return isSatisfied;
        }

        public override void Reset()
        {
            isSatisfied = false;
        }
    }
}

[Serializable]
public class MissionLauncherAcceptedCondition : MissionCondition
{
    // TODO check TODO in ReachTargetCondition for similar pattern, consider unifying
    public override MissionConditionRuntime CreateRuntime(MissionRuntime missionRuntime)
    {
        return new MissionLauncherAcceptedConditionRuntime(missionRuntime);
    }
    public class MissionLauncherAcceptedConditionRuntime : MissionConditionRuntime
    {
        private bool isSatisfied = false;
        public MissionLauncherAcceptedConditionRuntime(MissionRuntime missionRuntime)
            : base(missionRuntime)
        {
        }
        public override void OnEvent(GameEventData data)
        {
            if (data is PlayerAcceptedEventData e &&
                e.acceptedMission == missionRuntime)
            {
                isSatisfied = true;
            }
        }
        public override bool IsSatisfied()
        {
            return isSatisfied;
        }

        public override void Reset()
        {
            isSatisfied = false;
        }
    }
}

[Serializable]
public class TimeElapsedCondition : MissionCondition
{
    public float requiredTimeSeconds;
    public override MissionConditionRuntime CreateRuntime(MissionRuntime missionRuntime)
    {
        return new TimeElapsedConditionRuntime(requiredTimeSeconds, missionRuntime);
    }
    public class TimeElapsedConditionRuntime : MissionConditionRuntime, IExplainable
    {
        private float requiredTimeSeconds;
        private float elapsedTime = 0f;
        public TimeElapsedConditionRuntime(float requiredTimeSeconds, MissionRuntime missionRuntime)
            : base(missionRuntime)
        {
            this.requiredTimeSeconds = requiredTimeSeconds;
        }
        public override void OnEvent(GameEventData data)
        {
            if (data is TimePassEvent e)
            {
                elapsedTime += e.DeltaTime;
            }
        }
        public override bool IsSatisfied()
        {
            return elapsedTime >= requiredTimeSeconds;
        }

        public override void Reset()
        {
            elapsedTime = 0f;
        }

        public string GetExplanation()
        {
            return "Time has run out";
        }
    }
}
internal class TimePassEvent : GameEventData
{
    public float DeltaTime;

    public TimePassEvent(float deltaTime)
    {
        DeltaTime = deltaTime;
    }
}

public class BustedCondition : MissionCondition
{
    public override MissionConditionRuntime CreateRuntime(MissionRuntime missionRuntime)
    {
        return new BustedConditionRuntime(missionRuntime);
    }

    public class BustedConditionRuntime : MissionConditionRuntime, IExplainable
    {
        private bool isBusted = false;
        public BustedConditionRuntime(MissionRuntime missionRuntime)
            : base(missionRuntime)
        {
        }
        public override void OnEvent(GameEventData data)
        {
            if (data is PlayerBustedEventData)
            {
                isBusted = true;
            }
        }
        public override bool IsSatisfied()
        {
            return isBusted;
        }

        public override void Reset()
        {
            isBusted = false;
        }

        public string GetExplanation()
        {
            return "Player got busted by cops";
        }
    }
}