//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces", Justification = "This interface is a public interface that serves to refine the type of the generic member relative to the base interface")]
    public interface IResourceJsonDeserializingFactory<TOutput> :
        ISchematizedJsonDeserializingFactory<TOutput>
        where TOutput : Resource
    {
    }
}