// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;

    public interface IExceptionNotification : INotification<Exception>
    {
        bool Critical { get; set; }
    }
}
