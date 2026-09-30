public enum PartSlotType { Engine, Wheels, Spoiler }

public interface IPartSlot
{
    public abstract PartSlotType SlotType { get; }
    public bool TryAccept(Product product);
    public abstract void Detach();
}