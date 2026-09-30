//Version: 6
//increment version each change
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Ivan's handy lifehack methods for every life situation
/// ...and some comments
/// </summary>
public static class CommonTools
{
    public static Transform[] AllChildren(Transform transform)
    {
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i));
        }
        return children.ToArray();
    }


    public static float truncateFloat(float orig, float truncate_value = 100f)
    {
        return Mathf.Round(orig * truncate_value) / truncate_value;
    }

    public static bool HitsContainsThisTransform(Transform transform, RaycastHit[] hits, out RaycastHit hit)
    {
        hit = System.Array.Find(hits, obj => obj.transform == transform);
        return hit.transform != null;
    }

    public static Vector3 yPlaneVector(Vector3 vector)
    {
        return new Vector3(vector.x, 0f, vector.z);
    }

    public static Vector3 yPlaneVector(Vector3 vector, float y)
    {
        return new Vector3(vector.x, y, vector.z);
    }

    public static Vector3 zPlaneVector(Vector3 vector)
    {
        return new Vector3(vector.x, vector.y, 0f);
    }
    public static Vector3 zPlaneVector(Vector3 vector, float z)
    {
        return new Vector3(vector.x, vector.y, z);
    }

    public static Vector3 MaxVector(Vector3 a, Vector3 b)
    {
        if (a.magnitude >= b.magnitude)
            return a;
        return b;
    }

    public static float linearEquation(float x, float k, float b)
    {
        return x * k + b;
    }

    public static float quadraticEquation(float x, float a, float b, float c)
    {
        return x * x * a + x * b + x * c;
    }

    public static Transform[] getChildren(Transform parent)
    {
        int count = parent.childCount;
        Transform[] children = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            children[i] = parent.GetChild(i);
        }
        return children;
    }

    public static Vector3 GetPerpendicularPointFromPointToLine(Vector3 point, Vector3 lineA, Vector3 lineB)
    {
        Vector3 AB = lineB - lineA;
        Vector3 BC = point - lineB;
        float dotB = Vector3.Dot(AB, BC);
        return lineB + AB.normalized * (dotB / AB.magnitude);
    }

    public static float getCurrentAngleRadians(Vector2 direction)
    {
        return Vector2.SignedAngle(Vector3.right, direction) * Mathf.PI * 0.00555f;
    }

    //public static Vector2 GetPerpendicularPointFromPointToLine(Vector2 point, Vector2 lineA, Vector2 lineB)
    //{
    //    Vector2 AB = lineB - lineA;
    //    Vector2 BC = point - lineB;
    //    float dotB = Vector2.Dot(AB, BC);
    //    return lineB + AB.normalized * (dotB / AB.magnitude);
    //}


    public static int RandomIndex(int[] weights, int weightsSum)
    {
        int val = Random.Range(0, weightsSum);
        int j = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            j += weights[i];
            if (val < j)
                return i;
        }
        throw new System.Exception("WeightsSum is not equal to the sum of weights");
    }

    public static int RandomIndex(int[] weights)
    {
        int sum = 0;
        foreach (var weight in weights)
        {
            sum += weight;
        }
        return RandomIndex(weights, sum);
    }

    public static T RandomObject<T>(this T[] elements)
    {
        return elements[Random.Range(0, elements.Length)];
    }

    public static T RandomObject<T>(this T[] elements, int[] weights)
    {
        if (weights.Length != elements.Length)
            throw new System.Exception("weights count doesn't match elements count");
        return elements[RandomIndex(weights)];
    }

    public static T RandomObject<T>(this T[] elements, float[] weights)
    {
        if (weights.Length != elements.Length)
            throw new System.Exception("weights count doesn't match elements count");
        return elements[RandomIndex(weights)];
    }


    public static int RandomIndex(float[] weights, float weightsSum)
    {
        if (weightsSum <= 0f)
            throw new System.Exception("WeightsSum must be positive");

        float val = Random.Range(0f, weightsSum);
        float cumulative = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (val < cumulative)
                return i;
        }

        // Fallback: if due to float precision we didn't return inside the loop,
        // but the sum is positive, return the last index instead of throwing.
        return weights.Length - 1;
    }

    public static int RandomIndex(float[] weights)
    {
        float sum = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            sum += weights[i];
        }

        if (sum <= 0f)
            throw new System.Exception("Sum of weights must be positive");

        return RandomIndex(weights, sum);
    }

    public static Vector2 RotateDir(Vector2 dir, float angle)
    {
        float cosA = Mathf.Cos(Mathf.Deg2Rad * angle);
        float sinA = Mathf.Sin(Mathf.Deg2Rad * angle);
        return new Vector2(dir.x * cosA - dir.y * sinA, sinA * dir.x + cosA * dir.y);
    }

    public static float crossProduct(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="threshold"> percentage in values from 0 to 1</param>
    /// <returns></returns>
    public static bool relativelyDifferent(float a, float b, float threshold)
    {
        return (Mathf.Abs(a - b) / Mathf.Max(Mathf.Abs(a), Mathf.Abs(b)) > threshold);
    }

    //TODO: improve by finding out b and a:
    /// <summary>
    /// returns smoothed value
    /// </summary>
    /// <param name="x"></param>
    /// <param name="a">The bigger a means the severer curve, especially by 0,5</param>
    /// <returns></returns>
    public static float ClampAtan01(float x, float a)
    {
        return Mathf.Atan((x - 0.5f) * a) * 0.5f / Mathf.Atan(0.5f * a) + 0.5f;
    }

    //TODO: improve by finding out b and a:
    /// <summary>
    /// returns smoothed value
    /// </summary>
    /// <param name="x"></param>
    /// <param name="a">The bigger a means the severer curve, especially by x (+-)-> 0,5</param>
    /// <returns></returns>
    public static float ClampAtan10(float x, float a)
    {
        return 0.5f - Mathf.Atan((x - 0.5f) * a) * 0.5f / Mathf.Atan(0.5f * a);
    }

    //TODO not perfect, use ClampRepeat in combination
    public static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    public static float ClampRepeat(float x, float xMin, float xMax)
    {
        if (x > xMax)
        {
            float span = xMax - xMin;
            x = x - span * Mathf.Floor((x - xMin) / span);
        }
        else if (x < xMin)
        {
            float span = xMax - xMin;
            x = x + span * Mathf.Floor((xMax - x) / span);
        }
        return x;
    }

    //Copy-past this template into your script
    // TO PROVE: NEVER  use static coroutines, because they can't nest 
    static IEnumerator a_to_b_Animation(bool to_b, float speed)
    {
        float n = 0f;
        float result = 0f;
        while (n < 1f)
        {
            n = Mathf.Min(1f, Time.deltaTime * speed + n);
            // Gradually : (int)((to_b ? n : 1f - n) * iterationsCount) / (float)iterationsCount;
            result = (to_b ? n : 1f - n);
            yield return new WaitForEndOfFrame();
        }
    }

    //Changable a_to_b:
    static IEnumerator changable_a_to_b_Animation(bool to_a, float speed)
    {
        // TODO: make to_b instead of to_a
        float n = 0f;
        float result = 0f;
        while ((to_a && n < 1f) || (!to_a && n > 0f))
        {
            if (to_a)
                n = Mathf.Min(1f, n + Time.deltaTime * speed);
            else
                n = Mathf.Max(0f, n - Time.deltaTime * speed);
            result = 1f - n;
            yield return new WaitForEndOfFrame();
        }
    }

    //Rotation around an axis example: 
    //tumblerModelTransform.localRotation = defaultRotation * Quaternion.AngleAxis((rotationAngle *(1f - n)), rotationAxis);
}

