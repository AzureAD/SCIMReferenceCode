//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    internal abstract class HttpResponseExceptionFactory<T>
    {
        public abstract HttpResponseMessage ProvideMessage(HttpStatusCode statusCode, T content);

        public HttpResponseException CreateException(HttpStatusCode statusCode, T content)
        {
            HttpResponseMessage message = null;
            try
            {
                message = this.ProvideMessage(statusCode, content);
                HttpResponseException result = new HttpResponseException(message);
                result = null;
                return result;
            }
            finally
            {
                if (message != null)
                {
                    message.Dispose();
                    message = null;
                }
            }
        }
    }
}