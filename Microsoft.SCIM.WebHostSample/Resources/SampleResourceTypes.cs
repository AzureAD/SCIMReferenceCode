using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.SCIM.WebHostSample.Resources
{
    public class SampleResourceTypes
    {
        public static Core2ResourceType userResourceType
        {
            get
            {
                Core2ResourceType userResource = new Core2ResourceType();
                userResource.Identifier = SampleConstants.User;
                userResource.Endpoint = new Uri("http://localhost:58464/Scim/Users");
                userResource.Schema = SampleConstants.UserEnterpriseSchema;

                return userResource;
            }
        }

        public static Core2ResourceType groupResourceType
        {
            get
            {
                Core2ResourceType groupResource = new Core2ResourceType();
                groupResource.Identifier = SampleConstants.Group;
                groupResource.Endpoint = new Uri("http://localhost:58464/Scim/Groups");
                groupResource.Schema = SampleConstants.GroupSchema;

                return groupResource;
            }
        }
    }
}
