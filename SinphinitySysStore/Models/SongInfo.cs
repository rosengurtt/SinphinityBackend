using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace SinphinitySysStore.Models
{
    public class SongInfo
    {
        public SongInfo() { }
        public SongInfo(Song song)
        {
            Id = song.Id;
            Name = song.Name;
            IsSongProcessed = song.IsSongProcessed;
            IsMidiCorrect = song.IsMidiCorrect;
            CantBeProcessed = song.CantBeProcessed;
            Band = song.Band;
            Style = song.Style;
            DurationInSeconds = song.DurationInSeconds;
            DurationInTicks = song.DurationInTicks;
            AverageTempoInBeatsPerMinute = song.AverageTempoInBeatsPerMinute;
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

        public string Name { get; set; }
        public bool IsSongProcessed { get; set; }
        public bool IsMidiCorrect { get; set; }
        public bool CantBeProcessed { get; set; }
        public Band Band { get; set; }
        public Style Style { get; set; }
        public MidiStats MidiStats { get; set; }
        public long DurationInSeconds { get; set; }
        public long DurationInTicks { get; set; }
        public long AverageTempoInBeatsPerMinute { get; set; }

        /// <summary>
        /// Foreign key to collection songData
        /// </summary>
        public string SongDataId { get; set; }
        /// <summary>
        /// Foreign key to collection songMidi
        /// </summary>
        public string SongMidiId { get; set; }
    }
}
