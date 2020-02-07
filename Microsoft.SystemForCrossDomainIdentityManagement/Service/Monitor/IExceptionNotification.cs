//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;

    public interface IExceptionNotification : INotification<Exception>
    {
        bool Critical { get; set; }
    }
}