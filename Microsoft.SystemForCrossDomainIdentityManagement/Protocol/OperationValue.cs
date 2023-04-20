// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Globalization;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class OperationValue
    {
        private const string Template = "{0} {1}";

        [DataMember(Name = ProtocolAttributeNames.Reference, Order = 0, IsRequired = false, EmitDefaultValue = false)]
        public string Reference
        {
            get;
            set;
        }

        [DataMember(Name = AttributeNames.Value, Order = 1, IsRequired = false, EmitDefaultValue = false)]
        public string Value
        {
            get;
            set;
        }

        public override string ToString()
        {
            string result =
                string.Format(
                    CultureInfo.InvariantCulture,
                    OperationValue.Template,
                    this.Value,
                    this.Reference)
                .Trim();
            return result;
        }
    }
}