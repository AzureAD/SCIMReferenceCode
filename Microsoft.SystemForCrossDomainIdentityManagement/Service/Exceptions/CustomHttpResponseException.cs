using System;
using System.Net;

namespace Microsoft.SCIM
{
    public class CustomHttpResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }

        public CustomHttpResponseException(HttpStatusCode statusCode): base(GetErrorMessage(statusCode))
        {
            StatusCode = statusCode;
        }
        private static string GetErrorMessage(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.BadRequest:
                    return "Request is unparsable, syntactically incorrect, or violates schema.";
                case HttpStatusCode.Unauthorized:
                    return "Authorization failure. The authorization header was invalid or missing.";
                case HttpStatusCode.Forbidden:
                    return "Operation is not permitted based on the supplied authorization.";
                case HttpStatusCode.NotFound:
                    return "Specified resource (e.g., User, Group, etc.) or endpoint does not exist.";
                case HttpStatusCode.Conflict:
                    return "The specified version number does not match the resource's latest version number, or a service provider refused to create a new, duplicate resource.";
                case HttpStatusCode.InternalServerError:
                    return "The server encountered an unexpected condition which prevented it from fulfilling the request.";
                case HttpStatusCode.NotImplemented:
                    return "Service provider does not support the requested operation.";
                default:
                    return "An error occurred.";
            }
        }
    }
}
