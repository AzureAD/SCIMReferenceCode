using System;

namespace Microsoft.SCIM;

public class ScimTypeException: Exception
{
    public ScimTypeException()
    {
    }

    public ErrorType ErrorType { get; set; }

    public ScimTypeException(string message): base(message)
    {
    }

    public ScimTypeException(string message, Exception innerException): base(message, innerException)
    {
    }

    public ScimTypeException(ErrorType errorType, string message) : base(message)
    {
        this.ErrorType = errorType;
    }
}
