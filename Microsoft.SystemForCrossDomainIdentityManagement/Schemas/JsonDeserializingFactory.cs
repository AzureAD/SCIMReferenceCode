//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Threading;

    public abstract class JsonDeserializingFactory<TDataContract> : IJsonNormalizationBehavior
    {
        private static readonly Lazy<DataContractJsonSerializerSettings> JsonSerializerSettings =
            new Lazy<DataContractJsonSerializerSettings>(
                () =>
                    new DataContractJsonSerializerSettings()
                    {
                        EmitTypeInformation = EmitTypeInformation.Never
                    });

        private static readonly Lazy<DataContractJsonSerializer> JsonSerializer =
            new Lazy<DataContractJsonSerializer>(
                () =>
                    new DataContractJsonSerializer(
                        typeof(TDataContract),
                        JsonDeserializingFactory<TDataContract>.JsonSerializerSettings.Value));

        private IJsonNormalizationBehavior jsonNormalizer;

        public bool AcceptLargeObjects
        {
            get;
            set;
        }

        public virtual IJsonNormalizationBehavior JsonNormalizer
        {
            get
            {
                IJsonNormalizationBehavior result =
                    LazyInitializer.EnsureInitialized<IJsonNormalizationBehavior>(
                        ref this.jsonNormalizer,
                        () =>
                            new JsonNormalizer());
                return result;
            }
        }

        public virtual TDataContract Create(IReadOnlyDictionary<string, object> json)
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
                    TDataContract result =
                        (TDataContract)JsonDeserializingFactory<TDataContract>.JsonSerializer.Value.ReadObject(
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

        public virtual TDataContract Create(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentNullException(nameof(json));
            }

            IReadOnlyDictionary<string, object> keyedValues = JsonFactory.Instance.Create(json, this.AcceptLargeObjects);
            TDataContract result = this.Create(keyedValues);
            return result;
        }

        public IReadOnlyDictionary<string, object> Normalize(IReadOnlyDictionary<string, object> json)
        {
            if (null == json)
            {
                throw new ArgumentNullException(nameof(json));
            }

            IReadOnlyDictionary<string, object> result = this.JsonNormalizer.Normalize(json);
            return result;
        }

        public virtual TDataContract Read(string json)
        {
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
                    writer.Write(json);
                    writer.Flush();

                    streamed.Position = 0;
                    TDataContract result =
                        (TDataContract)JsonDeserializingFactory<TDataContract>.JsonSerializer.Value.ReadObject(
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
    }
}