//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.SCIM
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public static class ProtocolConstants
    {
        public const string ContentType = "application/scim+json";
        public const string PathGroups = "Groups";
        public const string PathUsers = "Users";
        public const string PathWebBatchInterface = SchemaConstants.PathInterface + "/batch";

        public readonly static Lazy<JsonSerializerSettings> JsonSettings =
            new Lazy<JsonSerializerSettings>(() => ProtocolConstants.InitializeSettings());

        private static JsonSerializerSettings InitializeSettings()
        {
            JsonSerializerSettings result = new JsonSerializerSettings();
            result.Error = delegate (object sender, ErrorEventArgs args) { args.ErrorContext.Handled = true; };
            return result;
        }
    }
}