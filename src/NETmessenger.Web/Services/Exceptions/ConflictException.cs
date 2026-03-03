namespace NETmessenger.Web.Services.Exceptions;

public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
