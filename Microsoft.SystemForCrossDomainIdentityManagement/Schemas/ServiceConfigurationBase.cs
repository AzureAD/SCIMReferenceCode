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
    public abstract class ServiceConfigurationBase : Schematized
    {
        [DataMember(Name = AttributeNames.AuthenticationSchemes)]
        private List<AuthenticationScheme> authenticationSchemes;
        private IReadOnlyCollection<AuthenticationScheme> authenticationSchemesWrapper;

        private object thisLock;

        protected ServiceConfigurationBase()
        {
            this.OnInitialization();
            this.OnInitialized();
        }

        public IReadOnlyCollection<AuthenticationScheme> AuthenticationSchemes
        {
            get
            {
                return this.authenticationSchemesWrapper;
            }
        }

        [DataMember(Name = AttributeNames.Bulk)]
        public BulkRequestsFeature BulkRequests
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Documentation)]
        public Uri DocumentationResource
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.EntityTag)]
        public Feature EntityTags
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Filter)]
        public Feature Filtering
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.ChangePassword)]
        public Feature PasswordChange
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Patch)]
        public Feature Patching
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Sort)]
        public Feature Sorting
        {
            get;
            set;
        }

        public void AddAuthenticationScheme(AuthenticationScheme authenticationScheme)
        {
            if (null == authenticationScheme)
            {
                throw new ArgumentNullException(nameof(authenticationScheme));
            }

            if (string.IsNullOrWhiteSpace(authenticationScheme.Name))
            {
                throw new ArgumentException(
                    SystemForCrossDomainIdentityManagementSchemasResources.ExceptionUnnamedAuthenticationScheme);
            }

            Func<bool> containsFunction =
                new Func<bool>(
                        () =>
                            this
                            .authenticationSchemes
                            .Any(
                                (AuthenticationScheme item) =>
                                    string.Equals(item.Name, authenticationScheme.Name, StringComparison.OrdinalIgnoreCase)));


            if (!containsFunction())
            {
                lock (this.thisLock)
                {
                    if (!containsFunction())
                    {
                        this.authenticationSchemes.Add(authenticationScheme);
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
            this.authenticationSchemes = new List<AuthenticationScheme>();
        }

        private void OnInitialized()
        {
            this.authenticationSchemesWrapper = this.authenticationSchemes.AsReadOnly();
        }
    }
}