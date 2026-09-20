namespace ProductInventory.Domain.Exceptions;

/// <summary>Base type for exceptions representing violations of domain/business rules.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message)
    {
    }
}
