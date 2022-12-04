using Sinphinity.Models;
using System.Text.RegularExpressions;

namespace SinphinityProcMelodyAnalyser.MelodyLogic
{
    public static class PatternDetection
    {

        /// <summary>
        /// Looks for sections of notes between edges that consist of a pattern that repeats itself and adds edges to separate the part that consist of a repeating pattern
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="edgesSoFar"></param>
        /// <returns></returns>
        public static HashSet<long> GetRepeatingPatterns(List<Note> notes, HashSet<long> edgesSoFar)
        {
            var newEdges = GetNonContiguousPatterns(notes, PhraseTypeEnum.PitchDirectionAndMetrics, 7, edgesSoFar);
            if (newEdges != null && newEdges.Count > 0)
                edgesSoFar = edgesSoFar.Union(newEdges).ToHashSet();

            edgesSoFar = GetRepeatingPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.Both);
            edgesSoFar = GetRepeatingPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.Metrics);
            edgesSoFar = GetRepeatingPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.Pitches);
            edgesSoFar = GetRepeatingPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.PitchDirection);
            edgesSoFar = GetRepeatingNonContiguousPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.Both);
            edgesSoFar = GetRepeatingNonContiguousPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.Metrics);
            edgesSoFar = GetRepeatingNonContiguousPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.Pitches);
            return GetRepeatingNonContiguousPatternsOfType(notes, edgesSoFar, PhraseTypeEnum.PitchDirection);
        }
        public static HashSet<long> GetRepeatingPatternsOfType(List<Note> notes, HashSet<long> edgesSoFar, PhraseTypeEnum type)
        {
            var edges = edgesSoFar.ToList().OrderBy(x => x).ToList();
            var edgesToAdd = new HashSet<long>();
            for (var i = 0; i < edges.Count - 1; i++)
            {
                var intervalNotes = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= edges[i] && x.StartSinceBeginningOfSongInTicks < edges[i + 1]).ToList();
                var newEdges = GetRepeatingNotePatternSection(intervalNotes, type);

                // before adding the edges, we check that we will not create phrases that are too short
                foreach (var edge in newEdges)
                {
                    if (!PhraseAnalysis.WillNewEdgeCreatePhraseTooShort(notes, edgesSoFar, edge))
                        edgesSoFar.Add(edge);
                }
            }
            return edgesSoFar;
        }

      
        public static HashSet<long> GetRepeatingNonContiguousPatternsOfType(List<Note> notes, HashSet<long> edgesSoFar, PhraseTypeEnum type)
        {
            var edges = edgesSoFar.ToList().OrderBy(x => x).ToList();
            var edgesToAdd = new HashSet<long>();
            for (var i = 0; i < edges.Count - 1; i++)
            {
                var intervalNotes = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= edges[i] && x.StartSinceBeginningOfSongInTicks < edges[i + 1]).ToList();
                var newEdges = GetNonContiguousPatterns(intervalNotes, type);
                edgesToAdd = edgesToAdd.Union(newEdges).ToHashSet();

            }
            return edgesSoFar.Union(edgesToAdd).ToHashSet();
        }


        /// <summary>
        /// Given a group of consecutive notes (or metric intervals or pitches, depending on the parameter "type"), looks for a subset of them 
        /// that consist of a repeating pattern and returns the start and end of the repeating pattern section
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>

        private static HashSet<long> GetRepeatingNotePatternSection(List<Note> notes, PhraseTypeEnum type)
        {
            var retObj = new HashSet<long>();
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            var totalNotes = notes.Count;
            var intervalDurationInTicks = orderedNotes.Max(x => x.EndSinceBeginningOfSongInTicks) - orderedNotes.Min(y => y.StartSinceBeginningOfSongInTicks);

            // to search for patterns we expect that we have at least 8 notes
            // we dont't want to produce short intervals with few notes, we check the product duration * qty notes
            if (totalNotes < 8 || intervalDurationInTicks * totalNotes < 8 * 192)
                return retObj;

            // we use this variable to avoid testing the same value twice
            var patternsAlreadyTested = new List<string>();

            // we use regex pattern matching so we convert the notes to a string
            string asString = GetNotesAsString(notes, type);


            for (var i = 1; i < notes.Count / 2; i++)
            {
                // pat is a regex pattern to search for groups of i consecutive notes
                (var pat, var noteSeparator) = GetRegexPatternAndSeparatorForTypeAndLength(type, i);

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
                        var pattern = match.Value;
                        var patternLength = pattern.Count(x => x == noteSeparator);
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
                                // break at the beginning of each repetition if we are going to have a phrase that is too long or if the pattern is repeated too many times
                                if (patternDuration > 4 * 96 || patternSectionDuration > 12 * 96 || (j > 4 && patternLength > 3))
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
        /// Returns the regex pattern to use to look for repeating patterns of a specific type and a specific length
        /// </summary>
        /// <param name="type"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static (string,char) GetRegexPatternAndSeparatorForTypeAndLength(PhraseTypeEnum type, int i)
        {
            switch (type)
            {
                case PhraseTypeEnum.Both:
                    return (string.Concat(Enumerable.Repeat("[0-9]+,[-]?[0-9]+;", i)), ';');
                case PhraseTypeEnum.Metrics:
                    return (string.Concat(Enumerable.Repeat("[0-9]+,", i)), ',');
                case PhraseTypeEnum.Pitches:
                    return (string.Concat(Enumerable.Repeat("[-]?[0-9]+,", i)), ',');
                case PhraseTypeEnum.PitchDirection:
                    return (string.Concat(Enumerable.Repeat("[-+0],", i)), ',');
                case PhraseTypeEnum.PitchDirectionAndMetrics:
                    return (string.Concat(Enumerable.Repeat("[0-9]+,[-+0];", i)), ';');
                default:
                    throw new Exception("Que mierda me mandaron?");
            }
        }


        /// <summary>
        /// Creates strings like 48,3;24,-1;24,2
        /// that represent consecutive relative notes, using a comma to separate duration from pitch and semicolon between consecutive notes
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        public static string GetNotesAsString(List<Note> notes, PhraseTypeEnum type)
        {
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            var asString = "";
            switch (type)
            {
                case PhraseTypeEnum.Both:
                    for (var i = 0; i < orderedNotes.Count - 1; i++)
                    {
                        asString += $"{orderedNotes[i + 1].StartSinceBeginningOfSongInTicks - orderedNotes[i].StartSinceBeginningOfSongInTicks}," +
                            $"{orderedNotes[i + 1].Pitch - orderedNotes[i].Pitch};";
                    }
                    return asString;
                case PhraseTypeEnum.Metrics:
                    return string.Join(",", orderedNotes.Select(x => x.DurationInTicks.ToString())) + ",";
                case PhraseTypeEnum.Pitches:
                    // for pitches, we don't look for exact matches, we want a tolerance of 1 semitone
                    for (var i = 0; i < orderedNotes.Count - 1; i++)
                    {
                        asString += (orderedNotes[i + 1].Pitch - orderedNotes[i].Pitch).ToString() + ",";
                    }
                    return asString;
                case PhraseTypeEnum.PitchDirection:
                    for (var i = 0; i < orderedNotes.Count-1; i++)
                    {
                        var value = Math.Sign(orderedNotes[i+1].Pitch - orderedNotes[i].Pitch);
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
                case PhraseTypeEnum.PitchDirectionAndMetrics:
                    for (var i = 0; i < orderedNotes.Count - 1; i++)
                    {
                        var value = Math.Sign(orderedNotes[i+1].Pitch - orderedNotes[i].Pitch);
                        switch (value)
                        {
                            case -1:
                                asString += $"{orderedNotes[i + 1].StartSinceBeginningOfSongInTicks - orderedNotes[i].StartSinceBeginningOfSongInTicks},-;";
                                break;
                            case 0:
                                asString += $"{orderedNotes[i + 1].StartSinceBeginningOfSongInTicks - orderedNotes[i].StartSinceBeginningOfSongInTicks},0;";
                                break;
                            case 1:
                                asString += $"{orderedNotes[i + 1].StartSinceBeginningOfSongInTicks - orderedNotes[i].StartSinceBeginningOfSongInTicks},+;";
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


        /// <summary>
        /// When after applying all the other crieteria to break a sequences of notes in smaller pieces we still have long sequences,
        /// we look for sections that appear in different parts, but they are not contiguous (we already looked for a pattern repeting
        /// contiguosuly). We look for the longest repeating pattern and we break where the pattern starts and ends
        /// </summary>
        public static HashSet<long> GetNonContiguousPatterns(List<Note> notes, PhraseTypeEnum type, int minPatternLength = 4, HashSet<long> edgesSoFar = null)
        {
            var retObj = new HashSet<long>();
            var orderedNotes = notes.OrderBy(x => x.StartSinceBeginningOfSongInTicks).ToList();
            var totalNotes = notes.Count;
            var intervalDurationInTicks = orderedNotes.Max(x => x.EndSinceBeginningOfSongInTicks) - orderedNotes.Min(y => y.StartSinceBeginningOfSongInTicks);
           

            // to search for patterns we expect that we have at least 16 notes
            // we dont't want to produce short intervals with few notes, we check the product duration * qty notes
            if (totalNotes < 16 || intervalDurationInTicks * totalNotes < 16 * 384)
                return retObj;

            // if the group of notes consists of a repeating contiguous pattern we don't want to split it
            if (DoesPhraseConsistOfARepeatingContiguousPattern(notes, type))
                return retObj;

            // we use this variable to avoid testing the same value twice
            var patternsAlreadyTested = new List<string>();

            // we use regex pattern matching so we convert the notes to a string
            string asString = GetNotesAsString(notes, type);

            var maxPatternLengthToSearch = Math.Min(25, notes.Count / 2);

            for (var i = maxPatternLengthToSearch; i >= minPatternLength; i--)
            {
                // pat is a regex pattern to search for groups of i consecutive notes
                (var pat, var noteSeparator) = GetRegexPatternAndSeparatorForTypeAndLength(type, i);


                foreach (Match match in Regex.Matches(asString, pat))
                {
                    // We add this test to avoid checking twice the same thing. the Regex.Matches doesn't return unique values
                    if (patternsAlreadyTested.Contains(match.Value))
                        continue;
                    patternsAlreadyTested.Add(match.Value);

                    // match is any sequence of i consecutive notes, we now try to see if this sequence happens more than once
                    // we need to escape the symbols + and - because they have special meanings.
                    var patito = match.Value.Replace("+", "\\+").Replace("-", "\\-");
                    var matches = Regex.Matches(asString, patito);
                    if (matches.Count == 1)
                        continue;

                    // if we are here is because we found a sequence of i notes that happens at least twice
                    for (var j=0;j< matches.Count;j++)
                    {
                        var m = matches[j];
                        // startCharacterIndex is the index in the asString string where the pattern starts
                        var startCharacterIndex = m.Index;
                        // startingNoteIndex is the index in the sequence of notes where the pattern starts
                        var startingNoteIndex = asString.Substring(0, startCharacterIndex).Split(noteSeparator).Count();

                        var patternStartTick = notes[startingNoteIndex].StartSinceBeginningOfSongInTicks;
                        var pattternEndTick = startingNoteIndex + i + 1 < notes.Count ?
                                    notes[startingNoteIndex + i + 1].StartSinceBeginningOfSongInTicks :
                                    notes[notes.Count - 1].EndSinceBeginningOfSongInTicks;

                        // Check that there isn't an edge inside the pattern
                        var allEdges = edgesSoFar != null ? edgesSoFar.Concat(retObj).ToHashSet() : retObj;
                        if (edgesSoFar != null && allEdges.Where(x => x > patternStartTick && x < pattternEndTick).Any())
                            continue;

                        // Check that we don't create short intervals
                        var edgeBefore = allEdges.Where(x => x < patternStartTick).OrderByDescending(y => y).FirstOrDefault();
                        var edgeAfter = allEdges.Where(x => x > pattternEndTick).Any() ?
                                    allEdges.Where(x => x > pattternEndTick).OrderBy(y => y).FirstOrDefault() :
                                    notes[notes.Count - 1].EndSinceBeginningOfSongInTicks;
                        var minLength = 192;
                        if (patternStartTick - edgeBefore < minLength || edgeAfter - pattternEndTick < minLength)
                            continue;

                        var notesGroupThatWillBeSplitted = notes.Where(x => x.StartSinceBeginningOfSongInTicks >= edgeBefore &&
                        x.StartSinceBeginningOfSongInTicks < edgeAfter).ToList();
                        // Add start and end of the pattern to hash of edges
                        if (!PhraseAnalysis.WillNewEdgeCreatePhraseTooShort(notesGroupThatWillBeSplitted, retObj, patternStartTick) &&
                            !WillNewEdgeBreakArepeatingPattern(notesGroupThatWillBeSplitted, type, patternStartTick))
                            retObj.Add(patternStartTick);   
                        if (!PhraseAnalysis.WillNewEdgeCreatePhraseTooShort(notesGroupThatWillBeSplitted, retObj, pattternEndTick) &&
                            !WillNewEdgeBreakArepeatingPattern(notesGroupThatWillBeSplitted, type, pattternEndTick))
                            retObj.Add(pattternEndTick);
                    }
                }
            }
            return retObj;
        } 

        private static bool WillNewEdgeBreakArepeatingPattern(List<Note> notes, PhraseTypeEnum type, long edge)
        {
            var repeatingPatternSection = GetRepeatingNotePatternSection(notes, type);
            if (repeatingPatternSection.Count == 2)
            {
                var start = repeatingPatternSection.Min();
                var end = repeatingPatternSection.Max();
                if (start < edge && edge < end)
                    return true;
            }
            return false;
        }

        public static bool DoesPhraseConsistOfARepeatingContiguousPattern(List<Note> notes, PhraseTypeEnum type)
        {
            var phrase = new Phrase(notes);
            switch (type) {
                case PhraseTypeEnum.Pitches:
                    return phrase.PitchesAsString.Contains("*") == true;
                case PhraseTypeEnum.Metrics:
                    return phrase.MetricsAsString.Contains("*") == true;
                case PhraseTypeEnum.Both:
                    return phrase.PitchesAsString.Contains("*") == true && phrase.MetricsAsString.Contains("*") == true;
                default:
                    return false;
            }
        }
    }
}
