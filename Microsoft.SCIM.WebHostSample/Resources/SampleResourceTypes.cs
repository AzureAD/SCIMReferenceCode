// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM.WebHostSample.Resources
{
    using System;

    public class SampleResourceTypes
    {
        public static Core2ResourceType UserResourceType
        {
            get
            {
                Core2ResourceType userResource = new Core2ResourceType
                {
                    Identifier = Types.User,
                    Endpoint = new Uri($"{SampleConstants.SampleScimEndpoint}/Users"),
                    Schema = SampleConstants.UserEnterpriseSchema
                };

                return userResource;
            }
        }

        public static Core2ResourceType GroupResourceType
        {
            get
            {
                Core2ResourceType groupResource = new Core2ResourceType
                {
                    Identifier = Types.Group,
                    Endpoint = new Uri($"{SampleConstants.SampleScimEndpoint}/Groups"),
                    Schema = $"{SampleConstants.Core2SchemaPrefix}{Types.Group}"
                };

                return groupResource;
            }
        }
    }
}
