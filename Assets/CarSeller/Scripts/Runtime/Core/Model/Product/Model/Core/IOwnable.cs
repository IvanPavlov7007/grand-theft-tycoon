public interface IOwnable
{
    IOwnable Owner { get; }
}

public interface IMutableOwnable : IOwnable
{
    void SetOwner(IOwnable newOwner);
}

public abstract class OwnableBase : IMutableOwnable
{
    public IOwnable Owner { get; private set; }

    public void SetOwner(IOwnable newOwner)
    {
        if (Owner == newOwner)
            return;

        Owner = newOwner;
    }
}

public enum OwnershipResolution
{
    None,           // Item remains with current owner
    Clear,          // Item becomes unowned
    Container,      // Container becomes owner (car parts)
    OwnerOfContainerIfNull  // Owner of container becomes owner (warehouse → player)
}

public interface IOwnershipContainer
{
    OwnershipResolution OwnershipResolution { get; }
    IOwnable GetOwnerOfContainer();
}