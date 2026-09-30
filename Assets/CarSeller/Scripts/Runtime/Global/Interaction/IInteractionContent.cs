public interface IInteractionContentProfile<T, U>
    where T : IInteractionContent
    where U : IInteractionContext
{
    T GenerateContent(
        object model,
        U context
    );
}

public interface IInteractionContent { }

public interface IInteractionContext { }