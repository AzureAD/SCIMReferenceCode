// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class InstantMessagingBase : TypedValue
    {
        public const string Aim = "aim";
        public const string Gtalk = "gtalk";
        public const string Icq = "icq";
        public const string Msn = "msn";
        public const string Qq = "qq";
        public const string Skype = "skype";
        public const string Xmpp = "xmpp";
        public const string Yahoo = "yahoo";

        internal InstantMessagingBase()
        {
        }
    }
}