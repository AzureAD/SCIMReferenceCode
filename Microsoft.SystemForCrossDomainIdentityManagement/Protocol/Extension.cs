// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Net.Http;

    public abstract class Extension : IExtension
    {
        private const string ArgumentNameController = "controller";
        private const string ArgumentNameJsonDeserializingFactory = "jsonDeserializingFactory";
        private const string ArgumentNamePath = "path";
        private const string ArgumentNameSchemaIdentifier = "schemaIdentifier";
        private const string ArgumentNameTypeName = "typeName";

        protected Extension(
            string schemaIdentifier,
            string typeName,
            string path,
            Type controller,
            JsonDeserializingFactory jsonDeserializingFactory)
        {
            if (string.IsNullOrWhiteSpace(schemaIdentifier))
            {
                throw new ArgumentNullException(Extension.ArgumentNameSchemaIdentifier);
            }

            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw new ArgumentNullException(Extension.ArgumentNameTypeName);
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(Extension.ArgumentNamePath);
            }

            this.SchemaIdentifier = schemaIdentifier;
            this.TypeName = typeName;
            this.Path = path;
            this.Controller = controller ?? throw new ArgumentNullException(Extension.ArgumentNameController);
            this.JsonDeserializingFactory = jsonDeserializingFactory ?? throw new ArgumentNullException(Extension.ArgumentNameJsonDeserializingFactory);
        }

        public Type Controller
        {
            get;
            private set;
        }

        public JsonDeserializingFactory JsonDeserializingFactory
        {
            get;
            private set;
        }

        public string Path
        {
            get;
            private set;
        }

        public string SchemaIdentifier
        {
            get;
            private set;
        }

        public string TypeName
        {
            get;
            private set;
        }

        public virtual bool Supports(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            bool result =
                request.RequestUri?.AbsolutePath?.EndsWith(
                       this.Path,
                       StringComparison.OrdinalIgnoreCase) == true;

            return result;
        }
    }
}