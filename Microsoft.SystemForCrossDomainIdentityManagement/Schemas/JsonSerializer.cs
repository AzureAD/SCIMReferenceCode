// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;

    internal class JsonSerializer : IJsonSerializable
    {
        private static readonly Lazy<DataContractJsonSerializerSettings> SerializerSettings =
            new Lazy<DataContractJsonSerializerSettings>(
                () =>
                    new DataContractJsonSerializerSettings()
                    {
                        EmitTypeInformation = EmitTypeInformation.Never
                    });

        private readonly object dataContractValue;

        public JsonSerializer(object dataContract)
        {
            this.dataContractValue = dataContract ??
                throw new ArgumentNullException(nameof(dataContract));
        }

        public string Serialize()
        {
            IDictionary<string, object> json = this.ToJson();
            string result = JsonFactory.Instance.Create(json, true);
            return result;
        }

        public Dictionary<string, object> ToJson()
        {
            Type type = this.dataContractValue.GetType();
            DataContractJsonSerializer serializer =
                new DataContractJsonSerializer(type, JsonSerializer.SerializerSettings.Value);

            string json;
            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream();
                serializer.WriteObject(stream, this.dataContractValue);
                stream.Position = 0;
                StreamReader streamReader = null;
                try
                {
                    streamReader = new StreamReader(stream);
                    stream = null;
                    json = streamReader.ReadToEnd();
                }
                finally
                {
                    if (streamReader != null)
                    {
                        streamReader.Close();
                        streamReader = null;
                    }
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }

            Dictionary<string, object> result = JsonFactory.Instance.Create(json, true);
            return result;
        }
    }
}