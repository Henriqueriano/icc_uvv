namespace backend.Infrastructure.Exceptions;

public class SparqlException : Exception
{
    public SparqlException(string message) : base(message)
    {
    }

    public SparqlException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
