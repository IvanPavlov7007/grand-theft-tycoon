/// <summary>
/// A Visitor-Pattern interface for building product views.
/// Products select the appropriate method to call based on their type.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IProductViewBuilder<T>
{
    T BuildCar(Car car);
    T BuildWheel(Wheel wheel);
    T BuildEngine(Engine engine);
    T BuildSpoiler(Spoiler spoiler);
    T BuildCarFrame(CarFrame carFrame);
}