//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class PatchRequest2DeserializingFactory<TPatchRequest, TOperation> :
        ProtocolJsonDeserializingFactory<TPatchRequest>,
        ISchematizedJsonDeserializingFactory<TPatchRequest>
        where TOperation : PatchOperation2Base
        where TPatchRequest : PatchRequest2Base<TOperation>
    {
        public override TPatchRequest Create(IReadOnlyDictionary<string, object> json)
        {
            Dictionary<string, object> normalized =
                this.Normalize(json)
                .ToDictionary(
                    (KeyValuePair<string, object> item) => item.Key,
                    (KeyValuePair<string, object> item) => item.Value);
            if (normalized.TryGetValue(ProtocolAttributeNames.Operations, out object operations))
            {
                normalized.Remove(ProtocolAttributeNames.Operations);
            }
            TPatchRequest result = base.Create(normalized);
            if (operations != null)
            {
                IReadOnlyCollection<PatchOperation2Base> patchOperations =
                    PatchRequest2DeserializingFactory<TPatchRequest, TOperation>.Deserialize(operations);
                foreach (PatchOperation2Base patchOperation in patchOperations)
                {
                    result.AddOperation(patchOperation as TOperation);
                }
            }
            return result;
        }

        private static bool TryDeserialize(Dictionary<string, object> json, out PatchOperation2Base operation)
        {
            operation = null;
            if (null == json)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (!json.TryGetValue(AttributeNames.Value, out object value))
            {
                return false;
            }

            switch (value)
            {
                case string scalar:
                    operation = new PatchOperation2SingleValuedJsonDeserializingFactory().Create(json);
                    return true;
                case ArrayList _:
                case object _:
                    operation = new PatchOperation2JsonDeserializingFactory().Create(json);
                    return true;
                default:
                    string unsupported = value.GetType().FullName;
                    throw new NotSupportedException(unsupported);
            }
        }

        private static IReadOnlyCollection<PatchOperation2Base> Deserialize(ArrayList operations)
        {
            if (null == operations)
            {
                throw new ArgumentNullException(nameof(operations));
            }

            List<PatchOperation2Base> result = new List<PatchOperation2Base>(operations.Count);
            foreach (Dictionary<string, object> json in operations)
            {
                if
                (
                    PatchRequest2DeserializingFactory<TPatchRequest, TOperation>.TryDeserialize(
                        json,
                        out PatchOperation2Base patchOperation)
                )
                {
                    result.Add(patchOperation);
                }
            }
            return result;
        }

        private static IReadOnlyCollection<PatchOperation2Base> Deserialize(object[] operations)
        {
            if (null == operations)
            {
                throw new ArgumentNullException(nameof(operations));
            }

            List<PatchOperation2Base> result = new List<PatchOperation2Base>(operations.Length);
            foreach (Dictionary<string, object> json in operations)
            {
                if
                (
                    PatchRequest2DeserializingFactory<TPatchRequest, TOperation>.TryDeserialize(
                        json,
                        out PatchOperation2Base patchOperation)
                )
                {
                    result.Add(patchOperation);
                }
            }
            return result;
        }

        private static IReadOnlyCollection<PatchOperation2Base> Deserialize(object operations)
        {
            IReadOnlyCollection<PatchOperation2Base> result;
            switch (operations)
            {
                case ArrayList list:
                    result = PatchRequest2DeserializingFactory<TPatchRequest, TOperation>.Deserialize(list);
                    return result;
                case object[] array:
                    result = PatchRequest2DeserializingFactory<TPatchRequest, TOperation>.Deserialize(array);
                    return result;
                default:
                    string unsupported = operations.GetType().FullName;
                    throw new NotSupportedException(unsupported);
            }
        }
    }
}