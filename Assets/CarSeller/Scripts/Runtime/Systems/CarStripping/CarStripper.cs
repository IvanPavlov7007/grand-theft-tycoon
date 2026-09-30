using System.Collections.Generic;
using UnityEngine;

public class CarStripper
{
    public StrippingProcess Strip(Car car, IStrippingPolicy policy)
    {
        Debug.Assert(car != null, "Car to strip cannot be null");
        Debug.Assert(policy != null, "Stripping policy cannot be null");

        var destroyedParts = new List<Product>();
        var strippedParts = new List<Product>();

        var allCandidates = new List<Product>();

        foreach (var partLocation in car.carParts.Keys)
        {
            if (partLocation.Occupant is Product part)
            {
                allCandidates.Add(part);
            }
        }

        allCandidates.Add(car);
        allCandidates.Shuffle();

        IStrippingContext context = new BasicStrippingContext(allCandidates);

        foreach (var product in allCandidates)
        {
            bool stripped = policy.CanStrip(product, context);

            if (stripped)
            {
                strippedParts.Add(product);
            }
            else
            {
                destroyedParts.Add(product);
            }

            policy.UpdateContext(product, stripped, context);
        }

        return new StrippingProcess(strippedParts, destroyedParts);
    }
}

public sealed class StrippingProcess
{
    public readonly IReadOnlyList<Product> StrippedParts;
    public readonly IReadOnlyList<Product> DestroyedParts;

    internal StrippingProcess(List<Product> strippedParts, List<Product> lostParts)
    {
        StrippedParts = strippedParts;
        DestroyedParts = lostParts;
    }

    public void Strip()
    {
        StripParts(StrippedParts);
        DestroyParts(DestroyedParts);
    }

    private void StripParts(IReadOnlyList<Product> toStrip)
    {
        foreach (var part in toStrip)
        {
            if (!G.ProductLifetimeService.MoveProduct(part, World.Instance.HiddenSpace.GetEmptyLocation()))
            {
                Debug.LogError($"Failed to strip part {part.Name} (ID: {part.Id})");
            }
        }
    }

    private void DestroyParts(IReadOnlyList<Product> toDestroy)
    {
        foreach (var part in toDestroy)
        {
            G.ProductLifetimeService.DestroyProduct(part);
        }
    }
}

/// <summary>
/// Can be used to determine whether a product can be stripped or not.
/// Decorator policies can be used to modify behavior.
/// </summary>
public interface IStrippingPolicy
{
    bool CanStrip(Product product, IStrippingContext context);
    void UpdateContext(Product product, bool stripped, IStrippingContext context);
}

/// <summary>
/// Used in cases when strip depends on previous outcomes etc
/// </summary>
public interface IStrippingContext
{
}

public sealed class BasicStrippingContext : IStrippingContext
{
    private readonly HashSet<Product> _candidates;

    public int TotalCandidates { get; }
    public int CurrentIndex { get; private set; }
    public int StrippedCount { get; private set; }

    public int RemainingCandidates => TotalCandidates - CurrentIndex;

    public BasicStrippingContext(IReadOnlyList<Product> candidates)
    {
        Debug.Assert(candidates != null, "Candidates cannot be null.");

        _candidates = new HashSet<Product>(candidates);
        TotalCandidates = candidates.Count;
        CurrentIndex = 0;
        StrippedCount = 0;
    }

    public bool IsCandidate(Product product) => product != null && _candidates.Contains(product);

    public void RegisterDecision(Product product, bool stripped)
    {
        if (stripped)
        {
            StrippedCount++;
        }

        CurrentIndex++;
    }
}