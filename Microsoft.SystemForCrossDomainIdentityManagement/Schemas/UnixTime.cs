//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Globalization;

    // Refer to https://en.wikipedia.org/wiki/Unix_time
    public class UnixTime : IUnixTime
    {
        public static readonly DateTime Epoch =
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public UnixTime(long epochTimestamp)
        {
            this.EpochTimestamp = epochTimestamp;
        }

        public UnixTime(int epochTimestamp)
            : this(Convert.ToInt64(epochTimestamp))
        {
        }

        public UnixTime(double epochTimestamp)
            : this(Convert.ToInt64(epochTimestamp))
        {
        }

        public UnixTime(TimeSpan sinceEpoch)
            : this(sinceEpoch.TotalSeconds)
        {
        }

        public UnixTime(DateTime dateTime)
            : this(dateTime.ToUniversalTime().Subtract(UnixTime.Epoch))
        {
        }

        public DateTime ToUniversalTime()
        {
            DateTime result = UnixTime.Epoch.AddSeconds(this.EpochTimestamp);
            return result;
        }

        public long EpochTimestamp
        {
            get;
            private set;
        }

        public override string ToString()
        {
            string result = this.EpochTimestamp.ToString(CultureInfo.InvariantCulture);
            return result;
        }
    }
}
