// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Newtonsoft.Json;

    public sealed class SampleProvider : ProviderBase, ISampleProvider
    {
        public const string ElectronicMailAddressHome = "babs@jensen.org";
        public const string ElectronicMailAddressWork = "bjensen@example.com";

        public const string ExtensionAttributeEnterpriseUserCostCenter = "4130";
        public const string ExtensionAttributeEnterpriseUserDepartment = "Tour Operations";
        public const string ExtensionAttributeEnterpriseUserDivision = "Theme Park";
        public const string ExtensionAttributeEnterpriseUserEmployeeNumber = "701984";
        public const string ExtensionAttributeEnterpriseUserOrganization = "Universal Studios";

        public const string GroupName = "Creative & Skinning";
        public const string IdentifierGroup = "acbf3ae7-8463-4692-b4fd-9b4da3f908ce";
        public const string IdentifierRole = "DA3B77DF-F495-45C7-9AAC-EC083B99A9D3";
        public const string IdentifierUser = "2819c223-7f76-453a-919d-413861904646";
        public const string IdentifierExternal = "bjensen";

        public const int LimitPageSize = 6;

        public const string Locale = "en-Us";
        public const string ManagerDisplayName = "John Smith";
        public const string ManagerIdentifier = "26118915-6090-4610-87e4-49d8ca9f808d";
        private const string NameFamily = "Jensen";
        private const string NameFormatted = "Ms. Barbara J Jensen III";
        private const string NameGiven = "Barbara";
        private const string NameHonorificPrefix = "Ms.";
        private const string NameHonorificSuffix = "III";
        private const string NameUser = "bjensen";
        public const string PhotoValue = "https://photos.example.com/profilephoto/72930000000Ccne/F";
        public const string ProfileUrl = "https://login.example.com/bjensen";
        public const string RoleDescription = "Attends an educational institution";
        public const string RoleDisplay = "Student";
        public const string RoleValue = "student";
        public const string TimeZone = "America/Los_Angeles";
        public const string UserType = "Employee";

        private readonly ElectronicMailAddress sampleElectronicMailAddressHome;
        private readonly ElectronicMailAddress sampleElectronicMailAddressWork;

        private readonly IReadOnlyCollection<ElectronicMailAddress> sampleElectronicMailAddresses;
        private readonly Manager sampleManager;
        private readonly Name sampleName;
        private readonly OperationValue sampleOperationValue;
        private readonly PatchOperation2Combined sampleOperation;
        private readonly PatchRequest2 samplePatch;

        private readonly Core2Group sampleGroup;
        private readonly Core2EnterpriseUser sampleUser;

        private JsonDeserializingFactory<Schematized> jsonDeserializingFactory;

        public SampleProvider()
        {
            this.sampleElectronicMailAddressHome =
                new ElectronicMailAddress
                {
                    ItemType = ElectronicMailAddress.Home,
                    Value = SampleProvider.ElectronicMailAddressHome
                };

            this.sampleElectronicMailAddressWork =
                new ElectronicMailAddress
                {
                    ItemType = ElectronicMailAddressWork,
                    Primary = true,
                    Value = SampleProvider.ElectronicMailAddressWork
                };

            this.sampleElectronicMailAddresses =
                new ElectronicMailAddress[]
                    {
                        this.sampleElectronicMailAddressHome,
                        this.sampleElectronicMailAddressWork
                    };

            this.sampleManager =
                new Manager()
                {
                    Value = SampleProvider.ManagerIdentifier,
                };

            this.sampleName =
                new Name()
                {
                    FamilyName = SampleProvider.NameFamily,
                    Formatted = SampleProvider.NameFormatted,
                    GivenName = SampleProvider.NameGiven,
                    HonorificPrefix = SampleProvider.NameHonorificPrefix,
                    HonorificSuffix = SampleProvider.NameHonorificSuffix
                };

            this.sampleOperationValue =
                new OperationValue()
                {
                    Value = SampleProvider.IdentifierUser
                };

            this.sampleOperation = this.ConstructOperation();

            this.samplePatch = this.ConstructPatch();

            this.sampleUser =
                new Core2EnterpriseUser()
                {
                    Active = true,
                    ElectronicMailAddresses = this.sampleElectronicMailAddresses,
                    ExternalIdentifier = SampleProvider.IdentifierExternal,
                    Identifier = SampleProvider.IdentifierUser,
                    Name = this.sampleName,
                    UserName = SampleProvider.NameUser
                };

            ExtensionAttributeEnterpriseUser2 enterpriseExtensionAttributeEnterpriseUser2 =
                new ExtensionAttributeEnterpriseUser2()
                {
                    CostCenter = SampleProvider.ExtensionAttributeEnterpriseUserCostCenter,
                    Department = SampleProvider.ExtensionAttributeEnterpriseUserDepartment,
                    Division = SampleProvider.ExtensionAttributeEnterpriseUserDivision,
                    EmployeeNumber = SampleProvider.ExtensionAttributeEnterpriseUserEmployeeNumber,
                    Manager = this.sampleManager,
                    Organization = SampleProvider.ExtensionAttributeEnterpriseUserOrganization
                };

            this.SampleUser.EnterpriseExtension = enterpriseExtensionAttributeEnterpriseUser2;

            this.sampleGroup =
                new Core2Group()
                {
                    DisplayName = SampleProvider.GroupName,
                };
        }

        private JsonDeserializingFactory<Schematized> JsonDeserializingFactory
        {
            get
            {
                JsonDeserializingFactory<Schematized> result =
                    LazyInitializer.EnsureInitialized<JsonDeserializingFactory<Schematized>>(
                        ref this.jsonDeserializingFactory,
                        this.InitializeJsonDeserializingFactory);
                return result;
            }
        }

        public Core2Group SampleGroup
        {
            get
            {
                return this.sampleGroup;
            }
        }

        public PatchRequest2 SamplePatch
        {
            get
            {
                return this.samplePatch;
            }
        }

        public Core2EnterpriseUser SampleResource
        {
            get
            {
                return this.SampleUser;
            }
        }

        public Core2EnterpriseUser SampleUser
        {
            get
            {
                return this.sampleUser;
            }
        }

        public override Task<Resource> CreateAsync(Resource resource, string correlationIdentifier)
        {
            if (null == resource)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            resource.Identifier = SampleProvider.IdentifierUser;

            Task<Resource> result = Task.FromResult(resource);
            return result;
        }

        private PatchOperation2Combined ConstructOperation()
        {
            IPath path = Path.Create(AttributeNames.Members);
            PatchOperation2Combined result =
                new PatchOperation2Combined()
                {
                    Name = OperationName.Add,
                    Path = path
                };
            result.Value = JsonConvert.SerializeObject(this.sampleOperationValue);
            return result;
        }

        private PatchRequest2 ConstructPatch()
        {
            PatchRequest2 result = new PatchRequest2();
            result.AddOperation(this.sampleOperation);
            return result;
        }

        public override Task DeleteAsync(IResourceIdentifier resourceIdentifier, string correlationIdentifier)
        {
            if (null == resourceIdentifier)
            {
                throw new ArgumentNullException(nameof(resourceIdentifier));
            }

            Task result = Task.WhenAll();
            return result;
        }

        private static bool HasMember(IResourceIdentifier containerIdentifier, string memberAttributePath, string memberIdentifier)
        {
            if (null == containerIdentifier)
            {
                throw new ArgumentNullException(nameof(containerIdentifier));
            }

            if (string.IsNullOrWhiteSpace(memberAttributePath))
            {
                throw new ArgumentNullException(nameof(memberAttributePath));
            }

            if (string.IsNullOrWhiteSpace(memberIdentifier))
            {
                throw new ArgumentNullException(nameof(memberIdentifier));
            }

            if (string.IsNullOrWhiteSpace(containerIdentifier.Identifier))
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidIdentifier);
            }

            if (!string.Equals(memberAttributePath, AttributeNames.Members, StringComparison.Ordinal))
            {
                string exceptionMessage =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SystemForCrossDomainIdentityManagementServiceResources.ExceptionFilterAttributePathNotSupportedTemplate,
                        memberAttributePath);
                throw new NotSupportedException(exceptionMessage);
            }

            if (!string.Equals(SchemaIdentifiers.Core2Group, containerIdentifier.SchemaIdentifier, StringComparison.Ordinal))
            {
                throw new NotSupportedException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionFilterNotSupported);
            }

            bool result =
                    string.Equals(SampleProvider.IdentifierGroup, containerIdentifier.Identifier, StringComparison.OrdinalIgnoreCase)
                && string.Equals(SampleProvider.IdentifierUser, memberIdentifier, StringComparison.OrdinalIgnoreCase);

            return result;
        }

        private JsonDeserializingFactory<Schematized> InitializeJsonDeserializingFactory()
        {
            JsonDeserializingFactory<Schematized> result =
                new SchematizedJsonDeserializingFactory()
                {
                    Extensions = this.Extensions,
                    GroupDeserializationBehavior = this.GroupDeserializationBehavior,
                    PatchRequest2DeserializationBehavior = this.PatchRequestDeserializationBehavior,
                    UserDeserializationBehavior = this.UserDeserializationBehavior
                };
            return result;
        }

        public override async Task<QueryResponseBase> PaginateQueryAsync(IRequest<IQueryParameters> request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (null == request.Payload)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            if (string.IsNullOrWhiteSpace(request.CorrelationIdentifier))
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidRequest);
            }

            IReadOnlyCollection<Resource> resources = await this.QueryAsync(request).ConfigureAwait(false);
            QueryResponseBase result = new QueryResponse(resources);
            if (null == request.Payload.PaginationParameters)
            {
                result.TotalResults =
                    result.ItemsPerPage =
                        resources.Count;
            }

            return result;
        }

        private Resource[] Query(IQueryParameters parameters)
        {
            if (null == parameters)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (parameters.AlternateFilters.Count != 1)
            {
                throw new NotSupportedException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionFilterCount);
            }

            if (parameters.PaginationParameters != null)
            {
                string exceptionMessage =
                        string.Format(
                            CultureInfo.InvariantCulture,
                            SystemForCrossDomainIdentityManagementServiceResources.ExceptionPaginationIsNotSupportedTemplate,
                            parameters.SchemaIdentifier);
                throw new NotSupportedException(exceptionMessage);
            }

            IFilter filter = parameters.AlternateFilters.Single();
            if (filter.AdditionalFilter != null)
            {
                Resource[] result = SampleProvider.QueryMember(parameters, filter);
                return result;
            }
            else if (string.Equals(parameters.SchemaIdentifier, SchemaIdentifiers.Core2EnterpriseUser, StringComparison.OrdinalIgnoreCase))
            {
                Resource[] result = this.QueryUsers(parameters, filter);
                return result;
            }
            else if (string.Equals(parameters.SchemaIdentifier, SchemaIdentifiers.Core2Group, StringComparison.OrdinalIgnoreCase))
            {
                Resource[] result = this.QueryGroups(parameters, filter);
                return result;
            }
            else
            {
                string exceptionMessage =
                        string.Format(
                            CultureInfo.InvariantCulture,
                            SystemForCrossDomainIdentityManagementServiceResources.ExceptionFilterAttributePathNotSupportedTemplate,
                            filter.AttributePath);
                throw new NotSupportedException(exceptionMessage);
            }
        }

        public override Task<Resource[]> QueryAsync(IQueryParameters parameters, string correlationIdentifier)
        {
            Resource[] resources = this.Query(parameters);
            Task<Resource[]> result = Task.FromResult(resources);
            return result;
        }

        private Resource[] QueryGroups(IQueryParameters parameters, IFilter filter)
        {
            if (null == parameters)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (null == filter)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if
            (
                    null == parameters.ExcludedAttributePaths
                || !parameters.ExcludedAttributePaths.Any()
                || parameters.ExcludedAttributePaths.Count != 1
                || !parameters.ExcludedAttributePaths.Single().Equals(AttributeNames.Members, StringComparison.Ordinal)
            )
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionQueryNotSupported);
            }

            if (
                       !string.Equals(filter.AttributePath, AttributeNames.ExternalIdentifier, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(filter.AttributePath, AttributeNames.DisplayName, StringComparison.OrdinalIgnoreCase)
               )
            {
                string exceptionMessage =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SystemForCrossDomainIdentityManagementServiceResources.ExceptionFilterAttributePathNotSupportedTemplate,
                        filter.AttributePath);
                throw new NotSupportedException(exceptionMessage);
            }

            if (filter.FilterOperator != ComparisonOperator.Equals)
            {
                string exceptionMessage =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SystemForCrossDomainIdentityManagementServiceResources.ExceptionFilterOperatorNotSupportedTemplate,
                        filter.FilterOperator);
                throw new NotSupportedException(exceptionMessage);
            }

            Resource[] results;
            if (!string.Equals(filter.ComparisonValue, SampleProvider.GroupName, StringComparison.OrdinalIgnoreCase))
            {
                results = Enumerable.Empty<Resource>().ToArray();
            }
            else
            {
                results = this.sampleGroup.ToCollection().ToArray();
            }

            return results;
        }

        private static Resource[] QueryMember(IQueryParameters parameters, IFilter filter)
        {
            if (null == parameters)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (null == filter)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (null == filter.AdditionalFilter)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionQueryNotSupported);
            }

            Resource[] results = null;

            if (parameters.ExcludedAttributePaths != null && parameters.ExcludedAttributePaths.Any())
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionQueryNotSupported);
            }

            if (!string.Equals(parameters.SchemaIdentifier, SchemaIdentifiers.Core2Group, StringComparison.Ordinal))
            {
                throw new NotSupportedException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionQueryNotSupported);
            }

            if (null == parameters.RequestedAttributePaths || !parameters.RequestedAttributePaths.Any())
            {
                throw new NotSupportedException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionQueryNotSupported);
            }

            if (filter.AdditionalFilter.AdditionalFilter != null)
            {
                throw new NotSupportedException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionQueryNotSupported);
            }

            string selectedAttribute = parameters.RequestedAttributePaths.SingleOrDefault();
            if (string.IsNullOrWhiteSpace(selectedAttribute))
            {
                throw new NotSupportedException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionQueryNotSupported);
            }

            if (!string.Equals(selectedAttribute, AttributeNames.Identifier, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionQueryNotSupported);
            }

            IReadOnlyCollection<IFilter> filters =
                new IFilter[]
                        {
                            filter,
                            filter.AdditionalFilter
                        };

            IFilter filterIdentifier =
                filters
                .SingleOrDefault(
                    (IFilter item) =>
                        item.AttributePath.Equals(AttributeNames.Identifier, StringComparison.OrdinalIgnoreCase));
            if (null == filterIdentifier)
            {
                throw new NotSupportedException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionQueryNotSupported);
            }

            IFilter filterMembers =
                filters
                .SingleOrDefault(
                    (IFilter item) =>
                        item.AttributePath.Equals(AttributeNames.Members, StringComparison.OrdinalIgnoreCase));
            if (null == filterMembers)
            {
                throw new NotSupportedException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionQueryNotSupported);
            }

            IResourceIdentifier containerIdentifier =
                new ResourceIdentifier()
                {
                    SchemaIdentifier = parameters.SchemaIdentifier,
                    Identifier = filterIdentifier.ComparisonValue
                };

            if (!SampleProvider.HasMember(containerIdentifier, filterMembers.AttributePath, filterMembers.ComparisonValue))
            {
                results = Array.Empty<Resource>();
            }
            else
            {
                Resource container =
                    new Core2Group()
                    {
                        Identifier = containerIdentifier.Identifier
                    };
                results = container.ToCollection().ToArray();
            }

            return results;
        }

        private Resource[] QueryUsers(IQueryParameters parameters, IFilter filter)
        {
            if (null == parameters)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (null == filter)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (parameters.ExcludedAttributePaths != null && parameters.ExcludedAttributePaths.Any())
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionQueryNotSupported);
            }

            if
            (
                    !string.Equals(filter.AttributePath, AttributeNames.ExternalIdentifier, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(filter.AttributePath, AttributeNames.UserName, StringComparison.OrdinalIgnoreCase)
            )
            {
                string exceptionMessage =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SystemForCrossDomainIdentityManagementServiceResources.ExceptionFilterAttributePathNotSupportedTemplate,
                        filter.AttributePath);
                throw new NotSupportedException(exceptionMessage);
            }

            if (filter.FilterOperator != ComparisonOperator.Equals)
            {
                string exceptionMessage =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        SystemForCrossDomainIdentityManagementServiceResources.ExceptionFilterOperatorNotSupportedTemplate,
                        filter.FilterOperator);
                throw new NotSupportedException(exceptionMessage);
            }

            Resource[] results;
            if
            (
                   !string.Equals(filter.ComparisonValue, SampleProvider.IdentifierExternal, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(filter.ComparisonValue, this.SampleUser.UserName, StringComparison.OrdinalIgnoreCase)
            )
            {
                results = Enumerable.Empty<Resource>().ToArray();
            }
            else
            {
                results = this.SampleUser.ToCollection().ToArray();
            }

            return results;
        }

        public override Task<Resource> ReplaceAsync(Resource resource, string correlationIdentifier)
        {
            if (null == resource)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (null == resource.Identifier)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidResource);
            }

            if
            (
                    resource.Is(SchemaIdentifiers.Core2EnterpriseUser)
                && string.Equals(resource.Identifier, SampleProvider.IdentifierUser, StringComparison.OrdinalIgnoreCase)
            )
            {
                Task<Resource> result = Task.FromResult(resource);
                return result;
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        public override Task<Resource> RetrieveAsync(IResourceRetrievalParameters parameters, string correlationIdentifier)
        {
            if (null == parameters)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (null == parameters.ResourceIdentifier)
            {
                throw new ArgumentException(SystemForCrossDomainIdentityManagementServiceResources.ExceptionInvalidParameters);
            }

            Resource resource = null;
            if
            (
                    string.Equals(
                        parameters.ResourceIdentifier.SchemaIdentifier,
                        SchemaIdentifiers.Core2EnterpriseUser,
                        StringComparison.Ordinal)
                && string.Equals(
                        parameters.ResourceIdentifier.Identifier,
                        SampleProvider.IdentifierUser,
                        StringComparison.OrdinalIgnoreCase)
            )
            {
                resource = this.SampleUser;
            }

            Task<Resource> result = Task.FromResult(resource);
            return result;
        }

        public override Task UpdateAsync(IPatch patch, string correlationIdentifier)
        {
            if (null == patch)
            {
                throw new ArgumentNullException(nameof(patch));
            }

            Task result = Task.WhenAll();
            return result;
        }
    }
}
