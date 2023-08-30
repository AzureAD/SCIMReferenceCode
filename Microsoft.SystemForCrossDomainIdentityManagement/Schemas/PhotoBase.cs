// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Runtime.Serialization;

    [DataContract]
    public class PhotoBase : TypedValue
    {
        internal PhotoBase()
        {
        }

        public const string Photo = "photo";
        public const string Thumbnail = "thumbnail";
    }
}
