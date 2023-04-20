// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;

    public class SchemaIdentifier : ISchemaIdentifier
    {
        public SchemaIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            this.Value = value;
        }

        public string Value
        {
            get;
            private set;
        }

        public string FindPath()
        {
            if (!this.TryFindPath(out string result))
            {
                throw new NotSupportedException(this.Value);
            }

            return result;
        }

        public bool TryFindPath(out string path)
        {
            path = null;

            switch (this.Value)
            {
                case SchemaIdentifiers.Core2EnterpriseUser:
                case SchemaIdentifiers.Core2User:
                    path = ProtocolConstants.PathUsers;
                    return true;
                case SchemaIdentifiers.Core2Group:
                    path = ProtocolConstants.PathGroups;
                    return true;
                case SchemaIdentifiers.None:
                    path = SchemaConstants.PathInterface;
                    return true;
                default:
                    return false;
            }
        }
    }
}