//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Net;
    using System.Net.Http;

    internal abstract class HttpResponseMessageFactory<T>
    {
        public abstract HttpContent ProvideContent(T content);

        public HttpResponseMessage CreateMessage(HttpStatusCode statusCode, T content)
        {
            HttpContent messageContent = null;
            try
            {
                messageContent = this.ProvideContent(content);
                HttpResponseMessage result = null;
                try
                {
                    result = new HttpResponseMessage(statusCode);
                    result.Content = messageContent;
                    messageContent = null;
                    return result;
                }
                catch
                {
                    if (result != null)
                    {
                        result.Dispose();
                        result = null;
                    };

                    throw;
                }
            }
            finally
            {
                if (messageContent != null)
                {
                    messageContent.Dispose();
                    messageContent = null;
                }
            }
        }
    }
}