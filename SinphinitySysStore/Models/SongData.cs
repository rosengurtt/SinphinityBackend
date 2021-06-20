using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinitySysStore.Models
{
    public class SongData
    {
        private string _id;

        [BsonElement("_id")]
        [JsonProperty("_id")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id
        {
            get { return this._id; }
            set { this._id = value; }
        }
        public List<SongSimplification> SongSimplifications { get; set; }
        public List<Bar> Bars { get; set; }
        public List<TempoChange> TempoChanges { get; set; }
    }
}
