﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace SinphinitySysStore.Models
{
    public class SongInfo
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
        public bool IsSongProcessed { get; set; }
        public bool IsMidiCorrect { get; set; }
        public bool CantBeProcessed { get; set; }
        public Band Band { get; set; }
        public Style Style { get; set; }
        public MidiStats MidiStats { get; set; }

    }
}
