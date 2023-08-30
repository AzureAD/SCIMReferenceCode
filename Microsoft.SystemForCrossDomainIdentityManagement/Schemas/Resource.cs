// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class Resource : Schematized
    {
        [DataMember(Name = AttributeNames.ExternalIdentifier, IsRequired = false, EmitDefaultValue = false)]
        public string ExternalIdentifier
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Identifier)]
        public string Identifier
        {
            get;
            set;
        }

        public virtual bool TryGetIdentifier(Uri baseIdentifier, out Uri identifier)
        {
            identifier = null;
            return false;
        }

        public virtual bool TryGetPathIdentifier(out Uri pathIdentifier)
        {
            pathIdentifier = null;
            return false;
        }
    }
}