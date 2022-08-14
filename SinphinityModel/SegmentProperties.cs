using Sinphinity.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sinphinity.Models
{
    public class SegmentProperties
    {
        public SegmentProperties(List<Note> notes)
        {
            Notes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
        }
        public List<Note> Notes { get; set; }
        /// <summary>
        /// The number of changes in pitch direction, divided by (the number of pitches minus 2)
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
        /// </summary>
        public int GeneralDirection
        {
            get
            {
                return Math.Sign(Notes[Notes.Count - 1].Pitch - Notes[0].Pitch);
            }
        }

        public int Step
        {
            get
            {
                return Math.Abs(Notes[Notes.Count - 1].Pitch - Notes[0].Pitch);
            }
        }

    
        public double RythmChangeIndex
        {
            get
            {
                if (Notes.Count < 3) return 0;

                var totalRythmChanges = 0;
                for (var i = 1; i < Notes.Count - 1; i++)
                {
                    
                    if (Notes[i+1].DurationInTicks/(double)Notes[i].DurationInTicks>1.3 || Notes[i + 1].DurationInTicks / (double)Notes[i].DurationInTicks <0.7)
                    {
                        totalRythmChanges++;
                    }
                }
                return totalRythmChanges / (double)(Notes.Count - 2);
            }
        }
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

    }
}
