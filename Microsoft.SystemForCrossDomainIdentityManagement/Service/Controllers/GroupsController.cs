// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route(ServiceConstants.RouteGroups)]
    [Authorize]
    [ApiController]
    public sealed class GroupsController : ControllerTemplate<Core2Group>
    {
        public GroupsController(IProvider provider, IMonitor monitor)
            : base(provider, monitor)
        {
        }

        protected override IProviderAdapter<Core2Group> AdaptProvider(IProvider provider)
        {
            if (null == provider)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            IProviderAdapter<Core2Group> result =
                new Core2GroupProviderAdapter(provider);
            return result;
        }
    }
}
