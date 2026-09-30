using System;

[Obsolete]
public interface IPossession
{
    Guid Id { get; }
    string Name { get; }
}