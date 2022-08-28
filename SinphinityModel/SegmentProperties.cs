using Sinphinity.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinphinity.Models
{
    /// <summary>
    /// Given a sequence of consecutive notes, it calculates some indexes that give information of what is going on with the notes
    /// </summary>
    public class SegmentProperties
    {
        public SegmentProperties(List<Note> notes)
        {
            Notes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
        }
        public List<Note> Notes { get; set; }

        /// <summary>
        /// The difference between the highest and the lowest pitch
        /// </summary>
        public int Range
        {
            get
            {
                return Notes.Max(x => x.Pitch) - Notes.Min(x => x.Pitch);
            }
        }
        /// <summary>
        /// The number of changes in pitch direction, divided by 'the number of pitches minus 2'
        /// (we substract 2, because we need 3 notes to be able to have 1 change, and if the notes are changing direction all
        /// the time, the number of changes is n-2, and we want to have a Zigzag index of 1 in that case
        /// 
        /// A scale going up or down has a MonotoneIndex of 0
        /// 
        /// A group of notes alternating between 2 values, for ex. C4 G4 C4 G4 C4 G has a MonotoneIndex of 1
        /// </summary>
        public double ZigzagIndex
        {
            get
            {
                if (Notes.Count < 3) return 0;

                var totalDirectionChanges = 0;
                var currentDirection = 0;
                for (var i = 0; i < Notes.Count - 1; i++)
                {
                    if (i == 0)
                    {
                        while (Notes[i + 1].Pitch == Notes[i].Pitch && i < Notes.Count - 1)
                            i++;
                        currentDirection = Math.Sign(Notes[i + 1].Pitch - Notes[i].Pitch);
                        continue;
                    }
                    if (currentDirection * Math.Sign(Notes[i + 1].Pitch - Notes[i].Pitch) < 0)
                    {
                        totalDirectionChanges++;
                        currentDirection = Math.Sign(Notes[i + 1].Pitch - Notes[i].Pitch);
                    }
                }
                return totalDirectionChanges / (double)(Notes.Count - 2);
            }
        }
        /// <summary>
        /// Returns a dictionary where the keys are intervals and the value the times that interval is present in consecutive notes
        /// </summary>
        public Dictionary<IntervalsEnum, int> IntervalSpectrum
        {
            get
            {
                var retObj = new Dictionary<IntervalsEnum, int>();
                retObj[IntervalsEnum.unison] = 0;
                retObj[IntervalsEnum.second] = 0;
                retObj[IntervalsEnum.third] = 0;
                retObj[IntervalsEnum.forth] = 0;
                retObj[IntervalsEnum.tritone] = 0;
                retObj[IntervalsEnum.fifth] = 0;
                retObj[IntervalsEnum.sixth] = 0;
                retObj[IntervalsEnum.seventh] = 0;
                retObj[IntervalsEnum.octave] = 0;
                retObj[IntervalsEnum.ninth] = 0;
                retObj[IntervalsEnum.tenthPlus] = 0;

                for (var i = 0; i < Notes.Count - 1; i++)
                {
                    var pitchDif = Math.Abs(Notes[i + 1].Pitch - Notes[i].Pitch);
                    if (pitchDif == 0)
                        retObj[IntervalsEnum.unison] += 1;
                    else if (pitchDif >= 1 && pitchDif <= 2)
                        retObj[IntervalsEnum.second] += 1;
                    else if (pitchDif >= 3 && pitchDif <= 4)
                        retObj[IntervalsEnum.third] += 1;
                    else if (pitchDif == 5)
                        retObj[IntervalsEnum.forth] += 1;
                    else if (pitchDif == 6)
                        retObj[IntervalsEnum.tritone] += 1;
                    else if (pitchDif == 7)
                        retObj[IntervalsEnum.fifth] += 1;
                    else if (pitchDif >= 8 && pitchDif <= 9)
                        retObj[IntervalsEnum.sixth] += 1;
                    else if (pitchDif >= 10 && pitchDif <= 11)
                        retObj[IntervalsEnum.seventh] += 1;
                    else if (pitchDif == 12)
                        retObj[IntervalsEnum.octave] += 1;
                    else if (pitchDif >= 13 && pitchDif <= 14)
                        retObj[IntervalsEnum.ninth] += 1;
                    else
                        retObj[IntervalsEnum.tenthPlus] += 1;
                }
                return retObj;
            }
        }

        /// <summary>
        /// If the last note has a higher pitch than the first, returns 1, if they are equal returns 0, if it is lower -1
        /// It is saying if the notes are going up or down
        /// </summary>
        public int GeneralDirection
        {
            get
            {
                return Math.Sign(Notes[Notes.Count - 1].Pitch - Notes[0].Pitch);
            }
        }

        /// <summary>
        /// The absolute value of the difference in pitches between the last note and the first note
        /// A large step means a big difference, regardless if the notes are going up or down
        /// </summary>
        public int Step
        {
            get
            {
                return Math.Abs(Notes[Notes.Count - 1].Pitch - Notes[0].Pitch);
            }
        }

        /// <summary>
        /// Similar to the Zigzag index, but for duration. When there is change in duration of 2 consecutive notes, the index increases
        /// A group of n notes of equal duration has a RythmChangeIndex of 0, if there is a change in duration in every single note
        /// the index is 1
        /// </summary>
        public double RythmChangeIndex
        {
            get
            {
                if (Notes.Count < 3) return 0;

                var totalRythmChanges = 0;
                for (var i = 1; i < Notes.Count - 1; i++)
                {

                    if (Notes[i + 1].DurationInTicks / (double)Notes[i].DurationInTicks > 1.3 || Notes[i + 1].DurationInTicks / (double)Notes[i].DurationInTicks < 0.7)
                    {
                        totalRythmChanges++;
                    }
                }
                return totalRythmChanges / (double)(Notes.Count - 2);
            }
        }
        /// <summary>
        /// Similar to the IntervalSpectrum, but for durations.
        /// We discretize the durations so the possible durations are:
        /// 6 (semifusa), 8, 12 (fusa), 16, 24 (semicorchea), 32, 36, 48 (corchea), 64, 72, 96 (negra), 144, 192 (blanca), 240,288, 384
        /// </summary>
        public Dictionary<int, int> RythmSpectrum
        {
            get
            {
                var retObj = new Dictionary<int, int>();
                foreach (var n in Notes)
                {
                    var standardDuration = GetNearestStandardDuration(n.DurationInTicks);
                    if (retObj.Keys.Contains(standardDuration))
                        retObj[standardDuration]++;
                    else
                        retObj[standardDuration] = 1;
                }
                return retObj;
            }
        }

        private int GetNearestStandardDuration(int duration)
        {
            if (duration < 7)
                return 6;
            if (duration < 10)
                return 8;
            if (duration < 15)
                return 12;
            if (duration < 20)
                return 16;
            if (duration < 28)
                return 24;
            if (duration < 35)
                return 32;
            if (duration < 40)
                return 36;
            if (duration < 59)
                return 48;
            if (duration < 70)
                return 64;
            if (duration < 82)
                return 72;
            if (duration < 110)
                return 96;
            if (duration < 160)
                return 144;
            if (duration < 220)
                return 192;
            if (duration < 260)
                return 240;
            if (duration < 320)
                return 288;

            return 384;

        }

        /// <summary>
        /// Ornaments are 1, 2 or 3 short notes played before a long note
        /// It can also be a couple of close pitch notes played alternatively (a trill)
        /// </summary>
        public double OrnamentIndex
        {
            get
            {
                return GetNumberOrOrnamentNotes(Notes) / (double)Notes.Count;
            }
        }


        private static int GetNumberOrOrnamentNotes(List<Note> notes)
        {
            return GetNumberOfEmbellishments(notes) + GetNumberOfTrillNotes(notes);
        }
        /// <summary>
        /// Given a group of consecutive notes, returns the number of embellishment notes
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static int GetNumberOfEmbellishments(List<Note> notes)
        {
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            var totalEmbellishments = 0;
            for (int i = 0; i < orderedNotes.Count; i++)
            {
                var previousNote = i == 0 ? null : orderedNotes[i - 1];
                for (int j = 2; j < 5 && i + j <= orderedNotes.Count; j++)
                {
                    var followingNote = (i + j) < orderedNotes.Count ? orderedNotes[i + j] : null;
                    if (IsAnEmbelishment(previousNote, orderedNotes.GetRange(i, j), followingNote))
                    {
                        totalEmbellishments += j - 1;
                    }
                }
            }
            return totalEmbellishments;
        }

        /// It checks if notes consists of 3 or less short notes followed by 1 long note (where short means less than a sixteenth and long a sixteen or longer)
        /// The note before the group of notes must be a long note (more than a sixteenth)
        /// </summary>
        /// <param name="previousNote">The note before the group of notes, if there is one</param>
        /// <param name="notes">Group of notes that may be an embelishment. If we return true, it means they really are</param>
        /// <param name="followingNote">The note after the group of notes, if there is one</param>
        /// <returns></returns>
        private static bool IsAnEmbelishment(Note? previousNote, List<Note> notes, Note? followingNote)
        {
            // If the time between the start of the previous note and the group of notes is shorter than a sixteenth, then we don't consider it an embellishments, but a rapid passage
            if (previousNote != null && notes.Min(x => x.StartSinceBeginningOfSongInTicks) - previousNote.StartSinceBeginningOfSongInTicks < 21)
                return false;
            // if the number of notes in the group is not between 2 and 4 we don't consider it an embelishment
            if (notes.Count > 4 || notes.Count < 2)
                return false;

            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            var lastNote = orderedNotes[orderedNotes.Count - 1];
            var firstNotes = orderedNotes.GetRange(0, orderedNotes.Count - 1);
            // If last note in the group is not a "long" note (at least a sixteenth), then it is not an embellishment
            if ((followingNote != null && followingNote.StartSinceBeginningOfSongInTicks - lastNote.StartSinceBeginningOfSongInTicks < 21) ||
            (followingNote == null && lastNote.DurationInTicks < 21))
                return false;

            // If any of the first group of notes (all but the last) is longer than a 32nd, then it is not an embellishment
            for (var i = 0; i < orderedNotes.Count - 1; i++)
            {
                if (orderedNotes[i + 1].StartSinceBeginningOfSongInTicks - orderedNotes[i].StartSinceBeginningOfSongInTicks >= 10)
                    return false;
            }
            return true;
        }


        /// <summary>
        /// A trill, also known as a "shake", is a rapid alternation between an indicated note and the one above it.
        /// Returns all notes that belong to trills
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static int GetNumberOfTrillNotes(List<Note> notes)
        {
            var totalTrillNotes = 0;
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            for (int i = 0; i < orderedNotes.Count - 5; i++)
            {
                var j = 1;
                while (j + i < orderedNotes.Count && IsAtrill(orderedNotes.GetRange(i, j)))
                {
                    j++;
                }
                if (j > 5)
                {
                    totalTrillNotes += j;
                }
                i += j;
            }
            return totalTrillNotes;
        }

        private static bool IsAtrill(List<Note> notes)
        {
            // Check they are all short
            if (notes.Where(x => x.DurationInTicks > 15).Count() > 0) return false;
            // Check there are only 2 different pitches
            if (notes.Select(x => x.Pitch).Distinct().Count() > 2) return false;
            // If the difference between the 2 notes is more than 3 semitones, it is not a trill
            if (notes.Select(x => x.Pitch).Max() - notes.Select(x => x.Pitch).Min() > 3) return false;
            // Check that goes up and down alternatively
            for (int j = 0; j < notes.Count() - 2; j++)
            {
                if ((notes[j + 1].Pitch - notes[j].Pitch) * (notes[j + 2].Pitch - notes[j + 1].Pitch) >= 0)
                    return false;
            }
            return true;
        }

        public bool IsSegmentUniform()
        {
            return
                IsSegmentUniform(SegmentPropertiesEnum.GeneralDirection) &&
                IsSegmentUniform(SegmentPropertiesEnum.IntervalSpectrum) &&
                IsSegmentUniform(SegmentPropertiesEnum.Range) &&
                IsSegmentUniform(SegmentPropertiesEnum.RythmChangeIndex) &&
                IsSegmentUniform(SegmentPropertiesEnum.RythmSpectrum) &&
                IsSegmentUniform(SegmentPropertiesEnum.Step) &&
                IsSegmentUniform(SegmentPropertiesEnum.ZigzagIndex);
        }

        /// <summary>
        /// We consider a segment uniform on a specific property, if we consider subsegments of it and the property doesn't change much
        /// </summary>
        /// <returns></returns>
        public bool IsSegmentUniform(SegmentPropertiesEnum prop)
        {
            var subSegments = GetSubSegments(Notes);
            foreach (var s in subSegments)
            {
                var segProp = new SegmentProperties(s);
                switch (prop)
                {
                    case SegmentPropertiesEnum.GeneralDirection:
                        if (this.GeneralDirection != segProp.GeneralDirection)
                            return false;
                        break;
                    case SegmentPropertiesEnum.IntervalSpectrum:
                       if ( this.IntervalSpectrum.Count == segProp.IntervalSpectrum.Count &&
                            !this.IntervalSpectrum.Except(segProp.IntervalSpectrum).Any())
                        return false;
                        break;
                    case SegmentPropertiesEnum.OrnamentIndex:
                        break;
                    case SegmentPropertiesEnum.Range:
                        if (this.Range != segProp.Range)
                            return false;
                        break;
                    case SegmentPropertiesEnum.Step:
                        if (this.Step != segProp.Step)
                            return false;
                        break;
                    case SegmentPropertiesEnum.RythmChangeIndex:
                        if (Math.Abs(this.RythmChangeIndex - segProp.RythmChangeIndex) > 0.1)
                            return false;
                        break;
                    case SegmentPropertiesEnum.RythmSpectrum:

                        if (this.RythmSpectrum.Count == segProp.RythmSpectrum.Count &&
                             !this.RythmSpectrum.Except(segProp.RythmSpectrum).Any())
                            return false;
                            break;
                    case SegmentPropertiesEnum.ZigzagIndex:
                        if (Math.Abs(this.ZigzagIndex - segProp.ZigzagIndex) > 0.1)
                            return false;
                        break;
                }
            }
            return true;
        }

       /// <summary>
       /// We get subsegments by dividing a segment in groups of the same quantity of notes
       /// We divide the group in 1, then in 2, then in 3, etc until we have less than 4 notes in a subgroup
       /// </summary>
       /// <param name="notes"></param>
       /// <returns></returns>
        private static List<List<Note>> GetSubSegments(List<Note> notes)
        {
            int minNotes = 4;
            var retObj = new List<List<Note>>();
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            int div = 1;
            while (notes.Count / div >= minNotes)
            {
                var notesPerSubsegment = notes.Count / div;
                for (int i = 0; i < div; i++)
                {
                    retObj.Add(orderedNotes.Skip(notesPerSubsegment * i).Take(notesPerSubsegment).ToList());
                }
                div++;
            }
            return retObj;
        }
    }

    public enum SegmentPropertiesEnum
    {
        GeneralDirection,
        IntervalSpectrum,
        OrnamentIndex,
        Range,
        RythmChangeIndex,
        RythmSpectrum,
        Step,
        ZigzagIndex
    }
}

