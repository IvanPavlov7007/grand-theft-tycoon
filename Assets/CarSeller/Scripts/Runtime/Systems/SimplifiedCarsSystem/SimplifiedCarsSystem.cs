//Map (Type,Color,Rarity)
//Data base that allows to easily create cars
//Some identificaitons?

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplifiedCarsManager
{
    CarsDefinitionLibrary CreationBuilder => G.SimplifiedCarsCreationBuilder;

    public Car CreateCar(CarKind identifier, ILocation location)
    {
        var car = G.ProductManager.CreateCar(CreationBuilder.RuntimeConfigs[identifier].ToArray().RandomObject(), location);
        return car;
    }

    public Car CreateCar(string identifier, ILocation location)
    {
        var car = G.ProductManager.CreateCar(CreationBuilder.RuntimeConfigsById[identifier], location);
        return car;
    }

    public Car CreateCarHidden(CarKind identifier)
    {
        var car = CreateCar(identifier, World.Instance.HiddenSpace.GetEmptyLocation());
        return car;
    }

    public Car CreateCarHidden(string identifier)
    {
        var car = CreateCar(identifier, World.Instance.HiddenSpace.GetEmptyLocation());
        return car;
    }

    public List<Car> CreatePooledCarList(CarSpawnWeight[] spawnWeights, float weightMultiplicator = 1f)
    {
        var result = new List<Car>();
        foreach (var spawnWeight in spawnWeights)
        {
            if(CreationBuilder.RuntimeConfigs.TryGetValue(spawnWeight.CarKind, out var configs))
            {
                if(configs.Count == 0)
                {
                    Debug.LogWarning($"No config found for car kind {spawnWeight.CarKind}");
                    continue;
                }
            }
            else
            {
                Debug.LogWarning($"No config found for car kind {spawnWeight.CarKind}");
                continue;
            }
            var weight = spawnWeight.Weight * weightMultiplicator;
            var count = Mathf.FloorToInt(weight);
            for (int i = 0; i < count; i++)
            {
                result.Add(CreateCarHidden(spawnWeight.CarKind));
            }
        }
        return result;
    }
}

public interface IReadOnlyDuplicatesIdentifiedCarList<T> : IReadOnlyDictionary<CarKind, List<T>>
{
}

public interface IReadOnlyNonDuplicatesIdentifiedCarList<T> : IReadOnlyDictionary<CarKind, T>
{
}

public sealed class DuplicatesIdentifiedCarList<T> : IDictionary<CarKind, List<T>>, IReadOnlyDuplicatesIdentifiedCarList<T>
{
    private readonly Dictionary<CarKind, List<T>> _inner;

    public DuplicatesIdentifiedCarList()
        : this(new Dictionary<CarKind, List<T>>())
    {
    }

    public DuplicatesIdentifiedCarList(IEqualityComparer<CarKind> comparer)
        : this(new Dictionary<CarKind, List<T>>(comparer))
    {
    }

    public DuplicatesIdentifiedCarList(IDictionary<CarKind, List<T>> dictionary)
        : this(new Dictionary<CarKind, List<T>>(dictionary))
    {
    }

    public DuplicatesIdentifiedCarList(IReadOnlyDictionary<CarKind, List<T>> dictionary)
        : this(new Dictionary<CarKind, List<T>>(dictionary))
    {
    }

    private DuplicatesIdentifiedCarList(Dictionary<CarKind, List<T>> inner)
    {
        _inner = inner;
    }

    public List<T> this[CarKind key]
    {
        get => _inner[key];
        set => _inner[key] = value;
    }

    public ICollection<CarKind> Keys => _inner.Keys;

    IEnumerable<CarKind> IReadOnlyDictionary<CarKind, List<T>>.Keys => _inner.Keys;

    public ICollection<List<T>> Values => _inner.Values;

    IEnumerable<List<T>> IReadOnlyDictionary<CarKind, List<T>>.Values => _inner.Values;

