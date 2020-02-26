// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    public interface ISampleProvider
    {
        Core2Group SampleGroup { get; }
        PatchRequest2 SamplePatch { get; }
        Core2EnterpriseUser SampleResource { get; }
        Core2EnterpriseUser SampleUser { get; }
    }
}
