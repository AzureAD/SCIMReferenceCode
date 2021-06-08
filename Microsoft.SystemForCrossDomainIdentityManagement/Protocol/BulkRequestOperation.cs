//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class BulkRequestOperation : BulkOperation
    {
        private Uri path;

        [DataMember(Name = ProtocolAttributeNames.Path, Order = 0)]
        private string pathValue;

        private BulkRequestOperation()
        {
        }

        [DataMember(Name = ProtocolAttributeNames.Data, Order = 4)]
        public object Data
        {
            get;
            set;
        }

        public Uri Path
        {
            get => this.path;

            set
            {
                this.path = value;
                this.pathValue = new SystemForCrossDomainIdentityManagementResourceIdentifier(value).RelativePath;
            }
        }

        public static BulkRequestOperation CreateDeleteOperation(Uri resource) =>
            new BulkRequestOperation
            {
                Method = HttpMethod.Delete,
                Path = resource ?? throw new ArgumentNullException(nameof(resource))
            };

        public static BulkRequestOperation CreatePatchOperation(Uri resource, PatchRequest2 data)
        {
            if (null == resource)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (null == data)
            {
                throw new ArgumentNullException(nameof(data));
            }
            PatchRequest2 patchRequest = new PatchRequest2(data.Operations);
            BulkRequestOperation result = new BulkRequestOperation
            {
                Method = ProtocolExtensions.PatchMethod,
                Path = resource,
                Data = patchRequest
            };
            return result;
        }

        public static BulkRequestOperation CreatePostOperation(Resource data)
        {
            if (null == data)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (null == data.Schemas)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementProtocolResources.ExceptionUnidentifiableSchema);
            }

            if (!data.Schemas.Any())
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementProtocolResources.ExceptionUnidentifiableSchema);
            }

            IList<Uri> paths = new List<Uri>(1);
            IEnumerable<ISchemaIdentifier> schemaIdentifiers =
                data
                .Schemas
                .Select(
                    (string item) =>
                        new SchemaIdentifier(item));
            foreach (ISchemaIdentifier schemaIdentifier in schemaIdentifiers)
            {
                Uri schemaIdentifierPath = null;
                if (schemaIdentifier.TryFindPath(out string pathValue))
                {
                    schemaIdentifierPath = new Uri(pathValue, UriKind.Relative);
                    if
                    (
                        !paths
                        .Any(
                            (Uri item) =>
                                0 == Uri.Compare(
                                        item,
                                        schemaIdentifierPath,
                                        UriComponents.AbsoluteUri,
                                        UriFormat.UriEscaped,
                                        StringComparison.OrdinalIgnoreCase))
                    )
                    {
                        paths.Add(schemaIdentifierPath);
                    }
                }

                if (data.TryGetPathIdentifier(out Uri resourcePath))
                {
                    if
                   (
                       !paths
                       .Any(
                           (Uri item) =>
                               0 == Uri.Compare(
                                       item,
                                       resourcePath,
                                       UriComponents.AbsoluteUri,
                                       UriFormat.UriEscaped,
                                       StringComparison.OrdinalIgnoreCase))
                   )
                    {
                        paths.Add(resourcePath);
                    }
                }
            }

            if (paths.Count != 1)
            {
                string schemas = string.Join(Environment.NewLine, data.Schemas);
                throw new NotSupportedException(schemas);
            }

            BulkRequestOperation result = new BulkRequestOperation
            {
                path = paths.Single(),
                Method = HttpMethod.Post,
                Data = data
            };
            return result;
        }

        private void InitializePath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                this.path = null;
                return;
            }

            this.path = new Uri(value, UriKind.Relative);
        }

        private void InitializePath() => this.InitializePath(this.pathValue);
        
        [OnDeserialized]
        private void OnDeserialized(StreamingContext _) => this.InitializePath();
    }
}