public class CooldownTimersManager<T>
{
    Dictionary<T, SimpleTimer> timers;

    public CooldownTimersManager()
    {
        this.timers = new Dictionary<T, SimpleTimer>();
    }
    public void resetAll()
    {
        foreach (var timer in timers.Values)
        {
            timer.reset();
        }
    }

    public void clearAll()
    {
        foreach (var timer in timers.Values)
        {
            timer.clear();
        }
    }

    public SimpleTimer this[T key]
    {
        get
        {
            SimpleTimer timer;
            if (!timers.TryGetValue(key, out timer))
            {
                timer = new SimpleTimer();
                timers[key] = timer;
            }
            return timer;
        }
        set
        {
            timers[key] = value;
        }
    }
}

public class SimpleTimer
{
    public float currentTime { get; private set; } = 0f;
    public float? timeoutTime { get; set; }

    public SimpleTimer(float timeoutTime)
    {
        Debug.Assert(timeoutTime >= 0f);
        this.timeoutTime = timeoutTime;
    }

    public SimpleTimer()
    {

    }

    public SimpleTimer setOnce(float timeoutTime)
    {
        if (this.timeoutTime == null)
        {
            this.timeoutTime = timeoutTime;
        }
        return this;
    }

    public bool isSet()
    {
        return timeoutTime != null;
    }

    /// <summary>
    /// return true if time is run out
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    public bool tick(float deltaTime)
    {
        currentTime += deltaTime;
        return isTimeout();
    }

    public bool isTimeout()
    {
        if (timeoutTime == null)
            throw new UnityException("Timer has timeout not set");
        return currentTime >= timeoutTime;
    }

    public void reset()
    {
        currentTime = 0f;
    }

    public void clear()
    {
        timeoutTime = null;
        reset();
    }

    public void reset(float timeoutTime)
    {
        currentTime = 0f;
        this.timeoutTime = timeoutTime;
    }
}

public class RecordedValues<T>
{
    Dictionary<T, RecordedValue<T>> values;

    public RecordedValues()
    {
        values = new Dictionary<T, RecordedValue<T>>();
    }

    public RecordedValue<T> this[T key]
    {
        get
        {
            RecordedValue<T> value;
            if (!values.TryGetValue(key, out value))
            {
                value = new RecordedValue<T>();
                values[key] = value;
            }
            return value;
        }
        set
        {
            values[key] = value;
        }
    }
}

public class RecordedValue<T>
{
    private T recordedValue;

    public void recordValue(T value)
    {
        recordedValue = value;
    }

    /// <summary>
    /// returns true if changed
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool valueChanged(T value)
    {
        return !EqualityComparer<T>.Default.Equals(recordedValue, value);
    }
}

//public class TrackedValue<T>
//{
//    private T _value;

//    public T Value
//    {
//        get => _value;
//        set => Set(value);
//    }

//    public bool thisFrameChanged
//    {
//        get; private set;
//    } = false;

//    public event Action<T> OnValueChanged;

//    public TrackedValue(T initialValue = default)
//    {
//        Value = initialValue;
//    }

//    public bool Set(T newValue)
//    {
//        if (_value.Equals(newValue))
//        {
//            thisFrameChanged = false;
//            return false; // No change occurred
//        }

//        _value = newValue;
//        thisFrameChanged = true;
//        OnValueChanged?.Invoke(_value);
//        return true; // Value changed
//    }

//    public static implicit operator T(TrackedValue<T> tracked) => tracked._value;
//    public static implicit operator TrackedValue<T>(T value) => new TrackedValue<T>(value);
//}

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}