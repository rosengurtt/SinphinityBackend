using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SinphinitySysStore.Models
{
    public class Song 
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
        public string MidiBase64Encoded { get; set; }
        public Band Band { get; set; }
        public Style Style { get; set; }
        public MidiStats MidiStats { get; set; }

        public List<SongSimplification> SongSimplifications { get; set; }
        public List<Bar> Bars { get; set; }
        public List<TempoChange> TempoChanges { get; set; }
        public long DurationInSeconds { get; set; }
        public long DurationInTicks { get; set; }
        public long AverageTempoInBeatsPerMinute { get; set; }
    }
}
