//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class ErrorBase : Schematized
    {
        [DataMember(Name = "scimType", Order = 1)] //AttributeNames.ScimType
        public virtual string ScimType
        {
            get;
            set;
        }

        [DataMember(Name = "detail", Order = 2)] //AttributeNames.Detail
        public virtual string Detail
        {
            get;
            set;
        }

        [DataMember(Name = "status", Order = 3)] //AttributeNames.Status
        public virtual int Status
        {
            get;
            set;
        }
    }
}
