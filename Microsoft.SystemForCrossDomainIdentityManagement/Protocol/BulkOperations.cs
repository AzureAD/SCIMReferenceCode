// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class BulkOperations<TOperation> : Schematized where TOperation : BulkOperation
    {
        [DataMember(Name = ProtocolAttributeNames.Operations, Order = 2)]
        private List<TOperation> operations;
        private IReadOnlyCollection<TOperation> operationsWrapper;

        private object thisLock;

        protected BulkOperations(string schemaIdentifier)
        {
            if (string.IsNullOrWhiteSpace(schemaIdentifier))
            {
                throw new ArgumentNullException(nameof(schemaIdentifier));
            }

            this.AddSchema(schemaIdentifier);
            this.OnInitialization();
            this.OnInitialized();
        }

        public IReadOnlyCollection<TOperation> Operations => this.operationsWrapper;

        public void AddOperation(TOperation operation)
        {
            if (null == operation)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (string.IsNullOrWhiteSpace(operation.Identifier))
            {
                throw new ArgumentException(
                    SystemForCrossDomainIdentityManagementProtocolResources.ExceptionUnidentifiableOperation);
            }

            bool Contains() => this.operations.Any((BulkOperation item) => string.Equals(item.Identifier, operation.Identifier,
                StringComparison.OrdinalIgnoreCase));

            if (!Contains())
            {
                lock (this.thisLock)
                {
                    if (!Contains())
                    {
                        this.operations.Add(operation);
                    }
                }
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext _) => this.OnInitialized();

        [OnDeserializing]
        private void OnDeserializing(StreamingContext _) => this.OnInitialization();
       
        private void OnInitialization()
        {
            this.thisLock = new object();
            this.operations = new List<TOperation>();
        }

        private void OnInitialized() => this.operationsWrapper = this.operations.AsReadOnly();
    }
}
