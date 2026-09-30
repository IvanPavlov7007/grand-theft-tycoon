using UnityEngine;

public class ProbabilityBasedStrippingPolicy : IStrippingPolicy
{
    private readonly float successProbability;

    private ProbabilityBasedStrippingPolicy(float successProbability)
    {
        this.successProbability = successProbability;
    }

    public static ProbabilityBasedStrippingPolicy Create01(float successProbability)
    {
        Debug.Assert(successProbability >= 0f && successProbability <= 1f,
            "Success probability must be between 0 and 1.");
        return new ProbabilityBasedStrippingPolicy(successProbability);
    }

    public bool CanStrip(Product product, IStrippingContext context)
    {
        float roll = Random.Range(0f, 1f);
        return roll <= successProbability;
    }

    public void UpdateContext(Product product, bool stripped, IStrippingContext context)
    {
        if (context is BasicStrippingContext basic)
        {
            basic.RegisterDecision(product, stripped);
        }
    }
}