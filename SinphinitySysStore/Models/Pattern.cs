using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Sinphinity.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        public int DurationInTicks { get; set; }
        public int NumberOfNotes { get; set; }
        /// <summary>
        /// The difference between the highest pitch and the lowest pitch
        /// </summary>
        public int Range { get; set; }
        /// <summary>
        /// If true notes never go up or never go down
        /// </summary>
        public bool IsMonotone { get; set; }
        /// <summary>
        /// The difference between the pitch of the last and the first note
        /// </summary>
        public int Step { get; set; }

        public Pattern() { }
        public Pattern(string asString)
        {
            AsString = asString;

            var Duration = 0;
            int? highestNote = null;
            int? lowestNote = null;
            var noteAbsPitch = 0;
            IsMonotone = true;
            bool? IsGoingUp = null;
            NumberOfNotes = 0;
            foreach (Match m in Regex.Matches(asString, @"(\([0-9]+,[-]?[0-9]+\))"))
            {
                var values = Regex.Matches(m.Value, @"[-]?[0-9]+");
                var noteDuration = int.Parse(values[0].Value);
                var noteRelPitch = int.Parse(values[1].Value);
                if (IsGoingUp == null)
                {
                    if (noteRelPitch != 0)
                        IsGoingUp = noteRelPitch > 0;
                }
                else
                {
                    if (((bool)IsGoingUp && noteRelPitch < 0) ||
                        (!(bool)IsGoingUp && noteRelPitch < 0))
                        IsMonotone = false;
                }
                noteAbsPitch += noteRelPitch;
                Duration += noteDuration;
                if (highestNote == null || highestNote < noteAbsPitch)
                    highestNote = noteAbsPitch;
                if (lowestNote == null || lowestNote > noteAbsPitch)
                    lowestNote = noteAbsPitch;

                var relNote = new RelativeNote
                {
                    DeltaPitch = noteRelPitch,
                    DeltaTick = noteDuration
                };
            }
            NumberOfNotes -= 1;
            DurationInTicks = Duration;
            Range = (int)highestNote - (int)lowestNote;
            Step = noteAbsPitch;
        }
    }
}
