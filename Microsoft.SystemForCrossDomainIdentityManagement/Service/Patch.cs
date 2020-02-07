//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;

    public sealed class Patch : IPatch
    {
        public Patch()
        {
        }

        public Patch(IResourceIdentifier resourceIdentifier, PatchRequestBase request)
        {
            this.ResourceIdentifier = resourceIdentifier ?? throw new ArgumentNullException(nameof(resourceIdentifier));
            this.PatchRequest = request ?? throw new ArgumentNullException(nameof(request));
        }

        public PatchRequestBase PatchRequest
        {
            get;
            set;
        }

        public IResourceIdentifier ResourceIdentifier
        {
            get;
            set;
        }
    }
}