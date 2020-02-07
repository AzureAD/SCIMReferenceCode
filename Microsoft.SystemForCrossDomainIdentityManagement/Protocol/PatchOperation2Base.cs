//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;

    [DataContract]
    public abstract class PatchOperation2Base : IPatchOperation2Base
    {
        private const string Template = "{0} {1}";

        private OperationName name;
        private string operationName;

        private IPath path;
        [DataMember(Name = ProtocolAttributeNames.Path, Order = 1)]
        private string pathExpression;

        protected PatchOperation2Base()
        {
        }

        protected PatchOperation2Base(OperationName operationName, string pathExpression)
        {
            if (string.IsNullOrWhiteSpace(pathExpression))
            {
                throw new ArgumentNullException(nameof(pathExpression));
            }

            this.Name = operationName;
            this.Path = Microsoft.SCIM.Path.Create(pathExpression);
        }

        public OperationName Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
                this.operationName = Enum.GetName(typeof(OperationName), value);
            }
        }

        // It's the value of 'op' parameter within the json of request body.
        [DataMember(Name = ProtocolAttributeNames.Patch2Operation, Order = 0)]
        public string OperationName
        {
            get
            {
                return this.operationName;
            }

            set
            {
                if (!Enum.TryParse(value, true, out this.name))
                {
                    throw new NotSupportedException();
                }

                this.operationName = value;
            }
        }

        public IPath Path
        {
            get
            {
                if (null == this.path && !string.IsNullOrWhiteSpace(this.pathExpression))
                {
                    this.path = Microsoft.SCIM.Path.Create(this.pathExpression);
                }

                return this.path;
            }

            set
            {
                this.pathExpression = value?.ToString();
                this.path = value;
            }
        }

        public override string ToString()
        {
            string result =
                string.Format(
                    CultureInfo.InvariantCulture,
                    PatchOperation2Base.Template,
                    this.operationName,
                    this.pathExpression);
            return result;
        }
    }
}
