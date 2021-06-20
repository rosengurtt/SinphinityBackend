using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace SinphinitySysStore.Models
{
    /// <summary>
    /// This class is used for storing the original midi of the song in mongo db in an independent collection. This file can have several kilobytes, so we
    /// keep it out of collection SongInformation 
    /// </summary>
    public class SongMidi
    {
        public SongMidi() { }
        public SongMidi(Song song)
        {
            Id = song.SongMidiId;
            SongInfoId = song.Id;
            MidiBase64Encoded = song.MidiBase64Encoded;
        }
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

        public string MidiBase64Encoded { get; set; }

        /// <summary>
        /// A foreign key to the collection with general information of the songs
        /// </summary>
        public string SongInfoId { get; set; }
    }
}
