// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class TypedValue : TypedItem
    {
        [DataMember(Name = AttributeNames.Value, Order = 0)]
        public string Value
        {
            get;
            set;
        }
    }
}