using System;

public interface IRegisterable
{
    string Name { get; }
    Guid Id { get; }
}