﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using SPWebHookSrv.Models;
//
//    var apiError = ApiError.FromJson(jsonString);

namespace SPWebHookSrv.Models
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class ApiError
    {
        [JsonProperty("error")]
        public Error Error { get; set; }
    }

    public partial class Error
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public Message Message { get; set; }
    }

    public partial class Message
    {
        [JsonProperty("lang")]
        public string Lang { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public partial class ApiError
    {
        public static ApiError FromJson(string json) => JsonConvert.DeserializeObject<ApiError>(json, SPWebHookSrv.Models.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this ApiError self) => JsonConvert.SerializeObject(self, SPWebHookSrv.Models.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}