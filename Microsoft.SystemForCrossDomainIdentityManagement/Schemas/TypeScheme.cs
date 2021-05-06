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
    public sealed class TypeScheme : Resource
    {
        private List<AttributeScheme> attributes;
        private IReadOnlyCollection<AttributeScheme> attributesWrapper;

        private object thisLock;

        public TypeScheme()
        {
            this.OnInitialization();
            this.OnInitialized();
            this.AddSchema(SchemaIdentifiers.Core2Schema);
            this.Metadata =
                new Core2Metadata()
                {
                    ResourceType = Types.Schema
                };
        }

        [DataMember(Name = AttributeNames.Attributes, Order = 0)]
        public IReadOnlyCollection<AttributeScheme> Attributes => this.attributesWrapper;

        [DataMember(Name = AttributeNames.Name)]
        public string Name
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Description)]
        public string Description
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Metadata)]
        public Core2Metadata Metadata
        {
            get;
            set;
        }

        public void AddAttribute(AttributeScheme attribute)
        {
            if (null == attribute)
            {
                throw new ArgumentNullException(nameof(attribute));
            }

            Func<bool> containsFunction =
                new Func<bool>(
                        () =>
                            this
                            .attributes
                            .Any(
                                (AttributeScheme item) =>
                                    string.Equals(item.Name, attribute.Name, StringComparison.OrdinalIgnoreCase)));


            if (!containsFunction())
            {
                lock (this.thisLock)
                {
                    if (!containsFunction())
                    {
                        this.attributes.Add(attribute);
                    }
                }
            }
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
            this.attributes = new List<AttributeScheme>();
        }

        private void OnInitialized()
        {
            this.attributesWrapper = this.attributes.AsReadOnly();
        }  
    }
}
