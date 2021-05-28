using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace SinphinitySysStore.Models
{
    public class Band
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

        public string Name { get; set; }

        public Style Style { get; set; }
    }
}
