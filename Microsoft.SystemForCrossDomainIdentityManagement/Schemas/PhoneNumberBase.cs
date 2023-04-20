// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class PhoneNumberBase : TypedValue
    {
        public const string Fax = "fax";
        public const string Home = "home";
        public const string Mobile = "mobile";
        public const string Other = "other";
        public const string Pager = "pager";
        public const string Work = "work";

        internal PhoneNumberBase()
        {
        }
    }
}
