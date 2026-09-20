namespace backend.Infrastructure.Exceptions;

public class DependencyUnavailableException : Exception
{
    public DependencyUnavailableException(string message) : base(message)
    {
    }

    public DependencyUnavailableException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
