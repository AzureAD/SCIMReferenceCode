//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    [DataContract]
    public sealed class PatchOperation2Combined : PatchOperation2Base
    {
        private const string Template = "{0}: [{1}]";

        [DataMember(Name = AttributeNames.Value, Order = 2)]
        private object values;


        public PatchOperation2Combined()
        {
        }

        public PatchOperation2Combined(OperationName operationName, string pathExpression)
            : base(operationName, pathExpression)
        {
        }

        public string Value
        {
            get
            {
                if (this.values == null)
                {
                    return null;
                }

                string result = JsonConvert.SerializeObject(this.values);
                return result;
            }

            set
            {
                this.values = value;
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (this.Value == null)
            {
                if 
                (
                    this?.Path?.AttributePath != null &&
                    this.Path.AttributePath.Contains(AttributeNames.Members, StringComparison.OrdinalIgnoreCase) &&
                    this.Name == SCIM.OperationName.Remove &&
                    this.Path?.SubAttributes?.Count == 1
                )
                {
                    this.Value = this.Path.SubAttributes.First().ComparisonValue;
                    IPath path = SCIM.Path.Create(AttributeNames.Members);
                    this.Path = path;
                }
            }
        }

        public override string ToString()
        {
            string allValues = string.Join(Environment.NewLine, this.Value);
            string operation = base.ToString();
            string result =
                string.Format(
                    CultureInfo.InvariantCulture,
                    PatchOperation2Combined.Template,
                    operation,
                    allValues);
            return result;
        }
    }
}