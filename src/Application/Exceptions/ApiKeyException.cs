using System.Globalization;

namespace Application.Exceptions;

public class ApiKeyException : Exception
{
    public ApiKeyException() : base() { }

    public ApiKeyException(string message) : base(message) { }

    public ApiKeyException(string message, params object[] args) : base(string.Format(CultureInfo.CurrentCulture, message, args)) { }
}
