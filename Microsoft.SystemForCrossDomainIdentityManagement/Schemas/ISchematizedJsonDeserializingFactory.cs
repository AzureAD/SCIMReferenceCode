// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public interface ISchematizedJsonDeserializingFactory<TOutput> where TOutput : Schematized
    {
        TOutput Create(IReadOnlyDictionary<string, object> json);
    }
}