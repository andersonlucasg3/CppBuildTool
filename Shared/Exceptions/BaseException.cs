namespace Shared.Exceptions;

public abstract class BaseException : Exception
{
    protected BaseException() : base()
    {
        //
    }

    protected BaseException(string? message) : base(message)
    {
        //
    }
}
