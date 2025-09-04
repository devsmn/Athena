using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Athena.DataModel
{
    public class BackupMetaData
    {
        [JsonPropertyName("creationDate")]
        public long CreationDate { get; set; }

        [JsonPropertyName("appVersion")]
        public string AppVersion { get; set; }

        [JsonPropertyName("initiator")]
        public string Initiator { get; set; }

        [JsonPropertyName("folderCount")]
        public int FolderCount { get; set; }

        [JsonPropertyName("documentCount")]
        public int DocumentCount { get; set; }

        [JsonPropertyName("tagCount")]
        public int TagCount { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("minVersion")]
        public int MinVersion { get; set; }
    }
}
