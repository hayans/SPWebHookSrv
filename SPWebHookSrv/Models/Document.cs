using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPWebHookSrv.Models
{
    public class Document
    {
        [JsonProperty("__metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("Files")]
        public Files Files { get; set; }

        [JsonProperty("ListItemAllFields")]
        public Files ListItemAllFields { get; set; }

        [JsonProperty("ParentFolder")]
        public Files ParentFolder { get; set; }

        [JsonProperty("Properties")]
        public Files Properties { get; set; }

        [JsonProperty("StorageMetrics")]
        public Files StorageMetrics { get; set; }

        [JsonProperty("Folders")]
        public Files Folders { get; set; }

        [JsonProperty("Exists")]
        public bool Exists { get; set; }

        [JsonProperty("IsWOPIEnabled")]
        public bool IsWopiEnabled { get; set; }

        [JsonProperty("ItemCount")]
        public long ItemCount { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("ProgID")]
        public object ProgId { get; set; }

        [JsonProperty("ServerRelativeUrl")]
        public string ServerRelativeUrl { get; set; }

        [JsonProperty("TimeCreated")]
        public DateTimeOffset TimeCreated { get; set; }

        [JsonProperty("TimeLastModified")]
        public DateTimeOffset TimeLastModified { get; set; }

        [JsonProperty("UniqueId")]
        public Guid UniqueId { get; set; }

        [JsonProperty("WelcomePage")]
        public string WelcomePage { get; set; }
    }

    //public partial class Files
    //{
    //    [JsonProperty("__deferred")]
    //    public Deferred Deferred { get; set; }
    //}

    //public partial class Deferred
    //{
    //    [JsonProperty("uri")]
    //    public Uri Uri { get; set; }
    //}

    //public partial class Metadata
    //{
    //    [JsonProperty("id")]
    //    public Uri Id { get; set; }

    //    [JsonProperty("uri")]
    //    public Uri Uri { get; set; }

    //    [JsonProperty("type")]
    //    public string Type { get; set; }
    //}
}