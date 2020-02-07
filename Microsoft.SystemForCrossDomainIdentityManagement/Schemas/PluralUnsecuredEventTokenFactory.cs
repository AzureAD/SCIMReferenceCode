//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;

    public class PluralUnsecuredEventTokenFactory : UnsecuredEventTokenFactory
    {
        public PluralUnsecuredEventTokenFactory(string issuer)
            : base(issuer)
        {
        }

        public override IEventToken Create(IDictionary<string, object> events)
        {
            if (null == events)
            {
                throw new ArgumentNullException(nameof(events));
            }

            IEventToken result = new EventToken(this.Issuer, this.Header, events);
            return result;
        }
    }
}