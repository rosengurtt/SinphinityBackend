using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SinphinitySysStore.Models
{
    /// <summary>
    /// Class that has the data about the song that Sinphinity elaborates. This data can be large, so it is best to keep it in an independent collection
    /// </summary>
    public class SongData
    {
        public SongData() { }
        public SongData(Song song)
        {
            Id = song.SongDataId;
            SongInfoId = song.Id;
            SongSimplifications = song.SongSimplifications;
            Bars = song.Bars;
            TempoChanges = song.TempoChanges;
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
        public List<SongSimplification> SongSimplifications { get; set; }
        public List<Bar> Bars { get; set; }
        public List<TempoChange> TempoChanges { get; set; }
        /// <summary>
        /// A foreign key to the collection with general information of the songs
        /// </summary>
        public string SongInfoId { get; set; }
    }
}
