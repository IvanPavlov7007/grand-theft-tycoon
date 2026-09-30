using System.Collections.Generic;

/// <summary>
/// Generic preview of an offer for any logical operation
/// </summary>
public interface IOffer
{
    public abstract Transaction Accept();
    public abstract bool CanAccept();
}


public abstract class AcceptOnceOffer : IOffer
{
    public bool IsAccepted { get; protected set; } = false;

    public abstract Transaction Accept();

    public virtual bool CanAccept()
    {
        return !IsAccepted;
    }
}

public interface IOfferProvider<T> where T : IOffer
{
    List<T> GetOffers();
    void OfferAccepted(T offer);
}