using Sinphinity.Models;

namespace SinphinityProcMelodyAnalyser.MelodyLogic
{
    /// <summary>
    /// The idea is that a phrase may contain notes that are not very important, and that we can remove and still
    /// have a meaningful phrase that is not much different from the original
    /// These are the simplifications:
    /// - We remove very short notes between longer notes (ornaments)
    /// - We remove passing notes
    /// 
    /// </summary>
    public static class PhraseSkeleton
    {
        public static Phrase GetSkeleton(Phrase p)
        {
            var retObj = RemoveShortNotesBetweenLongNotes(p);
            retObj = RemovePassingNotes(retObj);
            return (RemoveRepeatingNotes(retObj));
        }

        public static Phrase RemoveShortNotesBetweenLongNotes(Phrase p)
        {
            var maxLengthOfShortNote = 9;
            var maxLengthOfContiguousShortNotes = 11;
            var newPitchItems = new List<int>() { p.PitchItems[0]};
            var newMetricItems = new List<int>() { p.MetricItems[0] };
            var lastNoteAdded = 0;
            for (var i = 1; i < p.NumberOfNotes - 1; i++)
            {
                // Check if it is a long note
                if (p.MetricItems[i] > maxLengthOfShortNote)
                {
                    if (lastNoteAdded == i - 1)
                    {
                        newPitchItems.Add(p.PitchItems[i]);
                        newMetricItems.Add(p.MetricItems[i]);
                    }
                    else
                    {
                        //look at the short notes before this long note
                        var newPitch = 0;
                        var newMetric = 0;
                        for (var j = lastNoteAdded + 1; j < i; j++)
                        {
                            newPitch += p.PitchItems[j];
                            newMetricItems[newMetricItems.Count - 1] += p.MetricItems[j];
                        }
                        // if the sum of ticks of the consecutive short notes is small, remove the short notes
                        if (newMetric < maxLengthOfContiguousShortNotes)
                        {
                            newPitchItems.Add(newPitch + p.PitchItems[i]);
                            newMetricItems.Add(p.MetricItems[i]);
                        }
                        //otherwise, keep them
                        else
                        {
                            for (var j = lastNoteAdded + 1; j <= i; j++)
                            {
                                newPitchItems.Add(p.PitchItems[j]);
                                newMetricItems.Add(p.MetricItems[j]);
                            }
                        }
                    }
                    lastNoteAdded = i;
                }
            }
            var pitchesAsString = string.Join(",", newPitchItems);
            var metricsAsString = string.Join(",", newMetricItems);
            return new Phrase(metricsAsString, pitchesAsString);
        }

        public static Phrase RemovePassingNotes(Phrase p)
        {
            var newPitchItems = new List<int>();
            var newMetricItems = new List<int>();
            var lastNoteAdded = -1;
            for (var i = 0; i < p.NumberOfNotes - 2; i++)
            {
                // Check if pitches change direction or keep going up or down
                if (p.PitchItems[i] * p.PitchItems[i + 1] <= 0)
                {
                    if (lastNoteAdded == i - 1)
                    {
                        newPitchItems.Add(p.PitchItems[i]);
                        newMetricItems.Add(p.MetricItems[i]);
                    }
                    else
                    {
                        var newPitch = 0;
                        var newMetric = 0;
                        // keep moving 
                        for (var j = lastNoteAdded + 1; j <= i; j++)
                        {
                            newPitch += p.PitchItems[j];
                            newMetric += p.MetricItems[j];
                        }
                        newPitchItems.Add(newPitch);
                        newMetricItems.Add(newMetric);
                    }
                    lastNoteAdded = i;
                }
            }
            newPitchItems.Add(p.PitchItems[p.NumberOfNotes - 2]);
            newMetricItems.Add(p.MetricItems[p.NumberOfNotes - 2]);
            var pitchesAsString = string.Join(",", newPitchItems);
            var metricsAsString = string.Join(",", newMetricItems);
            return new Phrase(metricsAsString, pitchesAsString);
        }

        public static Phrase RemoveRepeatingNotes(Phrase p)
        {
            var newPitchItems = new List<int>();
            var newMetricItems = new List<int>();
            var lastNoteAdded = -1;
            for (var i = 0; i < p.NumberOfNotes - 1; i++)
            {
                if (p.PitchItems[i] != 0)
                {
                    if (lastNoteAdded == i - 1)
                    {
                        newPitchItems.Add(p.PitchItems[i]);
                        newMetricItems.Add(p.MetricItems[i]);
                    }
                    else
                    {
                        var newMetrics = 0;
                        for (var j = lastNoteAdded + 1; j <= i; j++)
                        {
                            newMetrics += p.MetricItems[j];
                        }
                        newPitchItems.Add(p.PitchItems[i]);
                        newMetricItems.Add(newMetrics);
                    }
                    lastNoteAdded = i;
                }
            }
            var pitchesAsString = string.Join(",", newPitchItems);
            var metricsAsString = string.Join(",", newMetricItems);
            return new Phrase(metricsAsString, pitchesAsString);
        }
    }
}
