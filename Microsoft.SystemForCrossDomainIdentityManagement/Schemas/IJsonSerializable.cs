//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Collections.Generic;

    public interface IJsonSerializable
    {
        Dictionary<string, object> ToJson();
        string Serialize();
    }
}