    public int Count => _inner.Count;

    public bool IsReadOnly => ((IDictionary<CarKind, List<T>>)_inner).IsReadOnly;

    public void Add(CarKind key, List<T> value) => _inner.Add(key, value);

    public void Add(KeyValuePair<CarKind, List<T>> item)
        => ((IDictionary<CarKind, List<T>>)_inner).Add(item);

    public void Clear() => _inner.Clear();

    public bool Contains(KeyValuePair<CarKind, List<T>> item)
        => ((IDictionary<CarKind, List<T>>)_inner).Contains(item);

    public bool ContainsKey(CarKind key) => _inner.ContainsKey(key);

    public void CopyTo(KeyValuePair<CarKind, List<T>>[] array, int arrayIndex)
        => ((IDictionary<CarKind, List<T>>)_inner).CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<CarKind, List<T>>> GetEnumerator() => _inner.GetEnumerator();

    public bool Remove(CarKind key) => _inner.Remove(key);

    public bool Remove(KeyValuePair<CarKind, List<T>> item)
        => ((IDictionary<CarKind, List<T>>)_inner).Remove(item);

    public bool TryGetValue(CarKind key, out List<T> value) => _inner.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_inner).GetEnumerator();
}

public sealed class NonDuplicatesIdentifiedCarList<T> : IDictionary<CarKind, T>, IReadOnlyNonDuplicatesIdentifiedCarList<T>
{
    private readonly Dictionary<CarKind, T> _inner;

    public NonDuplicatesIdentifiedCarList()
        : this(new Dictionary<CarKind, T>())
    {
    }

    public NonDuplicatesIdentifiedCarList(IEqualityComparer<CarKind> comparer)
        : this(new Dictionary<CarKind, T>(comparer))
    {
    }

    public NonDuplicatesIdentifiedCarList(IDictionary<CarKind, T> dictionary)
        : this(new Dictionary<CarKind, T>(dictionary))
    {
    }

    public NonDuplicatesIdentifiedCarList(IReadOnlyDictionary<CarKind, T> dictionary)
        : this(new Dictionary<CarKind, T>(dictionary))
    {
    }

    private NonDuplicatesIdentifiedCarList(Dictionary<CarKind, T> inner)
    {
        _inner = inner;
    }

    public T this[CarKind key]
    {
        get => _inner[key];
        set => _inner[key] = value;
    }

    public ICollection<CarKind> Keys => _inner.Keys;

    IEnumerable<CarKind> IReadOnlyDictionary<CarKind, T>.Keys => _inner.Keys;

    public ICollection<T> Values => _inner.Values;

    IEnumerable<T> IReadOnlyDictionary<CarKind, T>.Values => _inner.Values;

    public int Count => _inner.Count;

    public bool IsReadOnly => ((IDictionary<CarKind, T>)_inner).IsReadOnly;

    public void Add(CarKind key, T value) => _inner.Add(key, value);

    public void Add(KeyValuePair<CarKind, T> item)
        => ((IDictionary<CarKind, T>)_inner).Add(item);

    public void Clear() => _inner.Clear();

    public bool Contains(KeyValuePair<CarKind, T> item)
        => ((IDictionary<CarKind, T>)_inner).Contains(item);

    public bool ContainsKey(CarKind key) => _inner.ContainsKey(key);

    public void CopyTo(KeyValuePair<CarKind, T>[] array, int arrayIndex)
        => ((IDictionary<CarKind, T>)_inner).CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<CarKind, T>> GetEnumerator() => _inner.GetEnumerator();

    public bool Remove(CarKind key) => _inner.Remove(key);

    public bool Remove(KeyValuePair<CarKind, T> item)
        => ((IDictionary<CarKind, T>)_inner).Remove(item);

    public bool TryGetValue(CarKind key, out T value) => _inner.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_inner).GetEnumerator();
}