using UnityEngine;

/// <summary>
/// Decorator stripping policy that ensures at least one product is stripped.
/// </summary>
public sealed class EnsureAtLeastOneStrippedPolicy : IStrippingPolicy
{
    private readonly IStrippingPolicy _inner;

    public EnsureAtLeastOneStrippedPolicy(IStrippingPolicy inner)
    {
        Debug.Assert(inner != null, "Inner policy cannot be null");
        _inner = inner;
    }

    public bool CanStrip(Product product, IStrippingContext context)
    {
        if (context is not BasicStrippingContext basic)
        {
            return _inner.CanStrip(product, context);
        }

        // If we're at the last candidate and nothing has been stripped yet, force success.
        if (basic.RemainingCandidates <= 1 && basic.StrippedCount == 0 && basic.IsCandidate(product))
        {
            return true;
        }

        return _inner.CanStrip(product, context);
    }

    public void UpdateContext(Product product, bool stripped, IStrippingContext context)
    {
        _inner.UpdateContext(product, stripped, context);

        if (context is BasicStrippingContext basic)
        {
            // In case inner policy doesn't track, ensure the base context is always updated.
            // (Idempotency: BasicStrippingContext increments counts, so only call this if inner doesn't)
            // Convention: BasicStrippingContext should be updated exactly once per candidate;
            // ProbabilityBasedStrippingPolicy already does it, so keep this disabled.
            // basic.RegisterDecision(product, stripped);
        }
    }
}