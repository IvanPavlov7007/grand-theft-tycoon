using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuyerManager
{
    Dictionary<Buyer, BuyerSpawnPoint> activeBuyers = new Dictionary<Buyer, BuyerSpawnPoint>();
    Dictionary<Buyer, BuyersPool> buyerPools = new Dictionary<Buyer, BuyersPool>();

    public void TrySpawnBuyerAtRandomFreeSpawnPoint(AreaBuyerSystem system, BuyersPool pool)
    {
        if (system == null || pool == null)
            return;

        if(system.activeSpawnPoints.Count >= system.Area.CurrentLevel.MaxBuyersCount)
            return;

        var freeSpawnPoints = system.SpawnPoints.Where(sp => sp.buyer == null).ToList();
        if (freeSpawnPoints.Count == 0)
            return;

        if (pool.Queue == null || pool.Queue.Count == 0)
            return;

        var randomSpawnPoint = freeSpawnPoints[UnityEngine.Random.Range(0, freeSpawnPoints.Count)];
        Buyer buyer = pool.Queue.Dequeue();
        if (buyer == null)
            return;

        var edge = randomSpawnPoint.Position.Edge;
        // Check if the buyer's required car type can be spawned on this edge
        if (!GameRules.BuyerTypeCanBeSpanwnedOnEdge.Check(buyer.RequiredCarType, edge))
        {
            pool.Queue.Enqueue(buyer);
            return;
        }

        SpawnBuyerAtPosition(buyer, randomSpawnPoint.Position, randomSpawnPoint, pool);
        system.lastSpawnTime = Time.time;
    }

    public void SpawnBuyerAtPosition(Buyer buyer, CityPosition position, BuyerSpawnPoint spawnPoint, BuyersPool pool)
    {
        if (buyer == null)
            return;

        CityEntitiesCreationHelper.CreateBuyer(buyer, position);

        buyer.onBeingDestroyed -= onBuyerRemove;
        buyer.onBeingDestroyed += onBuyerRemove;

        if (spawnPoint != null)
        {
            spawnPoint.buyer = buyer;
            activeBuyers[buyer] = spawnPoint;
        }

        if (pool != null)
        {
            buyerPools[buyer] = pool;
        }
    }

    void onBuyerRemove(IDestroyable buyer)
    {
        if (buyer is not Buyer b)
            return;

        b.onBeingDestroyed -= onBuyerRemove;

        if (activeBuyers.TryGetValue(b, out BuyerSpawnPoint spawnPoint))
        {
            spawnPoint.buyer = null;
            activeBuyers.Remove(b);
        }

        if (buyerPools.TryGetValue(b, out var pool))
        {
            buyerPools.Remove(b);

            if (pool.AllItems.Contains(b) && !pool.Queue.Contains(b))
                pool.Queue.Enqueue(b);
        }
    }

    public static List<Buyer> CreatePooledBuyerList(BuyerSpawnSpawnWeight[] weights)
    {
        var list = new List<Buyer>();
        foreach (var weight in weights)
        {
            int count = Mathf.FloorToInt(weight.Weight * 10); // Scale weight to determine count
            for (int i = 0; i < count; i++)
            {
                Buyer buyer = new Buyer(weight.CarType);
                list.Add(buyer);
            }
        }

        return list;
    }
}

public class Buyer : CityDestroyable, ILocatable
{
    public readonly CarType RequiredCarType;
    public string Name = "Buyer";

    public Buyer(CarType requiredCarType)
    {
        this.RequiredCarType = requiredCarType;
    }

    public static Buyer Any()
    {
        return new Buyer(CarType.Any);
    }
}