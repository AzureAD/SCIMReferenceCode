// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    public interface IInformationNotification : INotification<string>
    {
        bool Verbose { get; set; }
    }
}
