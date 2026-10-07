// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.


namespace Microsoft.SCIM
{
    using System.Collections.Generic;
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    internal class RootProviderAdapter : ProviderAdapterTemplate<Resource>
    {
        public RootProviderAdapter(IProvider provider)
            : base(provider)
        {
        }

        public override string SchemaIdentifier
        {
            get
            {
                return SchemaIdentifiers.None;
            }
        }

        public override Task<Resource> Create(
            HttpContext httpContext,
            Resource resource,
            string correlationIdentifier)
        {
            throw new NotImplementedException();
        }

        public override IResourceIdentifier CreateResourceIdentifier(string identifier)
        {
            throw new NotImplementedException();
        }

        public override Task<Resource> Delete(
            HttpContext httpContext,
            string identifier,
            string correlationIdentifier)
        {
            throw new NotImplementedException();
        }

        public override Task<Resource> Replace(
            HttpContext httpContext,
            Resource resource, string
            correlationIdentifier)
        {
            throw new NotImplementedException();
        }

        public override Task<Resource> Retrieve(
            HttpContext httpContext,
            string identifier,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths,
            string correlationIdentifier)
        {
            throw new NotImplementedException();
        }

        public override Task<Resource> Update(
            HttpContext httpContext,
            string identifier,
            PatchRequestBase patchRequest,
            string correlationIdentifier)
        {
            throw new NotImplementedException();
        }
    }
}
