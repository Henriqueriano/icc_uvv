namespace backend.Infrastructure.Exceptions;

public class RdfException : Exception
{
    public RdfException(string message) : base(message)
    {
    }

    public RdfException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
