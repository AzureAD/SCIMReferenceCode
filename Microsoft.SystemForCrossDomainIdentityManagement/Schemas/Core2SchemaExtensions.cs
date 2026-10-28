//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class Core2SchemaExtensions
    {
        [DataMember(Name = AttributeNames.Schema, Order = 0)]
        public string Schema
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Required, Order = 1)]
        public bool Required
        {
            get;
            set;
        }
    }
}
