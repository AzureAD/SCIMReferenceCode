// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;

    public interface IMonitor
    {
        void Inform(IInformationNotification notification);
        void Report(IExceptionNotification notification);
        void Warn(Notification<Exception> notification);
        void Warn(Notification<string> notification);
    }
}
