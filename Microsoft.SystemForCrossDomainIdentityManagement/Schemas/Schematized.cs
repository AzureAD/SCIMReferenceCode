//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class Schematized : IJsonSerializable
    {
        [DataMember(Name = AttributeNames.Schemas, Order = 0)]
        private List<string> schemas;
        private IReadOnlyCollection<string> schemasWrapper;

        private object thisLock;
        private IJsonSerializable serializer;

        protected Schematized()
        {
            this.OnInitialization();
            this.OnInitialized();
        }

        public virtual IReadOnlyCollection<string> Schemas
        {
            get
            {
                return this.schemasWrapper;
            }
        }

        public void AddSchema(string schemaIdentifier)
        {
            if (string.IsNullOrWhiteSpace(schemaIdentifier))
            {
                throw new ArgumentNullException(nameof(schemaIdentifier));
            }

            Func<bool> containsFunction =
                new Func<bool>(
                    () =>
                        this
                        .schemas
                        .Any(
                            (string item) =>
                                string.Equals(
                                    item,
                                    schemaIdentifier,
                                    StringComparison.OrdinalIgnoreCase)));


            if (!containsFunction())
            {
                lock (this.thisLock)
                {
                    if (!containsFunction())
                    {
                        this.schemas.Add(schemaIdentifier);
                    }
                }
            }
        }

        public bool Is(string scheme)
        {
            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentNullException(nameof(scheme));
            }

            bool result =
                this
                .schemas
                .Any(
                    (string item) =>
                        string.Equals(item, scheme, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            this.OnInitialized();
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.OnInitialization();
        }

        private void OnInitialization()
        {
            this.thisLock = new object();
            this.serializer = new JsonSerializer(this);
            this.schemas = new List<string>();
        }

        private void OnInitialized()
        {
            this.schemasWrapper = this.schemas.AsReadOnly();
        }

        public virtual Dictionary<string, object> ToJson()
        {
            Dictionary<string, object> result = this.serializer.ToJson();
            return result;
        }

        public virtual string Serialize()
        {
            IDictionary<string, object> json = this.ToJson();
            string result = JsonFactory.Instance.Create(json, true);
            return result;
        }

        public override string ToString()
        {
            string result = this.Serialize();
            return result;
        }

        public virtual bool TryGetPath(out string path)
        {
            path = null;
            return false;
        }

        public virtual bool TryGetSchemaIdentifier(out string schemaIdentifier)
        {
            schemaIdentifier = null;
            return false;
        }
    }
}