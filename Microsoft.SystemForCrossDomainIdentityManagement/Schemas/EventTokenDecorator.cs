// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;

    public abstract class EventTokenDecorator : IEventToken
    {
        protected EventTokenDecorator(IEventToken innerToken)
        {
            this.InnerToken = innerToken ?? throw new ArgumentNullException(nameof(innerToken));
        }

        protected EventTokenDecorator(string serialized)
            : this(new EventToken(serialized))
        {
        }

        public IReadOnlyCollection<string> Audience
        {
            get
            {
                IReadOnlyCollection<string> result = this.InnerToken.Audience;
                return result;
            }

            set
            {
                this.InnerToken.Audience = value;
            }
        }

        public IDictionary<string, object> Events
        {
            get
            {
                IDictionary<string, object> results = this.InnerToken.Events;
                return results;
            }
        }

        public DateTime? Expiration
        {
            get
            {
                DateTime? result = this.InnerToken.Expiration;
                return result;
            }

            set
            {
                this.InnerToken.Expiration = value;
            }
        }

        public JwtHeader Header
        {
            get
            {
                JwtHeader result = this.InnerToken.Header;
                return result;
            }
        }

        public string Identifier
        {
            get
            {
                string result = this.InnerToken.Identifier;
                return result;
            }
        }

        public IEventToken InnerToken
        {
            get;
            private set;
        }

        public DateTime IssuedAt
        {
            get
            {
                DateTime result = this.InnerToken.IssuedAt;
                return result;
            }
        }

        public string Issuer
        {
            get
            {
                string result = this.InnerToken.Issuer;
                return result;
            }
        }

        public DateTime? NotBefore
        {
            get
            {
                DateTime? result = this.InnerToken.NotBefore;
                return result;
            }
        }

        public string Subject
        {
            get
            {
                string result = this.InnerToken.Subject;
                return result;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Transaction
        {
            get
            {
                string result = this.InnerToken.Transaction;
                return result;
            }

            set
            {
                this.InnerToken.Transaction = value;
            }
        }
    }
}