// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public interface IMetadataProvider
    {
        Core2ServiceConfiguration Configuration { get; }
        IReadOnlyCollection<Core2ResourceType> ResourceTypes { get; }
        IReadOnlyCollection<TypeScheme> Schema { get; }
    }
}
