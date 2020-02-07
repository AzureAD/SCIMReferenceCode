//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class QualifiedResource : Resource
    {
        private const string ResourceSchemaIdentifierTemplateSuffix = "{0}";

        private string resourceSchemaIdentifierTemplate;

        protected QualifiedResource(string schemaIdentifier, string resourceSchemaPrefix)
        {
            this.OnInitialized(schemaIdentifier, resourceSchemaPrefix);
        }

        private string ResourceSchemaPrefix
        {
            get;
            set;
        }

        public virtual void AddResourceSchemaIdentifier(string resourceTypeName)
        {
            if (this.TryGetResourceTypeName(out string value))
            {
                string typeName = this.GetType().Name;
                string errorMessage =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SystemForCrossDomainIdentityManagementSchemasResources.ExceptionMultipleQualifiedResourceTypeIdentifiersTemplate,
                        typeName);
                throw new InvalidOperationException(errorMessage);
            }
            string schemaIdentifier =
                string.Format(
                    CultureInfo.InvariantCulture,
                    this.resourceSchemaIdentifierTemplate,
                    resourceTypeName);
            this.AddSchema(schemaIdentifier);
        }

        public void OnDeserialized(string schemaIdentifier, string resourceSchemaPrefix)
        {
            this.OnInitialized(schemaIdentifier, resourceSchemaPrefix);
            int countResourceSchemaIdentifiers =
                this
                .Schemas
                .Where(
                    (string item) =>
                        item.StartsWith(this.ResourceSchemaPrefix, StringComparison.Ordinal))
                .Count();
            if (countResourceSchemaIdentifiers > 1)
            {
                string typeName = this.GetType().Name;
                string errorMessage =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SystemForCrossDomainIdentityManagementSchemasResources.ExceptionMultipleQualifiedResourceTypeIdentifiersTemplate,
                        typeName);
                throw new InvalidOperationException(errorMessage);
            }
        }

        private void OnInitialized(string schemaIdentifier, string resourceSchemaPrefix)
        {
            if (string.IsNullOrWhiteSpace(schemaIdentifier))
            {
                throw new ArgumentNullException(nameof(schemaIdentifier));
            }

            if (string.IsNullOrWhiteSpace(resourceSchemaPrefix))
            {
                throw new ArgumentNullException(nameof(resourceSchemaPrefix));
            }

            this.ResourceSchemaPrefix = resourceSchemaPrefix;
            this.resourceSchemaIdentifierTemplate =
                this.ResourceSchemaPrefix + QualifiedResource.ResourceSchemaIdentifierTemplateSuffix;
        }

        public virtual bool TryGetResourceTypeName(out string resourceTypeName)
        {
            resourceTypeName = null;

            string resourceSchemaIdentifier =
                this
                .Schemas
                .SingleOrDefault(
                    (string item) =>
                        item.StartsWith(this.ResourceSchemaPrefix, StringComparison.Ordinal));
            if (string.IsNullOrWhiteSpace(resourceSchemaIdentifier))
            {
                return false;
            }
            string buffer = resourceSchemaIdentifier.Substring(this.ResourceSchemaPrefix.Length);
            if (buffer.Length <= 0)
            {
                return false;
            }
            resourceTypeName = buffer;
            return true;
        }
    }
}