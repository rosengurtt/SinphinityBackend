using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Sinphinity.Models;
using System.Collections.Generic;

namespace SinphinitySysStore.Models
{
    public class Pattern
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

        public string AsString { get; set; }
    }
}
