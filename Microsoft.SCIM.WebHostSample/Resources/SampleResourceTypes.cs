// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM.WebHostSample.Resources
{
    using System;
    using System.Collections.Generic;
    
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
                    Schema = $"{SampleConstants.Core2SchemaPrefix}{Types.User}",
                    SchemaExtensions = new List<Core2SchemaExtensions>() {
                        new Core2SchemaExtensions()
                        {
                            Schema = SampleConstants.UserEnterpriseSchema,
                            Required = false
                        }
                    }
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
