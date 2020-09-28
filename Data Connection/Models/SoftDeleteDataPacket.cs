using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DataConnection.Models
{
    [JsonObject]
    public class SoftDeleteDataPacket<T> : DataPacket<T>, ISoftDeleteDataPacket
    {
        [JsonProperty("deleted_at")]
        public DateTime? DeletedAt { get; set; } = null;
    }
}
