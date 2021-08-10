//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;

    [DataContract]
    [KnownType(typeof(ErrorResponse))]
    [KnownType(typeof(Core2EnterpriseUser))]
    [KnownType(typeof(QueryResponse<Core2EnterpriseUser>))]
    [KnownType(typeof(Core2User))]
    [KnownType(typeof(QueryResponse<Core2User>))]
    [KnownType(typeof(QueryResponse<Core2Group>))]
    [KnownType(typeof(Core2Group))]
    public sealed class BulkResponseOperation : BulkOperation, IResponse
    {
        private IResponse response;

        public BulkResponseOperation(string identifier)
            : base(identifier)
        {
            this.OnInitialization();
        }

        public BulkResponseOperation()
            : base(null)
        {
            this.OnInitialization();
        }

        [DataMember(Name = ProtocolAttributeNames.Location)]
        public Uri Location
        {
            get;
            set;
        }

        [DataMember(Name = ProtocolAttributeNames.Response)]
        public object Response
        {
            get;
            set;
        }

        public HttpStatusCode Status
        {
            get => this.response.Status;
            set => this.response.Status = value;
        }

        [DataMember(Name = ProtocolAttributeNames.Status)]
        public string StatusCodeValue
        {
            get => this.response.StatusCodeValue;
            set => this.response.StatusCodeValue = value;
        }

        public bool IsError() => this.response.IsError();
        
        [OnDeserializing]
        private void OnDeserializing(StreamingContext _) => this.OnInitialization();

        private void OnInitialization() => this.response = new Response();
    }
}
