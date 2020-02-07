//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    public class PaginationParameters : IPaginationParameters
    {
        int? count;
        int? startIndex;

        public int? Count
        {
            get
            {
                return this.count;
            }

            set
            {
                if (value.HasValue && value.Value < 0)
                {
                    this.count = 0;
                    return;
                }
                this.count = value;
            }
        }

        public int? StartIndex
        {
            get
            {
                return this.startIndex;
            }

            set
            {
                if (value.HasValue && value.Value < 1)
                {
                    this.startIndex = 1;
                    return;
                }
                this.startIndex = value;
            }
        }
    }
}