// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.SCIM
{
    using System.Threading;

    public abstract class ProtocolJsonDeserializingFactory : ProtocolJsonDeserializingFactory<Schematized>
    {
    }

    public abstract class ProtocolJsonDeserializingFactory<T> : JsonDeserializingFactory<T>
    {
        private IJsonNormalizationBehavior jsonNormalizer;

        public override IJsonNormalizationBehavior JsonNormalizer
        {
            get
            {
                IJsonNormalizationBehavior result =
                    LazyInitializer.EnsureInitialized<IJsonNormalizationBehavior>(
                        ref this.jsonNormalizer,
                        () =>
                            new ProtocolJsonNormalizer());
                return result;
            }
        }
    }
}