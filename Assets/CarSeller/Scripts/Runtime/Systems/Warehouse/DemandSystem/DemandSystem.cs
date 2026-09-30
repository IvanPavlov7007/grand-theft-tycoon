using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public sealed class DemandSystem
{
    IDemandStrategy demandStrategy;
    DemandContext demandContext;

    DemandProcess demandProcess;

    public void Initialize()
    {
        demandProcess = new DemandProcess(this);
    }

    public void Pause()
    {
    }

    public void Continue()
    {

    }

    void onProductPoolChanged()
    {
        //update demands
    }

    void createDemand()
    {
        Debug.Assert(demandStrategy != null, "Demand strategy is not set.");
        Debug.Assert(demandContext != null, "Demand context is not set.");

        demandStrategy.GenerateDemand(demandContext);
    }

    float getNextDemandInterval()
    {
        Debug.Assert(demandStrategy != null, "Demand strategy is not set.");
        Debug.Assert(demandContext != null, "Demand context is not set.");

        return demandStrategy.NextDemandInterval(demandContext);
    }

    sealed class DemandProcess : RoutinedObject
    {
        DemandSystem demandSystem;
        internal bool paused = false;
        internal float nextDemandTime = 0f;

        public DemandProcess(DemandSystem demandSystem)
        {
            this.demandSystem = demandSystem;
        }

        public void StartProcess()
        {
            TryStartRoutine(process());
        }

        IEnumerator process()
        {
            while (true)
            {
                Update();
                yield return null;
            }
        }

        private void Update()
        {
            if(paused)
                return;
            nextDemandTime -= Time.deltaTime;
            if (nextDemandTime <= 0f)
            {
                demandSystem.createDemand();
                nextDemandTime = demandSystem.getNextDemandInterval();
            }
        }
    }
}
public interface IDemandStrategy
{
    float NextDemandInterval(DemandContext demandContext);
    Demand GenerateDemand(DemandContext demandContext);
}

public abstract class Demand : AcceptOnceOffer
{
    public event Action<Demand> Rejected;
    public bool IsRejected { get; private set; } = false;

    protected Demand(IDemandValidationVisitor demandValidation)
    {
        RegisterValidator(demandValidation);
    }

    internal void Reject()
    {
        if(IsRejected)
            return;
        IsRejected = true;
        Rejected?.Invoke(this);
    }

    protected abstract void RegisterValidator(IDemandValidationVisitor validator);
}

public interface IDemandValidationVisitor
{
    public void RegisterValidation(ProductDemand demand);
}

public class DemandContext
{
    public List<Product> supplyProducts;
    public IDemandValidationVisitor demandValidator;

}

public interface IDemandResolver<T> where T : Demand
{
    public List<T> getPossibleDemands(DemandContext demandContext);
}

//Implementations

public class DemandValidatorVisitorImp : IDemandValidationVisitor
{
    ProductDemandValidator productDemandValidator = new ProductDemandValidator();
    
    public void RegisterValidation(ProductDemand demand)
    {
        productDemandValidator.RegisterDemand(demand);
    }

    class ProductDemandValidator
    {
        Dictionary<Product, Demand> demands = new Dictionary<Product, Demand>();
        public void ProductRemoved(Product product)
        {
            if (demands.ContainsKey(product))
            {
                var demand = demands[product];
                demand.Reject();
                demands.Remove(product);
            }
        }

        public void RegisterDemand(ProductDemand demand)
        {
            if (!demands.ContainsKey(demand.Product))
            {
                demands.Add(demand.Product, demand);
            }
            else
            {
                throw new InvalidOperationException("Demand for this product is already registered.");
            }
        }
    }

}

public class ProductDemand : Demand
{
    public readonly Product Product;
    public ProductDemand(Product product, IDemandValidationVisitor demandValidation)
        : base(demandValidation)
    {
        this.Product = product;
    }

    public override Transaction Accept()
    {
        throw new NotImplementedException();
    }

    protected override void RegisterValidator(IDemandValidationVisitor validator)
    {
        validator.RegisterValidation(this);
    }
}

public class RandomProductDemandPolicy : IDemandStrategy
{
    public float NextDemandInterval(DemandContext demandContext)
    {
        return Random.Range(5f, 15f);
    }
    public Demand GenerateDemand(DemandContext demandContext)
    {
        var products = demandContext.supplyProducts;
        if (products.Count == 0)
        {
            return null;
        }
        var randomIndex = Random.Range(0, products.Count);
        var selectedProduct = products[randomIndex];
        return new ProductDemand(selectedProduct,demandContext.demandValidator);
    }
}

