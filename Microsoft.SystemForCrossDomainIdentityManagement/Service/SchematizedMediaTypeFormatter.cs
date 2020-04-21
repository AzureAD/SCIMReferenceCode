// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.

namespace Microsoft.SCIM
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    public sealed class SchematizedMediaTypeFormatter : MediaTypeFormatter
    {
        private static readonly Encoding Encoding = Encoding.UTF8;

        private static readonly Lazy<MediaTypeHeaderValue> MediaTypeHeaderJavaWebToken =
            new Lazy<MediaTypeHeaderValue>(
                () =>
                    new MediaTypeHeaderValue(MediaTypes.JavaWebToken));

        private static readonly Lazy<MediaTypeHeaderValue> MediaTypeHeaderJson =
            new Lazy<MediaTypeHeaderValue>(
                () =>
                    new MediaTypeHeaderValue(MediaTypes.Json));

        private static readonly Lazy<MediaTypeHeaderValue> MediaTypeHeaderProtocol =
            new Lazy<MediaTypeHeaderValue>(
                () =>
                    new MediaTypeHeaderValue(MediaTypes.Protocol));

        private static readonly Lazy<MediaTypeHeaderValue> MediaTypeHeaderStream =
            new Lazy<MediaTypeHeaderValue>(
                () =>
                    new MediaTypeHeaderValue(MediaTypes.Stream));

        public SchematizedMediaTypeFormatter(
            IMonitor monitor,
            JsonDeserializingFactory<Schematized> deserializingFactory)
        {
            this.Monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));

            this.SupportedMediaTypes.Add(SchematizedMediaTypeFormatter.MediaTypeHeaderJavaWebToken.Value);
            this.SupportedMediaTypes.Add(SchematizedMediaTypeFormatter.MediaTypeHeaderJson.Value);
            this.SupportedMediaTypes.Add(SchematizedMediaTypeFormatter.MediaTypeHeaderProtocol.Value);
            this.SupportedMediaTypes.Add(SchematizedMediaTypeFormatter.MediaTypeHeaderStream.Value);

            this.DeserializingFactory = deserializingFactory ?? throw new ArgumentNullException(nameof(deserializingFactory));
        }

        private JsonDeserializingFactory<Schematized> DeserializingFactory
        {
            get;
            set;
        }

        private IMonitor Monitor
        {
            get;
            set;
        }

        private static bool CanProcessType(Type type)
        {
            Type schematizedType = typeof(Schematized);
            bool result = schematizedType.IsAssignableFrom(type) || type == typeof(string);
            return result;
        }

        public override bool CanReadType(Type type)
        {
            if (null == type)
            {
                throw new ArgumentNullException(nameof(type));
            }

            bool result = SchematizedMediaTypeFormatter.CanProcessType(type);
            return result;
        }

        public override bool CanWriteType(Type type)
        {
            if (null == type)
            {
                throw new ArgumentNullException(nameof(type));
            }

            bool result = SchematizedMediaTypeFormatter.CanProcessType(type);
            return result;
        }

        private static async Task<string> ReadFromStream(Stream readStream)
        {
            if (null == readStream)
            {
                throw new ArgumentNullException(nameof(readStream));
            }

            StreamReader reader = null;
            try
            {
                reader = new StreamReader(readStream, SchematizedMediaTypeFormatter.Encoding);
                string result = await reader.ReadToEndAsync().ConfigureAwait(false);
                return result;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader = null;
                }
            }
        }

        private async Task<object> ReadFromStream(Type type, Stream readStream, HttpContent content)
        {
            if (null == type)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (null == readStream)
            {
                throw new ArgumentNullException(nameof(readStream));
            }

            if
            (
                    !typeof(IDictionary<string, object>).IsAssignableFrom(type)
                && !typeof(Schematized).IsAssignableFrom(type)
                && typeof(string) != type
            )
            {
                throw new NotSupportedException(type.FullName);
            }

            string characters = await SchematizedMediaTypeFormatter.ReadFromStream(readStream).ConfigureAwait(false);
            string information = string.Concat(SystemForCrossDomainIdentityManagementServiceResources.InformationRead, characters);
            IInformationNotification notification =
                InformationNotificationFactory.Instance.CreateNotification(
                    information,
                    null,
                    ServiceNotificationIdentifiers.SchematizedMediaTypeFormatterReadFromStream);
            this.Monitor.Inform(notification);

            if
            (
                    string.Equals(
                        content?.Headers?.ContentType?.MediaType,
                        MediaTypes.JavaWebToken,
                        StringComparison.Ordinal)
            )
            {
                return characters;
            }

            Dictionary<string, object> json =
                JsonFactory.Instance.Create(
                    characters,
                    this.DeserializingFactory.AcceptLargeObjects);
            if (typeof(IDictionary<string, object>).IsAssignableFrom(type))
            {
                return json;
            }

            try
            {
                Schematized result = this.DeserializingFactory.Create(json);
                return result;
            }
            catch (ArgumentException)
            {
                return new HttpResponseException(HttpStatusCode.BadRequest);
            }
            catch (NotSupportedException)
            {
                return new HttpResponseException(HttpStatusCode.BadRequest);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
            {
                return new HttpResponseException(HttpStatusCode.BadRequest);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        public override Task<object> ReadFromStreamAsync(
            Type type,
            Stream readStream,
            HttpContent content,
            IFormatterLogger formatterLogger)
        {
            if (null == type)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (null == readStream)
            {
                throw new ArgumentNullException(nameof(readStream));
            }

            Task<object> result = this.ReadFromStream(type, readStream, content);
            return result;
        }

        public override Task<object> ReadFromStreamAsync(
            Type type,
            Stream readStream,
            HttpContent content,
            IFormatterLogger formatterLogger,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return this.ReadFromStreamAsync(type, readStream, content, formatterLogger);
        }

        private async Task WriteToStream(
            Type type,
            object value,
            Stream writeStream,
            Func<byte[], Task> writeFunction)
        {
            if (null == type)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (null == value)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (null == writeStream)
            {
                throw new ArgumentNullException(nameof(writeStream));
            }

            string characters;
            if (typeof(string) == type)
            {
                characters = (string)value;
            }
            else
            {
                IDictionary<string, object> json;
                if (typeof(IDictionary<string, object>).IsAssignableFrom(type))
                {
                    json = (IDictionary<string, object>)value;
                    characters = JsonFactory.Instance.Create(json, this.DeserializingFactory.AcceptLargeObjects);
                }
                else if (typeof(Schematized).IsAssignableFrom(type))
                {
                    Schematized schematized = (Schematized)value;
                    json = schematized.ToJson();
                    characters = JsonFactory.Instance.Create(json, this.DeserializingFactory.AcceptLargeObjects);
                }
                else
                {
                    throw new NotSupportedException(type.FullName);
                }
            }
            string information =
                string.Concat(
                    SystemForCrossDomainIdentityManagementServiceResources.InformationWrote,
                    characters);
            IInformationNotification notification =
                InformationNotificationFactory.Instance.CreateNotification(
                    information,
                    null,
                    ServiceNotificationIdentifiers.SchematizedMediaTypeFormatterWroteToStream);
            this.Monitor.Inform(notification);
            byte[] bytes = SchematizedMediaTypeFormatter.Encoding.GetBytes(characters);
            await writeFunction(bytes).ConfigureAwait(false);
            writeStream.Flush();
        }

        public override Task WriteToStreamAsync(
            Type type,
            object value,
            Stream writeStream,
            HttpContent content,
            TransportContext transportContext)
        {
            if (null == type)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (null == writeStream)
            {
                throw new ArgumentNullException(nameof(writeStream));
            }

            Func<byte[], Task> writeFunction =
                new Func<byte[], Task>(
                    async (byte[] buffer) =>
                        await writeStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false));
            Task result = this.WriteToStream(type, value, writeStream, writeFunction);
            return result;
        }

        public override Task WriteToStreamAsync(
            Type type,
            object value,
            Stream writeStream,
            HttpContent content,
            TransportContext transportContext,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (null == type)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (null == writeStream)
            {
                throw new ArgumentNullException(nameof(writeStream));
            }

            Func<byte[], Task> writeFunction =
                new Func<byte[], Task>(
                    async (byte[] buffer) =>
                        await writeStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false));
            Task result = this.WriteToStream(type, value, writeStream, writeFunction);
            return result;
        }
    }
}
