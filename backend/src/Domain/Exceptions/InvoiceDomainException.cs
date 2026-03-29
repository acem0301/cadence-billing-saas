namespace Domain.Exceptions;

public sealed class InvoiceDomainException(string message) : Exception(message)
{
}