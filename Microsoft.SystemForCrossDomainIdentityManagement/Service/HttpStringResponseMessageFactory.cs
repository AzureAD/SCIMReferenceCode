//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Net.Http;

    internal class HttpStringResponseMessageFactory : HttpResponseMessageFactory<string>
    {
        private const string ArgumentNameContent = "content";

        public override HttpContent ProvideContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentNullException(HttpStringResponseMessageFactory.ArgumentNameContent);
            }

            HttpContent result = null;
            try
            {
                result = new StringContent(content);
                return result;
            }
            catch
            {
                if (result != null)
                {
                    result.Dispose();
                    result = null;
                }

                throw;
            }
        }
    }
}