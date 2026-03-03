namespace NETmessenger.Web.Services.Exceptions;

public sealed class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message)
    {
    }
}
