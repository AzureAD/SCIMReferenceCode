//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "None")]
    public static class ProtocolExtensions
    {
        private const string BulkIdentifierPattern =
            @"^((\s*)bulkId(\s*):(\s*)(?<" +
            ProtocolExtensions.ExpressionGroupNameBulkIdentifier +
            @">[^\s]*))";

        private const string ExpressionGroupNameBulkIdentifier = "identifier";
        public const string MethodNameDelete = "DELETE";
        public const string MethodNamePatch = "PATCH";
        private static readonly Lazy<HttpMethod> MethodPatch =
            new Lazy<HttpMethod>(
                () =>
                    new HttpMethod(ProtocolExtensions.MethodNamePatch));
        private static readonly Lazy<Regex> BulkIdentifierExpression =
            new Lazy<Regex>(
                () =>
                    new Regex(ProtocolExtensions.BulkIdentifierPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
        private interface IHttpRequestMessageWriter : IDisposable
        {
            void Close();
            Task FlushAsync();
            Task WriteAsync();
        }

        public static HttpMethod PatchMethod
        {
            get
            {
                return ProtocolExtensions.MethodPatch.Value;
            }
        }

        public static void Apply(this Core2Group group, PatchRequest2 patch)
        {
            if (null == group)
            {
                throw new ArgumentNullException(nameof(group));
            }

            if (null == patch)
            {
                return;
            }

            if (null == patch.Operations || !patch.Operations.Any())
            {
                return;
            }

            foreach (PatchOperation2Combined operation in patch.Operations)
            {
                PatchOperation2 operationInternal = new PatchOperation2()
                {
                    OperationName = operation.OperationName,
                    Path = operation.Path
                };

                OperationValue[] values = null;
                if (operation?.Value != null)
                {
                    values =
                    JsonConvert.DeserializeObject<OperationValue[]>(
                        operation.Value,
                        ProtocolConstants.JsonSettings.Value);
                }

                if (values == null)
                {
                    string value = null;
                    if (operation?.Value != null)
                    {
                        value = JsonConvert.DeserializeObject<string>(operation.Value, ProtocolConstants.JsonSettings.Value);
                    }

                    OperationValue valueSingle = new OperationValue()
                    {
                        Value = value
                    };
                    operationInternal.AddValue(valueSingle);
                }
                else
                {
                    foreach(OperationValue value in values)
                    {
                        operationInternal.AddValue(value);
                    }
                }

                group.Apply(operationInternal);
            }
        }

        private static void Apply(this Core2Group group, PatchOperation2 operation)
        {
            if (null == operation || null == operation.Path || string.IsNullOrWhiteSpace(operation.Path.AttributePath))
            {
                return;
            }

            OperationValue value;
            switch (operation.Path.AttributePath)
            {
                case AttributeNames.DisplayName:
                    value = operation.Value.SingleOrDefault();

                    if (OperationName.Remove == operation.Name)
                    {
                        if (
                            null == value
                            || string.Equals(group.DisplayName, value.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            value = null;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (null == value)
                    {
                        group.DisplayName = null;
                    }
                    else
                    {
                        group.DisplayName = value.Value;
                    }
                    break;

                case AttributeNames.Members:
                    if (operation.Value != null)
                    {
                        switch (operation.Name)
                        {
                            case OperationName.Add:
                                IEnumerable<Member> membersToAdd =
                                     operation
                                     .Value
                                     .Select(
                                         (OperationValue item) =>
                                             new Member()
                                             {
                                                 Value = item.Value
                                             })
                                     .ToArray();

                                IList<Member> buffer = new List<Member>();
                                if(null == group.Members)
                                {
                                    group.Members = new List<Member>();
                                }
                                foreach (Member member in membersToAdd)
                                {
                                    //O(n) with the number of group members, so for large groups this is not optimal
                                    if (!group.Members.Any((Member item) =>
                                            string.Equals(item.Value, member.Value, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        buffer.Add(member);
                                    }
                                }

                                group.Members = group.Members.Concat(buffer.ToArray());
                                break;

                            case OperationName.Remove:
                                if (null == group.Members)
                                {
                                    break;
                                }

                                if (operation?.Value?.FirstOrDefault()?.Value == null)
                                {
                                    group.Members = Enumerable.Empty<Member>();
                                    break;
                                }

                                IDictionary<string, Member> members =
                                    new Dictionary<string, Member>(group.Members.Count());
                                foreach (Member item in group.Members)
                                {
                                    members.Add(item.Value, item);
                                }

                                foreach (OperationValue operationValue in operation.Value)
                                {
                                    if (members.TryGetValue(operationValue.Value, out Member removedMember))
                                    {
                                        members.Remove(operationValue.Value);
                                    }
                                }

                                group.Members = members.Values;
                                break;
                        }
                    }
                    break;
                case AttributeNames.ExternalIdentifier:

                    value = operation.Value.SingleOrDefault();

                    group.ExternalIdentifier= value?.Value;

                    break;
            }
        }

        public static HttpRequestMessage ComposeDeleteRequest(this Resource resource, Uri baseResourceIdentifier)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            Uri resourceIdentifier = resource.GetResourceIdentifier(baseResourceIdentifier);

            HttpRequestMessage result = null;
            try
            {
                result = new HttpRequestMessage(HttpMethod.Delete, resourceIdentifier);
                return result;
            }
            catch
            {
                if (result != null)
                {
                    result.Dispose();
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    result = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                }

                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of 'this' parameter of an extension method")]
        public static HttpRequestMessage ComposeGetRequest(
            this Schematized schematized,
            Uri baseResourceIdentifier,
            IReadOnlyCollection<IFilter> filters,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths,
            IPaginationParameters paginationParameters)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            if (null == filters)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            if (null == requestedAttributePaths)
            {
                throw new ArgumentNullException(nameof(requestedAttributePaths));
            }

            if (null == excludedAttributePaths)
            {
                throw new ArgumentNullException(nameof(excludedAttributePaths));
            }

            Uri resourceIdentifier =
                schematized.ComposeResourceIdentifier(
                    baseResourceIdentifier,
                    filters,
                    requestedAttributePaths,
                    excludedAttributePaths,
                    paginationParameters);
            HttpRequestMessage result = null;
            try
            {
                result = new HttpRequestMessage(HttpMethod.Get, resourceIdentifier);
                return result;
            }
            catch
            {
                if (result != null)
                {
                    result.Dispose();
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    result = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                }

                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of 'this' parameter of an extension method")]
        public static HttpRequestMessage ComposeGetRequest(
            this Schematized schematized,
            Uri baseResourceIdentifier,
            IReadOnlyCollection<IFilter> filters,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths)
        {
            HttpRequestMessage result = null;
            try
            {
                result =
                    schematized
                    .ComposeGetRequest(
                        baseResourceIdentifier,
                        filters,
                        requestedAttributePaths,
                        excludedAttributePaths,
                        null);
                return result;
            }
            catch
            {
                if (result != null)
                {
                    result.Dispose();
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    result = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                }

                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of 'this' parameter of an extension method")]
        public static HttpRequestMessage ComposeGetRequest(
            this Resource resource,
            Uri baseResourceIdentifier,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            if (null == requestedAttributePaths)
            {
                throw new ArgumentNullException(nameof(requestedAttributePaths));
            }

            if (null == excludedAttributePaths)
            {
                throw new ArgumentNullException(nameof(excludedAttributePaths));
            }

            Uri resourceIdentifier =
                resource.ComposeResourceIdentifier(
                    baseResourceIdentifier,
                    requestedAttributePaths,
                    excludedAttributePaths);
            HttpRequestMessage result = null;
            try
            {
                result = new HttpRequestMessage(HttpMethod.Get, resourceIdentifier);
                return result;
            }
            catch
            {
                if (result != null)
                {
                    result.Dispose();
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    result = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                }

                throw;
            }
        }

        public static HttpRequestMessage ComposeGetRequest(this Resource resource, Uri baseResourceIdentifier)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            HttpRequestMessage result = null;
            try
            {
                IReadOnlyCollection<string> requestedAttributePaths = Array.Empty<string>();
                IReadOnlyCollection<string> excludedAttributePaths = Array.Empty<string>();
                result = resource.ComposeGetRequest(baseResourceIdentifier, requestedAttributePaths, excludedAttributePaths);
                return result;
            }
            catch
            {
                if (result != null)
                {
                    result.Dispose();
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    result = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                }

                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "The parameter must be a patch for the operation to produce a semantically valid result")]
        public static HttpRequestMessage ComposePatchRequest(
            this Resource resource,
            Uri baseResourceIdentifier,
            PatchRequestBase patch)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            if (null == patch)
            {
                throw new ArgumentNullException(nameof(patch));
            }

            Dictionary<string, object> json = patch.ToJson();

            Uri resourceIdentifier = resource.GetResourceIdentifier(baseResourceIdentifier);

            HttpRequestMessage result = null;
            try
            {
                HttpContent requestContent = null;
                try
                {
                    string contentType = MediaTypes.Protocol;

                    MediaTypeFormatter contentFormatter = new JsonMediaTypeFormatter();
                    requestContent =
                        new ObjectContent<Dictionary<string, object>>(
                            json,
                            contentFormatter,
                            contentType);
                    result = new HttpRequestMessage(ProtocolExtensions.PatchMethod, resourceIdentifier);
                    result.Content = requestContent;
                    requestContent = null;
                    return result;
                }
                finally
                {
                    if (requestContent != null)
                    {
                        requestContent.Dispose();
                        requestContent = null;
                    }
                }
            }
            catch
            {
                if (result != null)
                {
                    result.Dispose();
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    result = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                }

                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of 'this' parameter of an extension method")]
        public static HttpRequestMessage ComposePatchRequest(
            this Resource patch,
            Uri baseResourceIdentifier)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            Dictionary<string, object> json = patch.ToJson();
            json.Trim();

            Uri resourceIdentifier = patch.GetResourceIdentifier(baseResourceIdentifier);

            HttpRequestMessage result = null;
            try
            {
                HttpContent requestContent = null;
                try
                {
                    MediaTypeFormatter contentFormatter = new JsonMediaTypeFormatter();
                    requestContent =
                        new ObjectContent<Dictionary<string, object>>(
                            json,
                            contentFormatter,
                            MediaTypes.Json);
                    result = new HttpRequestMessage(ProtocolExtensions.PatchMethod, resourceIdentifier);
                    result.Content = requestContent;
                    requestContent = null;
                    return result;
                }
                finally
                {
                    if (requestContent != null)
                    {
                        requestContent.Dispose();
                        requestContent = null;
                    }
                }
            }
            catch
            {
                if (result != null)
                {
                    result.Dispose();
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    result = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                }

                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of extension method")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Performing the operation on the base type would be invalid")]
        public static HttpRequestMessage ComposePutRequest(this Resource resource, Uri baseResourceIdentifier)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            string contentType = MediaTypes.Protocol;

            Dictionary<string, object> json = resource.ToJson();
            json.Trim();

            Uri resourceIdentifier = resource.GetResourceIdentifier(baseResourceIdentifier);

            HttpRequestMessage result = null;
            try
            {
                HttpContent requestContent = null;
                try
                {
                    MediaTypeFormatter contentFormatter = new JsonMediaTypeFormatter();
                    requestContent =
                        new ObjectContent<Dictionary<string, object>>(
                            json,
                            contentFormatter,
                            contentType);
                    result = new HttpRequestMessage(HttpMethod.Put, resourceIdentifier);
                    result.Content = requestContent;
                    requestContent = null;
                    return result;
                }
                finally
                {
                    if (requestContent != null)
                    {
                        requestContent.Dispose();
                        requestContent = null;
                    }
                }
            }
            catch
            {
                if (result != null)
                {
                    result.Dispose();
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    result = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                }

                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of extension method")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Performing the operation on the base type would be invalid")]
        public static HttpRequestMessage ComposePostRequest(this Resource resource, Uri baseResourceIdentifier)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            string contentType = MediaTypes.Protocol;

            Dictionary<string, object> json = resource.ToJson();
            json.Trim();

            Uri typeResourceIdentifier = resource.GetTypeIdentifier(baseResourceIdentifier);

            HttpRequestMessage result = null;
            try
            {
                HttpContent requestContent = null;
                try
                {
                    MediaTypeFormatter contentFormatter = new JsonMediaTypeFormatter();
                    requestContent =
                        new ObjectContent<Dictionary<string, object>>(
                            json,
                            contentFormatter,
                            contentType);
                    result = new HttpRequestMessage(HttpMethod.Post, typeResourceIdentifier);
                    result.Content = requestContent;
                    requestContent = null;
                    return result;
                }
                finally
                {
                    if (requestContent != null)
                    {
                        requestContent.Dispose();
                        requestContent = null;
                    }
                }
            }
            catch
            {
                if (result != null)
                {
                    result.Dispose();
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    result = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                }

                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of 'this' parameter of an extension method")]
        public static UriBuilder ComposeResourceIdentifier(this Resource resource, Uri baseResourceIdentifier)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            if (string.IsNullOrWhiteSpace(resource.Identifier))
            {
                throw new InvalidOperationException(SystemForCrossDomainIdentityManagementProtocolResources.ExceptionInvalidResource);
            }

            Uri foundation = resource.GetResourceIdentifier(baseResourceIdentifier);
            UriBuilder result = new UriBuilder(foundation);
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of 'this' parameter of an extension method")]
        public static Uri ComposeResourceIdentifier(
            this Resource resource,
            Uri baseResourceIdentifier,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            if (null == requestedAttributePaths)
            {
                throw new ArgumentNullException(nameof(requestedAttributePaths));
            }

            if (null == excludedAttributePaths)
            {
                throw new ArgumentNullException(nameof(excludedAttributePaths));
            }

            if (!resource.TryGetSchemaIdentifier(out string schemaIdentifier))
            {
                schemaIdentifier = resource.GetSchemaIdentifier();
            }

            if (!resource.TryGetPath(out string path))
            {
                path = resource.GetPath();
            }

            IResourceRetrievalParameters retrievalParameters =
                new ResourceRetrievalParameters(
                    schemaIdentifier,
                    path,
                    resource.Identifier,
                    requestedAttributePaths,
                    excludedAttributePaths);
            string query = retrievalParameters.ToString();
            UriBuilder resourceIdentifier = resource.ComposeResourceIdentifier(baseResourceIdentifier);
            resourceIdentifier.Query = query;
            Uri result = resourceIdentifier.Uri;
            return result;
        }

        public static Uri ComposeResourceIdentifier(
            this Schematized schematized,
            Uri baseResourceIdentifier,
            IQueryParameters parameters)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            if (null == parameters)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            Uri typeIdentifier = schematized.GetTypeIdentifier(baseResourceIdentifier);
            UriBuilder resourceIdentifier = new UriBuilder(typeIdentifier);
            resourceIdentifier.Query = parameters.ToString();
            Uri result = resourceIdentifier.Uri;
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of an extension method")]
        public static Uri ComposeResourceIdentifier(
            this Schematized schematized,
            Uri baseResourceIdentifier,
            IReadOnlyCollection<IFilter> filters,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths,
            IPaginationParameters paginationParameters)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            if (null == filters)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            if (null == requestedAttributePaths)
            {
                throw new ArgumentNullException(nameof(requestedAttributePaths));
            }

            if (null == excludedAttributePaths)
            {
                throw new ArgumentNullException(nameof(excludedAttributePaths));
            }

            if (!schematized.TryGetSchemaIdentifier(out string schemaIdentifier))
            {
                schemaIdentifier = schematized.GetSchemaIdentifier();
            }

            if (!schematized.TryGetPath(out string path))
            {
                path = schematized.GetPath();
            }

            IQueryParameters queryParameters =
                new QueryParameters(
                    schemaIdentifier,
                    path,
                    filters,
                    requestedAttributePaths,
                    excludedAttributePaths);
            queryParameters.PaginationParameters = paginationParameters;
            Uri result = schematized.ComposeResourceIdentifier(baseResourceIdentifier, queryParameters);
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of an extension method")]
        public static Uri ComposeResourceIdentifier(
            this Schematized schematized,
            Uri baseResourceIdentifier,
            IReadOnlyCollection<IFilter> filters,
            IReadOnlyCollection<string> requestedAttributePaths,
            IReadOnlyCollection<string> excludedAttributePaths)
        {
            Uri result =
                schematized.ComposeResourceIdentifier(
                    baseResourceIdentifier,
                    filters,
                    requestedAttributePaths,
                    excludedAttributePaths,
                    null);
            return result;
        }

        private static Uri ComposeTypeIdentifier(Uri baseResourceIdentifier, string path)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            if (null == path)
            {
                throw new ArgumentNullException(nameof(path));
            }

            string baseResourceIdentifierValue = baseResourceIdentifier.ToString();
            string resultValue =
                baseResourceIdentifierValue +
                SchemaConstants.PathInterface +
                ServiceConstants.SeparatorSegments +
                path;

            Uri result = new Uri(resultValue);
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of 'this' parameter of an extension method")]
        public static IResourceIdentifier GetIdentifier(this Resource resource)
        {
            if (!resource.TryGetSchemaIdentifier(out string schemaIdentifier))
            {
                schemaIdentifier = resource.GetSchemaIdentifier();
            }

            IResourceIdentifier result = new ResourceIdentifier(schemaIdentifier, resource.Identifier);
            return result;
        }

        private static string GetPath(this Schematized schematized)
        {
            if (schematized.TryGetPath(out string path))
            {
                return path;
            }

            if (schematized.Is(SchemaIdentifiers.Core2EnterpriseUser))
            {
                return ProtocolConstants.PathUsers;
            }

            if (schematized.Is(SchemaIdentifiers.Core2User))
            {
                return ProtocolConstants.PathUsers;
            }

            if (schematized.Is(SchemaIdentifiers.Core2Group))
            {
                return ProtocolConstants.PathGroups;
            }

            switch (schematized)
            {
                case UserBase _:
                    return ProtocolConstants.PathUsers;
                case GroupBase _:
                    return ProtocolConstants.PathGroups;
                default:
                    string unsupportedTypeName = schematized.GetType().FullName;
                    throw new NotSupportedException(unsupportedTypeName);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of 'this' parameter of an extension method")]
        public static Uri GetResourceIdentifier(this Resource resource, Uri baseResourceIdentifier)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            if (string.IsNullOrWhiteSpace(resource.Identifier))
            {
                throw new InvalidOperationException(SystemForCrossDomainIdentityManagementProtocolResources.ExceptionInvalidResource);
            }

            if (resource.TryGetIdentifier(baseResourceIdentifier, out Uri result))
            {
                return result;
            }

            Uri typeResource = resource.GetTypeIdentifier(baseResourceIdentifier);
            string escapedIdentifier = Uri.EscapeDataString(resource.Identifier);
            string resultValue =
                typeResource.ToString() +
                ServiceConstants.SeparatorSegments + 
                escapedIdentifier;
            result = new Uri(resultValue);
            return result;
        }

        private static string GetSchemaIdentifier(IReadOnlyCollection<string> schemaIdentifiers)
        {
            if (null == schemaIdentifiers)
            {
                throw new ArgumentNullException(nameof(schemaIdentifiers));
            }

            if (!schemaIdentifiers.Any())
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementProtocolResources.ExceptionUnidentifiableSchema);
            }

            foreach (string schema in schemaIdentifiers)
            {
                switch (schema)
                {
                    case SchemaIdentifiers.Core2User:
                    case SchemaIdentifiers.Core2EnterpriseUser:
                        return SchemaIdentifiers.Core2EnterpriseUser;
                    case SchemaIdentifiers.Core2Group:
                        return SchemaIdentifiers.Core2Group;
                }
            }

            string schemas = string.Join(Environment.NewLine, schemaIdentifiers);
            throw new NotSupportedException(schemas);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of 'this' parameter of an extension method")]
        public static string GetSchemaIdentifier(this Schematized schematized)
        {
            if (!schematized.TryGetSchemaIdentifier(out string result))
            {
                result = ProtocolExtensions.GetSchemaIdentifier(schematized.Schemas);
            }
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of 'this' parameter of an extension method")]
        public static Uri GetTypeIdentifier(this Schematized schematized, Uri baseResourceIdentifier)
        {
            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            if (null == schematized.Schemas)
            {
                throw new InvalidOperationException(SystemForCrossDomainIdentityManagementProtocolResources.ExceptionInvalidResource);
            }

            Uri result;
            string path = schematized.GetPath();
            result = ProtocolExtensions.ComposeTypeIdentifier(baseResourceIdentifier, path);

            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of 'this' parameter of an extension method")]
        public static bool Matches(this IExtension extension, string schemaIdentifier)
        {
            bool result = string.Equals(schemaIdentifier, extension.SchemaIdentifier, StringComparison.OrdinalIgnoreCase);
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "None")]
        internal static IEnumerable<ElectronicMailAddress> PatchElectronicMailAddresses(
            IEnumerable<ElectronicMailAddress> electronicMailAddresses,
            PatchOperation2 operation)
        {
            if (null == operation)
            {
                return electronicMailAddresses;
            }

            if
            (
                !string.Equals(
                    AttributeNames.ElectronicMailAddresses,
                    operation.Path.AttributePath,
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                return electronicMailAddresses;
            }

            if (null == operation.Path.ValuePath)
            {
                return electronicMailAddresses;
            }

            if (string.IsNullOrWhiteSpace(operation.Path.ValuePath.AttributePath))
            {
                return electronicMailAddresses;
            }

            IFilter subAttribute = operation.Path.SubAttributes.SingleOrDefault();
            if (null == subAttribute)
            {
                return electronicMailAddresses;
            }

            if
            (
                    (
                            operation.Value != null
                        && operation.Value.Count != 1
                    )
                || (
                            null == operation.Value
                        && operation.Name != OperationName.Remove
                    )
            )
            {
                return electronicMailAddresses;
            }

            if
            (
                !string.Equals(
                    Microsoft.SCIM.AttributeNames.Type,
                    subAttribute.AttributePath,
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                return electronicMailAddresses;
            }

            string electronicMailAddressType = subAttribute.ComparisonValue;
            if
            (
                    !string.Equals(electronicMailAddressType, ElectronicMailAddress.Home, StringComparison.Ordinal)
                && !string.Equals(electronicMailAddressType, ElectronicMailAddress.Work, StringComparison.Ordinal)
            )
            {
                return electronicMailAddresses;
            }

            ElectronicMailAddress electronicMailAddress;
            ElectronicMailAddress electronicMailAddressExisting;
            if (electronicMailAddresses != null)
            {
                electronicMailAddressExisting =
                    electronicMailAddress =
                        electronicMailAddresses
                        .SingleOrDefault(
                            (ElectronicMailAddress item) =>
                                string.Equals(subAttribute.ComparisonValue, item.ItemType, StringComparison.Ordinal));
            }
            else
            {
                electronicMailAddressExisting = null;
                electronicMailAddress =
                    new ElectronicMailAddress()
                    {
                        ItemType = electronicMailAddressType
                    };
            }

            string value = operation.Value?.Single().Value;
            if
            (
                    value != null
                && OperationName.Remove == operation.Name
                && string.Equals(value, electronicMailAddress.Value, StringComparison.OrdinalIgnoreCase)
            )
            {
                value = null;
            }
            electronicMailAddress.Value = value;

            IEnumerable<ElectronicMailAddress> result;
            if (string.IsNullOrWhiteSpace(electronicMailAddress.Value))
            {
                if (electronicMailAddressExisting != null)
                {
                    result =
                        electronicMailAddresses
                        .Where(
                            (ElectronicMailAddress item) =>
                                !string.Equals(subAttribute.ComparisonValue, item.ItemType, StringComparison.Ordinal))
                        .ToArray();
                }
                else
                {
                    result = electronicMailAddresses;
                }
                return result;
            }

            if (electronicMailAddressExisting != null)
            {
                return electronicMailAddresses;
            }

            result =
                new ElectronicMailAddress[]
                    {
                        electronicMailAddress
                    };
            if (null == electronicMailAddresses)
            {
                return result;
            }

            result = electronicMailAddresses.Union(electronicMailAddresses).ToArray();
            return result;
        }

        internal static IEnumerable<Role> PatchRoles(IEnumerable<Role> roles, PatchOperation2 operation)
        {
            if (null == operation)
            {
                return roles;
            }

            if
            (
                !string.Equals(
                    Microsoft.SCIM.AttributeNames.Roles,
                    operation.Path.AttributePath,
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                return roles;
            }

            if (null == operation.Path.ValuePath)
            {
                return roles;
            }

            if (string.IsNullOrWhiteSpace(operation.Path.ValuePath.AttributePath))
            {
                return roles;
            }

            IFilter subAttribute = operation.Path.SubAttributes.SingleOrDefault();
            if (null == subAttribute)
            {
                return roles;
            }

            if
            (
                    (
                           operation.Value != null
                        && operation.Value.Count != 1
                    )
                || (
                            null == operation.Value
                        && operation.Name != OperationName.Remove
                    )
            )
            {
                return roles;
            }

            Role role;
            Role roleExisting;
            if (roles != null)
            {
                roleExisting =
                    role =
                        roles
                        .SingleOrDefault(
                            (Role item) =>
                                string.Equals(subAttribute.ComparisonValue, item.ItemType, StringComparison.Ordinal));
            }
            else
            {
                roleExisting = null;
                role =
                    new Role()
                    {
                        Primary = true
                    };
            }

            string value = operation.Value?.Single().Value;
            if
            (
                    value != null
                && OperationName.Remove == operation.Name
                && string.Equals(value, role.Value, StringComparison.OrdinalIgnoreCase)
            )
            {
                value = null;
            }
            role.Value = value;

            IEnumerable<Role> result;
            if (string.IsNullOrWhiteSpace(role.Value))
            {
                if (roleExisting != null)
                {
                    result =
                        roles
                        .Where(
                            (Role item) =>
                                !string.Equals(subAttribute.ComparisonValue, item.ItemType, StringComparison.Ordinal))
                        .ToArray();
                }
                else
                {
                    result = roles;
                }
                return result;
            }

            if (roleExisting != null)
            {
                return roles;
            }

            result =
                new Role[]
                    {
                        role
                    };

            if (null == roles)
            {
                return result;
            }

            result = roles.Union(roles).ToArray();
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "resourceIdentifier", Justification = "False analysis of extension method")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of 'this' parameter of an extension method")]
#pragma warning disable IDE0060 // Remove unused parameter
        public static Uri Serialize(this IResourceIdentifier resourceIdentifier, Resource resource, Uri baseResourceIdentifier)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            if (null == resource)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (null == baseResourceIdentifier)
            {
                throw new ArgumentNullException(nameof(baseResourceIdentifier));
            }

            Uri typeResource = resource.GetTypeIdentifier(baseResourceIdentifier);
            string escapedIdentifier = Uri.EscapeDataString(resource.Identifier);
            string resultValue =
                typeResource.ToString() +
                ServiceConstants.SeparatorSegments +
                escapedIdentifier;

            Uri result = new Uri(resultValue);
            return result;
        }

        public static async Task<string> SerializeAsync(this HttpRequestMessage request, bool acceptLargeObjects)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            StringBuilder buffer = new StringBuilder();
            TextWriter textWriter = null;
            try
            {

#pragma warning disable CA2000 // Dispose objects before losing scope
                textWriter = new StringWriter(buffer);
#pragma warning restore CA2000 // Dispose objects before losing scope

                IHttpRequestMessageWriter requestWriter = null;
                try
                {
                    requestWriter = new HttpRequestMessageWriter(request, textWriter, acceptLargeObjects);
                    textWriter = null;
                    await requestWriter.WriteAsync().ConfigureAwait(false);
                    await requestWriter.FlushAsync().ConfigureAwait(false);
                    string result = buffer.ToString();
                    return result;
                }
                finally
                {
                    if (requestWriter != null)
                    {
                        requestWriter.Close();
                        requestWriter = null;
                    }
                }
            }
            finally
            {
                if (textWriter != null)
                {
                    textWriter.Flush();
                    textWriter.Close();
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                    textWriter = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                }
            }
        }

        public static async Task<string> SerializeAsync(this HttpRequestMessage request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string result = await request.SerializeAsync(false).ConfigureAwait(false);
            return result;
        }

        public static IReadOnlyCollection<T> ToCollection<T>(this IEnumerable enumerable)
        {
            if (null == enumerable)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            IList<T> list = new List<T>();
            foreach (object item in enumerable)
            {
                T typed = (T)item;
                list.Add(typed);
            }
            IReadOnlyCollection<T> result = list.ToArray();
            return result;
        }

        public static IReadOnlyCollection<T> ToCollection<T>(this ArrayList array)
        {
            if (null == array)
            {
                throw new ArgumentNullException(nameof(array));
            }

            IList<T> list = new List<T>(array.Count);
            foreach (object item in array)
            {
                T typed = (T)item;
                list.Add(typed);
            }
            IReadOnlyCollection<T> result = list.ToArray();
            return result;
        }

        public static IReadOnlyCollection<T> ToCollection<T>(this T item)
        {
            IReadOnlyCollection<T> result =
                new T[]
                    {
                        item
                    };
            return result;
        }

        private static bool TryMatch(
            IReadOnlyCollection<string> schemaIdentifiers,
            IReadOnlyCollection<IExtension> extensions,
            out IExtension matchingExtension)
        {
            matchingExtension = null;

            if (null == extensions)
            {
                return false;
            }

            if (null == schemaIdentifiers)
            {
                return false;
            }

            foreach (IExtension extension in extensions)
            {
                foreach (string schemaIdentifier in schemaIdentifiers)
                {
                    if (extension.Matches(schemaIdentifier))
                    {
                        matchingExtension = extension;
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool TryMatch(
            this IReadOnlyCollection<IExtension> extensions,
            IReadOnlyCollection<string> schemaIdentifiers,
            out IExtension matchingExtension)
        {
            bool result = ProtocolExtensions.TryMatch(schemaIdentifiers, extensions, out matchingExtension);
            return result;
        }

        public static bool TryMatch(
            this IReadOnlyCollection<IExtension> extensions,
            string schemaIdentifier,
            out IExtension matchingExtension)
        {
            if (string.IsNullOrWhiteSpace(schemaIdentifier))
            {
                matchingExtension = null;
                return false;
            }
            IReadOnlyCollection<string> schemaIdentifiers = schemaIdentifier.ToCollection();
            bool result = extensions.TryMatch(schemaIdentifiers, out matchingExtension);
            return result;
        }

        public static bool References(this PatchRequest2Base<PatchOperation2Combined> patch, string referee)
        {
            if (null == patch)
            {
                throw new ArgumentNullException(nameof(patch));
            }

            if (string.IsNullOrWhiteSpace(referee))
            {
                throw new ArgumentNullException(nameof(referee));
            }

            bool result = patch.TryFindReference(referee, out IReadOnlyCollection<OperationValue> _);
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "False analysis of the 'this' parameter of an extension method")]
        public static bool TryFindReference(
            this PatchRequest2Base<PatchOperation2Combined> patch,
            string referee,
            out IReadOnlyCollection<OperationValue> references)
        {
            if (null == patch)
            {
                throw new ArgumentNullException(nameof(patch));
            }

            references = null;

            if (string.IsNullOrWhiteSpace(referee))
            {
                throw new ArgumentNullException(nameof(referee));
            }

            List<OperationValue> patchOperation2Values = new List<OperationValue>();

            foreach (PatchOperation2Combined operation in patch.Operations)
            {
                OperationValue[] values = null;
                if (operation?.Value != null)
                {
                    values =
                    JsonConvert.DeserializeObject<OperationValue[]>(
                        operation.Value,
                        ProtocolConstants.JsonSettings.Value);
                }

                if (values == null)
                {
                    string value = null;
                    if (operation?.Value != null)
                    {
                        value = JsonConvert.DeserializeObject<string>(operation.Value, ProtocolConstants.JsonSettings.Value);
                    }

                    OperationValue valueSingle = new OperationValue()
                    {
                        Value = value
                    };
                    patchOperation2Values.Add(valueSingle);
                }
                else
                {
                    foreach (OperationValue value in values)
                    {
                        patchOperation2Values.Add(value);
                    }
                }

            }

            IReadOnlyCollection<OperationValue> patchOperationValues = patchOperation2Values.AsReadOnly();

            IList<OperationValue> referencesBuffer = new List<OperationValue>(patchOperationValues.Count);
            foreach (OperationValue patchOperationValue in patchOperationValues)
            {
                if (!patchOperationValue.TryParseBulkIdentifierReferenceValue(out string value))
                {
                    value = patchOperationValue.Value;
                }

                if (string.Equals(referee, value,StringComparison.InvariantCulture))
                {
                    referencesBuffer.Add(patchOperationValue);
                }
            }

            references = referencesBuffer.ToArray();
            bool result = references.Any();
            return result;
        }

        private static bool TryParseBulkIdentifierReferenceValue(string value, out string bulkIdentifier)
        {
            bulkIdentifier = null;

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            Match match = ProtocolExtensions.BulkIdentifierExpression.Value.Match(value);
            bool result = match.Success;
            if (result)
            {
                bulkIdentifier = match.Groups[ProtocolExtensions.ExpressionGroupNameBulkIdentifier].Value;
            }

            return result;
        }

        public static bool TryParseBulkIdentifierReferenceValue(this OperationValue value, out string bulkIdentifier)
        {
            bulkIdentifier = null;

            if (null == value)
            {
                return false;
            }

            bool result = ProtocolExtensions.TryParseBulkIdentifierReferenceValue(value.Value, out bulkIdentifier);
            return result;
        }


        private sealed class HttpRequestMessageWriter : IHttpRequestMessageWriter
        {
            private const string TemplateHeader = "{0}: {1}";

            private readonly object thisLock = new object();

            private TextWriter innerWriter;

            public HttpRequestMessageWriter(HttpRequestMessage message, TextWriter writer, bool acceptLargeObjects)
            {
                this.Message = message ?? throw new ArgumentNullException(nameof(message));
                this.innerWriter = writer ?? throw new ArgumentNullException(nameof(writer));
                this.AcceptLargeObjects = acceptLargeObjects;
            }

            private bool AcceptLargeObjects
            {
                get;
            }

            private HttpRequestMessage Message
            {
                get;
                set;
            }

            public void Close()
            {
                this.innerWriter.Flush();
                this.innerWriter.Close();
            }

            public void Dispose()
            {
                if (this.innerWriter != null)
                {
                    lock (this.thisLock)
                    {
                        if (this.innerWriter != null)
                        {
                            this.Close();
                            this.innerWriter = null;
                        }
                    }
                }
            }

            public async Task FlushAsync()
            {
                await this.innerWriter.FlushAsync().ConfigureAwait(false);
            }

            public async Task WriteAsync()
            {
                if (this.Message.RequestUri != null)
                {
                    string line = HttpUtility.UrlDecode(this.Message.RequestUri.AbsoluteUri);
                    await this.innerWriter.WriteLineAsync(line).ConfigureAwait(false);
                }

                if (this.Message.Headers != null)
                {
                    foreach (KeyValuePair<string, IEnumerable<string>> header in this.Message.Headers)
                    {
                        if (!header.Value.Any())
                        {
                            continue;
                        }

                        string value;
                        if (1 == header.Value.LongCount())
                        {
                            value = header.Value.Single();
                        }
                        else
                        {
                            string[] values = header.Value.ToArray();
                            value = JsonFactory.Instance.Create(values, this.AcceptLargeObjects);
                        }

                        string line =
                            string.Format(
                                CultureInfo.InvariantCulture,
                                HttpRequestMessageWriter.TemplateHeader,
                                header.Key,
                                value);
                        await this.innerWriter.WriteLineAsync(line).ConfigureAwait(false);
                    }
                }

                if (this.Message.Content != null)
                {
                    string line = await this.Message.Content.ReadAsStringAsync().ConfigureAwait(false);
                    await this.innerWriter.WriteLineAsync(line).ConfigureAwait(false);
                }
            }

        }
    }
}
