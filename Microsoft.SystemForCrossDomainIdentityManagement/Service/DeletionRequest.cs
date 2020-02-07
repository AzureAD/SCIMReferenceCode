//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Collections.Generic;
    using System.Net.Http;

    public sealed class DeletionRequest :
        SystemForCrossDomainIdentityManagementRequest<IResourceIdentifier>
    {
        public DeletionRequest(
            HttpRequestMessage request,
            IResourceIdentifier payload,
            string correlationIdentifier,
            IReadOnlyCollection<IExtension> extensions)
            : base(request, payload, correlationIdentifier, extensions)
        {
        }
    }
}