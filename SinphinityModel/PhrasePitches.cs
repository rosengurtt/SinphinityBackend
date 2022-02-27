using Sinphinity.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SinphinityModel
{
    /// <summary>
    /// The list of relative pitches used in a melodic phrase in the order they are played, without rythm information
    /// Relative means that is the difference in pitch between consecutive notes, not the real pitch
    /// </summary>
    public class PhrasePitches
    {
        /// <summary>
        /// The primary key of the record in the db
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// A comma separated list of the relative pitches
        /// </summary>
        public string AsString { get; set; }

        /// <summary>
        /// The difference between the highest absolute pitch and the lowest absolute pitch
        /// </summary>
        public int Range
        {
            get
            {
                int MaxAccum = 0;
                int MinAccum = 0;
                int currentPitch = 0;
                foreach (var item in Items)
                {
                    currentPitch += item;
                    if (currentPitch > MaxAccum)
                        MaxAccum = currentPitch;
                    if (currentPitch < MinAccum)
                        MinAccum = currentPitch;
                }
                return MaxAccum - MinAccum;
            }
        }
        /// <summary>
        /// If true notes never go up or never go down
        /// </summary>
        public bool IsMonotone
        {
            get
            {
                return Items.All(x => x >= 0) || Items.All((x => x <= 0));
            }
        }
        /// <summary>
        /// The difference between the pitch of the last and the first note
        /// </summary>
        public int Step
        {
            get
            {
                return Items.Sum();
            }
        }
        public int NumberOfNotes
        {
            get
            {
                return Items.Count + 1;
            }
        }

        public List<int> Items
        {
            get
            {
                return AsString.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            }
        }



        public PhrasePitches() { }
        public PhrasePitches(string asString)
        {
            AsString = asString;
        }
        public PhrasePitches(List<Note> notes)
        {
            AsString = "";
            if (notes.Count > 1)
            {
                for (int i = 0; i < notes.Count - 1; i++)
                {
                    AsString += (notes[i + 1].Pitch - notes[i].Pitch).ToString() + ",";
                }
            }
            // remove last comma
            if (AsString.Contains(","))
                AsString = AsString.Substring(0, AsString.Length - 1);
        }
    }
}
