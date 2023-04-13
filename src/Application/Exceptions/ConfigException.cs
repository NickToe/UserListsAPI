using System.Globalization;

namespace Application.Exceptions;

public class ConfigException : Exception
{
    public ConfigException() : base() { }

    public ConfigException(string message) : base(message) { }

    public ConfigException(string message, params object[] args) : base(string.Format(CultureInfo.CurrentCulture, message, args)) { }
}
