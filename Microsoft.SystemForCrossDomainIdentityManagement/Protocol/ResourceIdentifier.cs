//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Globalization;

    public sealed class ResourceIdentifier : IResourceIdentifier
    {
        public ResourceIdentifier()
        {
        }

        public ResourceIdentifier(string schemaIdentifier, string resourceIdentifier)
        {
            if (string.IsNullOrWhiteSpace(schemaIdentifier))
            {
                throw new ArgumentNullException(nameof(schemaIdentifier));
            }

            if (string.IsNullOrWhiteSpace(resourceIdentifier))
            {
                throw new ArgumentNullException(nameof(resourceIdentifier));
            }

            this.SchemaIdentifier = schemaIdentifier;
            this.Identifier = resourceIdentifier;
        }

        public string Identifier
        {
            get;
            set;
        }

        public string SchemaIdentifier
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            IResourceIdentifier otherIdentifier = obj as IResourceIdentifier;
            if (null == otherIdentifier)
            {
                return false;
            }

            if (!string.Equals(this.SchemaIdentifier, otherIdentifier.SchemaIdentifier, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.Equals(this.Identifier, otherIdentifier.Identifier, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int identifierCode = string.IsNullOrWhiteSpace(this.Identifier) ? 0 : this.Identifier.GetHashCode(StringComparison.InvariantCulture);
            int schemaIdentifierCode = string.IsNullOrWhiteSpace(this.SchemaIdentifier) ? 0 : this.SchemaIdentifier.GetHashCode(StringComparison.InvariantCulture);
            int result = identifierCode ^ schemaIdentifierCode;
            return result;
        }

        public override string ToString()
        {
            string result =
                string.Format(
                    CultureInfo.InvariantCulture,
                    SystemForCrossDomainIdentityManagementProtocolResources.ResourceIdentifierTemplate,
                    this.SchemaIdentifier,
                    this.Identifier);
            return result;
        }
    }
}