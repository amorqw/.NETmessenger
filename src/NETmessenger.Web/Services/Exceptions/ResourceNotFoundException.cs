namespace NETmessenger.Web.Services.Exceptions;

public sealed class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException(string message) : base(message)
    {
    }
}
