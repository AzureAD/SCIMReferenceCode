// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class QueryResponseBase : Schematized
    {
        [DataMember(Name = ProtocolAttributeNames.Resources, Order = 3)]
        private Resource[] resources = null;

        protected QueryResponseBase()
        {
            this.AddSchema(ProtocolSchemaIdentifiers.Version2ListResponse);
        }

        protected QueryResponseBase(IReadOnlyCollection<Resource> resources)
            : this()
        {
            if (null == resources)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            this.resources = resources.ToArray();
        }

        protected QueryResponseBase(IList<Resource> resources)
            : this()
        {
            if (null == resources)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            this.resources = resources.ToArray();
        }

        [DataMember(Name = ProtocolAttributeNames.ItemsPerPage, Order = 1)]
        public int ItemsPerPage
        {
            get;
            set;
        }

        public IEnumerable<Resource> Resources
        {
            get
            {
                return this.resources;
            }

            set
            {
                if (null == value)
                {
                    throw new InvalidOperationException(SystemForCrossDomainIdentityManagementProtocolResources.ExceptionInvalidValue);
                }

                this.resources = value.ToArray();
            }
        }

        [DataMember(Name = ProtocolAttributeNames.StartIndex, Order = 2)]
        public int? StartIndex
        {
            get;
            set;
        }

        [DataMember(Name = ProtocolAttributeNames.TotalResults, Order = 0)]
        public int TotalResults
        {
            get;
            set;
        }
    }

    [DataContract]
    public abstract class QueryResponseBase<TResource> : Schematized where TResource : Resource
    {
        [DataMember(Name = ProtocolAttributeNames.Resources, Order = 3)]
        private TResource[] resources;

        protected QueryResponseBase(string schemaIdentifier)
        {
            if (string.IsNullOrWhiteSpace(schemaIdentifier))
            {
                throw new ArgumentNullException(nameof(schemaIdentifier));
            }

            this.AddSchema(schemaIdentifier);
            this.OnInitialization();
        }

        protected QueryResponseBase(string schemaIdentifier, IReadOnlyCollection<TResource> resources)
            : this(schemaIdentifier)
        {
            if (null == resources)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            this.resources = resources.ToArray();
        }

        protected QueryResponseBase(string schemaIdentifier, IList<TResource> resources)
            : this(schemaIdentifier)
        {
            if (null == resources)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            this.resources = resources.ToArray();
        }

        [DataMember(Name = ProtocolAttributeNames.ItemsPerPage, Order = 1)]
        public int ItemsPerPage
        {
            get;
            set;
        }

        public IEnumerable<TResource> Resources
        {
            get
            {
                return this.resources;
            }

            set
            {
                if (null == value)
                {
                    throw new InvalidOperationException(SystemForCrossDomainIdentityManagementProtocolResources.ExceptionInvalidValue);
                }
                this.resources = value.ToArray();
            }
        }

        [DataMember(Name = ProtocolAttributeNames.StartIndex, Order = 2)]
        public int? StartIndex
        {
            get;
            set;
        }

        [DataMember(Name = ProtocolAttributeNames.TotalResults, Order = 0)]
        public int TotalResults
        {
            get;
            set;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.OnInitialization();
        }

        private void OnInitialization()
        {
            this.resources = Array.Empty<TResource>();
        }
    }
}