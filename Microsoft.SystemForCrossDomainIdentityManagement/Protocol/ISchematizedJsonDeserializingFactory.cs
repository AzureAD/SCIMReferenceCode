// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    internal interface ISchematizedJsonDeserializingFactory :
        IGroupDeserializer,
        IPatchRequest2Deserializer,
        IUserDeserializer
    {
    }
}