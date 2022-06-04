using Sinphinity.Models;
using System.Text.RegularExpressions;

namespace SinphinityProcMelodyAnalyser.BusinessLogic
{
    public static partial class PhraseDetection
    {

        /// <summary>
        /// Looks for sections of notes between edges that consist of a pattern that repeats itself and adds edges to separate the part that consist of a repeating pattern
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        public static HashSet<long> GetRepeatingPatterns(List<Note> notes, HashSet<long> edgesSoFar)
        {
            edgesSoFar = GetRepeatingPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.Both);
            edgesSoFar = GetRepeatingPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.Metrics);
            edgesSoFar = GetRepeatingPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.Pitches);
            return GetRepeatingPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.PitchDirection);
        }
        public static HashSet<long> GetRepeatingPatternsOfType(List<Note> notes, HashSet<long> edgesSoFar, PhraseTypeEnum type)
        {
            var edges = edgesSoFar.ToList().OrderBy(x => x).ToList();
            var edgesToAdd = new HashSet<long>();
            for (var i = 0; i < edges.Count - 1; i++)
            {
                var intervalNotes = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= edges[i] && x.StartSinceBeginningOfSongInTicks < edges[i + 1]).ToList();
                var newEdges = GetRepeatingNotePatternSection(intervalNotes, type);
                edgesToAdd = edgesToAdd.Union(newEdges).ToHashSet();

            }
            return edgesSoFar.Union(edgesToAdd).ToHashSet();
        }


        /// <summary>
        /// Given a group of consecutive notes (or metric intervals or pitches, depending on the parameter "type"), looks for a subset of them that consist of a repeating pattern
        /// and returns the start and end of the repeating pattern section
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>

        private static HashSet<long> GetRepeatingNotePatternSection(List<Note> notes, PhraseTypeEnum type)
        {
            var retObj = new HashSet<long>();
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            // if less than 8 notes leave it alone
            var totalNotes = notes.Count;
            if (totalNotes < 8) return retObj;
            // if total duration is not longer than 8 quarters and total of notes not more than 20 leave it alone
            // we actually check the product intervalDurationInTicks * totalNotes to consider cases where we have shorter intervals but many notes
            var intervalDurationInTicks = orderedNotes.Max(x => x.EndSinceBeginningOfSongInTicks) - orderedNotes.Min(y => y.StartSinceBeginningOfSongInTicks);
            if (intervalDurationInTicks * totalNotes < 8 * 96 * 12)
                return retObj;
            // we use this variable to avoid testing the same value twice
            var patternsAlreadyTested = new List<string>();

            // pat is a regex pattern to search for i consecutive notes
            string asString = GetNotesAsString(notes, type);


            for (var i = 1; i < notes.Count / 2; i++)
            {
                // pat is a regex pattern to search for i consecutive notes
                string pat;
                char noteSeparator;
                switch (type)
                {
                    case PhraseTypeEnum.Both:
                        pat = string.Concat(Enumerable.Repeat("[0-9]+,[-]?[0-9]+;", i));
                        noteSeparator = ';';
                        break;
                    case PhraseTypeEnum.Metrics:
                        pat = string.Concat(Enumerable.Repeat("[0-9]+,", i));
                        noteSeparator = ',';
                        break;
                    case PhraseTypeEnum.Pitches:
                        pat = string.Concat(Enumerable.Repeat("[-]?[0-9]+,", i));
                        noteSeparator = ',';
                        break;
                    case PhraseTypeEnum.PitchDirection:
                        pat = string.Concat(Enumerable.Repeat("[-+0],", i));
                        noteSeparator = ',';
                        break;
                    default:
                        throw new Exception("Que mierda me mandaron?");
                }
                foreach (Match match in Regex.Matches(asString, pat))
                {
                    // We add this test to avoid checking twice the same thing. the Regex.Matches doesn't return unique values
                    if (patternsAlreadyTested.Contains(match.Value))
                        continue;
                    patternsAlreadyTested.Add(match.Value);

                    // j is the times we repeat the pattern and then check if that repetition of patterns actually is present
                    for (var j = notes.Count / i; j > 2; j--)
                    {
                        // pat2 is the sequence of i consecutive notes repeated j times
                        // if the next "if" is true, it means the sequence "match.value" appear repeated j times
                        var pat2 = string.Concat(Enumerable.Repeat(match.Value, j));

                        if (asString.Contains(pat2))
                        {
                            var start = asString.IndexOf(pat2);
                            var notesBeforeStart = asString.Substring(0, start).Count(x => x == noteSeparator);
                            var totalNotesInRepeatedPattern = pat2.Count(x => x == noteSeparator);
                            // if we found something like x,x,x,x,x we don't want to search latter for x,x or x,x,x etc. so we add them to the patternsAlreadyTested list
                            for (var k = 2; k <= j; k++)
                                patternsAlreadyTested.Add(string.Concat(Enumerable.Repeat(match.Value, k)));

                            if (totalNotesInRepeatedPattern > 5)
                            {
                                // we add the point where the repeated pattern starts to the list of edges
                                var beginningOfPatternSection = orderedNotes[notesBeforeStart].StartSinceBeginningOfSongInTicks;
                                retObj.Add(beginningOfPatternSection);
                                // we add the point where the repeated pattern end to the list of edges

                                long endOfPatternSection = notes[notes.Count - 1].EndSinceBeginningOfSongInTicks;
                                if (notesBeforeStart + totalNotesInRepeatedPattern < notes.Count)
                                {
                                    endOfPatternSection = orderedNotes[notesBeforeStart + totalNotesInRepeatedPattern].EndSinceBeginningOfSongInTicks;
                                    retObj.Add(endOfPatternSection);
                                }

                                var patternDuration = orderedNotes[notesBeforeStart + i].EndSinceBeginningOfSongInTicks - orderedNotes[notesBeforeStart].StartSinceBeginningOfSongInTicks;
                                var patternSectionDuration = endOfPatternSection - beginningOfPatternSection;
                                // break at the beginning of each repetition if we are going to have a phrase that is too long
                                if (patternDuration > 4 * 96 || patternSectionDuration > 12 * 96)
                                {
                                    for (var m = 1; m < j; m++)
                                    {
                                        retObj.Add(orderedNotes[notesBeforeStart + m * i].StartSinceBeginningOfSongInTicks);
                                    }
                                }
                            }
                            // if we found j consecutive matches, there is no point to keep trying with smaller values of j, so break and continue with the next i
                            break;
                        }
                    }
                }
            }
            return retObj;
        }


        /// <summary>
        /// Creates strings like 48,3;24,-1;24,2
        /// that represent consecutive relative notes, using a comma to separate duration from pitch and semicolon between consecutive notes
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static string GetNotesAsString(List<Note> notes, PhraseTypeEnum type)
        {
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            var asString = "";
            switch (type)
            {
                case PhraseTypeEnum.Both:
                    for (var i = 0; i < orderedNotes.Count - 1; i++)
                    {
                        asString += $"{orderedNotes[i + 1].StartSinceBeginningOfSongInTicks - orderedNotes[i].StartSinceBeginningOfSongInTicks},{orderedNotes[i + 1].Pitch - orderedNotes[i].Pitch};";
                    }
                    return asString;
                case PhraseTypeEnum.Metrics:
                    return string.Join(",", orderedNotes.Select(x => x.DurationInTicks.ToString())) + ",";
                case PhraseTypeEnum.Pitches:
                    return string.Join(",", orderedNotes.Select(x => x.Pitch.ToString())) + ",";
                case PhraseTypeEnum.PitchDirection:
                    for (var j = 1; j < orderedNotes.Count; j++)
                    {
                        var value = Math.Sign(orderedNotes[j].Pitch - orderedNotes[j - 1].Pitch);
                        switch (value)
                        {
                            case -1:
                                asString += "-,";
                                break;
                            case 0:
                                asString += "0,";
                                break;
                            case 1:
                                asString += "+,";
                                break;
                            default:
                                throw new Exception("Que mierda me mandaron?");
                        }
                    }
                    return asString;
                default:
                    throw new Exception("Que mierda me mandaron?");
            }
        }

    }
}
