//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Deserializer", Justification = "False analysis")]
    public interface IGroupDeserializer
    {
        IResourceJsonDeserializingFactory<GroupBase> GroupDeserializationBehavior { get; set; }
    }
}
