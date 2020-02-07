//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    internal interface ISchematizedJsonDeserializingFactory :
        IGroupDeserializer,
        IPatchRequest2Deserializer,
        IUserDeserializer
    {
    }
}