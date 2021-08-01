using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;


namespace SinphinitySysStore.Models
{
    public class PatternOccurrence
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

        public string PatternId { get; set; }
        public string SongInfoId { get; set; }
        public byte Voice { get; set; }
        public int BarNumber { get; set; }
        public int Beat { get; set; }
    }
}
