// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class PatchOperation2SingleValued : PatchOperation2Base
    {
        private const string Template = "{0}: [{1}]";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "The serialized value is consumed.")]
        [DataMember(Name = AttributeNames.Value, Order = 2)]
        private string valueValue;

        public PatchOperation2SingleValued()
        {
            this.OnInitialization();
        }

        public PatchOperation2SingleValued(OperationName operationName, string pathExpression, string value)
            : base(operationName, pathExpression)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            this.valueValue = value;
        }

        public string Value
        {
            get
            {
                return this.valueValue;
            }
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.OnInitialization();
        }

        private void OnInitialization()
        {
            this.valueValue = string.Empty;
        }

        public override string ToString()
        {
            string operation = base.ToString();
            string result =
                string.Format(
                    CultureInfo.InvariantCulture,
                    PatchOperation2SingleValued.Template,
                    operation,
                    this.valueValue);
            return result;
        }
    }
}
