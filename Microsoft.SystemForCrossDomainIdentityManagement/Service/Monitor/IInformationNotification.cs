//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    public interface IInformationNotification : INotification<string>
    {
        bool Verbose { get; set; }
    }
}