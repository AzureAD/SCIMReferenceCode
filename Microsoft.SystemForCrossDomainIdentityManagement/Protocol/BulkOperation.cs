//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Net.Http;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class BulkOperation
    {
        private HttpMethod method;
        private string methodName;

        protected BulkOperation()
        {
            this.Identifier = Guid.NewGuid().ToString();
        }

        protected BulkOperation(string identifier)
        {
            this.Identifier = identifier;
        }

        [DataMember(Name = ProtocolAttributeNames.BulkOperationIdentifier, Order = 1)]
        public string Identifier
        {
            get;
            private set;
        }

        public HttpMethod Method
        {
            get => this.method;

            set
            {
                this.method = value;
                if (value != null)
                {
                    this.methodName = value.ToString();
                }
            }
        }

        [DataMember(Name = ProtocolAttributeNames.Method, Order = 0)]
#pragma warning disable IDE0051 // Remove unused private members
        private string MethodName
#pragma warning restore IDE0051 // Remove unused private members
        {
            get => this.methodName;

            set
            {
                this.method = new HttpMethod(value);
                this.methodName = value;
            }
        }
    }
}
