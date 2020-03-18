//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM.Protocol
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Threading;

    class PatchOperation2CombinedDeserializingFactory :
        ProtocolJsonDeserializingFactory<PatchOperation2Combined>
    {

        private static readonly Lazy<DataContractJsonSerializer> JsonSerializer =
            new Lazy<DataContractJsonSerializer>(
                () =>
                    new DataContractJsonSerializer(
                        typeof(PatchOperation2Combined)));

        public override PatchOperation2Combined Create(IReadOnlyDictionary<string, object> json)
        {
            if (null == json)
            {
                throw new ArgumentNullException(nameof(json));
            }

            IReadOnlyDictionary<string, object> normalizedJson = this.Normalize(json);
            string serialized = JsonFactory.Instance.Create(normalizedJson, this.AcceptLargeObjects);

            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream();
                Stream streamed = stream;
                StreamWriter writer = null;
                try
                {
                    writer = new StreamWriter(stream);
                    stream = null;
                    writer.Write(serialized);
                    writer.Flush();

                    streamed.Position = 0;
                    PatchOperation2Combined result =
                        (PatchOperation2Combined)PatchOperation2CombinedDeserializingFactory.JsonSerializer.Value.ReadObject(
                            streamed);
                    return result;
                }
                finally
                {
                    if (writer != null)
                    {
                        writer.Close();
                        writer = null;
                    }
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
#pragma warning disable IDE0059 // Value assigned to symbol is never used
                    stream = null;
#pragma warning restore IDE0059 // Value assigned to symbol is never used
                }
            }
        }

        public override PatchOperation2Combined Create(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentNullException(nameof(json));
            }

            IReadOnlyDictionary<string, object> keyedValues = JsonFactory.Instance.Create(json, this.AcceptLargeObjects);
            PatchOperation2Combined result = this.Create(keyedValues);
            return result;
        }

    }
}