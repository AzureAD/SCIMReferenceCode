// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Deserializer", Justification = "False analysis")]
    public interface IUserDeserializer
    {
        IResourceJsonDeserializingFactory<Core2UserBase> UserDeserializationBehavior { get; set; }
    }
}
