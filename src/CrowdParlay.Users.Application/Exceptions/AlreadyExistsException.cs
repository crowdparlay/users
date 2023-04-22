namespace CrowdParlay.Users.Application.Exceptions;

public class AlreadyExistsException : Exception
{
    public AlreadyExistsException()
        : base("Entity with the specified identity already exists."){ }

    public AlreadyExistsException(string message)
        : base(message) { }

    public AlreadyExistsException(string message, Exception innerException)
        : base(message, innerException) { }

    public AlreadyExistsException(string key, object value)
        : base($"Entity with {key} = {value} already exists.") { }
}