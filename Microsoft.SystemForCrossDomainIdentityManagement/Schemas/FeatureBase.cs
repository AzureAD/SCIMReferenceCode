// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class FeatureBase
    {
        [DataMember(Name = AttributeNames.Supported)]
        public bool Supported
        {
            get;
            set;
        }
    }
}