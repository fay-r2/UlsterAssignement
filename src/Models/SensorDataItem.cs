using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace todo.Models
{
    public class BlobJson
    {
        public long? Width { get; set; } = null;


        public long? Height { get; set; } = null;


        public string Unit { get; set; } = null;


        public double? Reading { get; set; } = null;

        public List<long> FrameData { get; set; }
    }

    public class SensorDataItem
    {
        [JsonIgnore]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string SensorUUID { get; set; }

        public string SensorHardwareID { get; set; }

        public double TimeStamp { get; set; }
        
        [JsonIgnore]
        public DateTime TimeDate { get; set; }

        public long DeviceMfg { get; set; }

        [Range(1, 2)]
        public long SensorClass { get; set; }

        public BlobJson BlobJson { get; set; }

    }
}