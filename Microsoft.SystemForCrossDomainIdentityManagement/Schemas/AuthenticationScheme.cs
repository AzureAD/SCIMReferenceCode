//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class AuthenticationScheme
    {
        private const string AuthenticationTypeResourceValueOpenStandardForAuthenticationBearerToken =
            "oauthbearertoken";

        private const string DescriptionOpenStandardForAuthenticationBearerToken =
            "Authentication Scheme using the OAuth Bearer Token Standard";

        private const string DocumentationResourceValueOpenStandardForAuthenticationBearerToken =
            "http://example.com/help/oauth.html";

        private const string NameOpenStandardForAuthenticationBearerToken = "OAuth Bearer Token";

        private const string SpecificationResourceValueOpenStandardForAuthenticationBearerToken =
            "http://tools.ietf.org/html/draft-ietf-oauth-v2-bearer-01";

        private static readonly Lazy<Uri> DocumentationResourceOpenStandardForAuthenticationBearerToken =
            new Lazy<Uri>(
                () =>
                    new Uri(AuthenticationScheme.DocumentationResourceValueOpenStandardForAuthenticationBearerToken));

        private static readonly Lazy<Uri> SpecificationResourceOpenStandardForAuthenticationBearerToken =
            new Lazy<Uri>(
                () =>
                    new Uri(AuthenticationScheme.SpecificationResourceValueOpenStandardForAuthenticationBearerToken));

        [DataMember(Name = AttributeNames.Type)]
        public string AuthenticationType
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

        [DataMember(Name = AttributeNames.Documentation)]
        public Uri DocumentationResource
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Name)]
        public string Name
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Primary)]
        public bool Primary
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Specification)]
        public Uri SpecificationResource
        {
            get;
            set;
        }

        public static AuthenticationScheme CreateOpenStandardForAuthorizationBearerTokenScheme()
        {
            AuthenticationScheme result =
                new AuthenticationScheme()
                {
                    AuthenticationType =
                            AuthenticationScheme.AuthenticationTypeResourceValueOpenStandardForAuthenticationBearerToken,
                    Name =
                            AuthenticationScheme.NameOpenStandardForAuthenticationBearerToken,
                    Description =
                            AuthenticationScheme.DescriptionOpenStandardForAuthenticationBearerToken,
                    DocumentationResource =
                            AuthenticationScheme.DocumentationResourceOpenStandardForAuthenticationBearerToken.Value,
                    SpecificationResource =
                            AuthenticationScheme.SpecificationResourceOpenStandardForAuthenticationBearerToken.Value
                };
            return result;
        }
    }
}