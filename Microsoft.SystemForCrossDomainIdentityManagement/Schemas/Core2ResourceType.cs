//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class Core2ResourceType : Resource
    {
        private Uri endpoint;

        [DataMember(Name = AttributeNames.Endpoint)]
        private string endpointValue;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Serialized")]
        [DataMember(Name = AttributeNames.Name)]
        private string name;

        public Core2ResourceType()
        {
            this.AddSchema(SchemaIdentifiers.Core2ResourceType);
            this.Metadata =
                new Core2Metadata()
                {
                    ResourceType = Types.ResourceType
                };
        }

        public Uri Endpoint
        {
            get
            {
                return this.endpoint;
            }

            set
            {
                this.endpoint = value;
                this.endpointValue = new SystemForCrossDomainIdentityManagementResourceIdentifier(value).RelativePath;
            }
        }

        [DataMember(Name = AttributeNames.Metadata)]
        public Core2Metadata Metadata
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Schema)]
        public string Schema
        {
            get;
            set;
        }

        private void InitializeEndpoint(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                this.endpoint = null;
                return;
            }

            this.endpoint = new Uri(value, UriKind.Relative);
        }

        private void InitializeEndpoint()
        {
            this.InitializeEndpoint(this.endpointValue);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.InitializeEndpoint();
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            this.name = this.Identifier;
        }
    }
}