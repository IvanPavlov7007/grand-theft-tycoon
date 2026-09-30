using System.Collections;
public class ProcessRunner : RoutinedObject
{
    public bool Run(IProcess process)
    {
        return TryStartRoutine(process.Run());
    }
}
public interface IProcess
{
    public IEnumerator Run();
}