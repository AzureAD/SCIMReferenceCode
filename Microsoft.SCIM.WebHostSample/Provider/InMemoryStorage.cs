// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM.WebHostSample.Provider
{
    using System;
    using System.Collections.Generic;
    using Microsoft.SCIM;

    public class InMemoryStorage
    {
        internal readonly IDictionary<string, Core2Group> Groups;
        internal readonly IDictionary<string, Core2EnterpriseUser> Users;

        private InMemoryStorage()
        {
            this.Groups = new Dictionary<string, Core2Group>();
            this.Users = new Dictionary<string, Core2EnterpriseUser>();
        }

        private static readonly Lazy<InMemoryStorage> InstanceValue =
                                new Lazy<InMemoryStorage>(
                                        () =>
                                            new InMemoryStorage());

        public static InMemoryStorage Instance => InMemoryStorage.InstanceValue.Value;
    }
}
