using Sinphinity.Models;
using SinphinityProcMelodyAnalyser.Clients;

namespace SinphinityProcMelodyAnalyser.MelodyLogic
{

    /// Melodic analysis
    /// ****************
    /// We carry the following steps:
    /// 
    /// 1. Pre processing
    /// Before we start analyzing melodies in a song we need to do the following:
    /// 
    /// - remove the notes that are percussion notes  
    /// - remove the tracks that only play chords
    /// - find which of the remaining tracks have multiple voices (for ex. a piano track where the left hand and the right hand play independent 
    ///   melodies)
    /// - split the ployphoinic tracks in 2 voices (a lower one and a higher one) using the SubVoice field of the Note objects. Any intermediate
    ///   voice is discarded
    /// - discretize the starts of the notes
    /// 
    /// 
    /// 2. Phrases detection
    /// Melodies tipically have phrases. When a track corresponds to a human singing a melody, there are usually intervals for the singer to breath 
    /// and there is a clear separation of phrases. 
    /// Even when the track is played by an instrument, tipically there are places where there is a large silence (or rest) or a note plays 
    /// for a longer time than any of the neighboor notes.
    /// 
    /// We split a melody in phrases with the following steps:
    /// 
    /// - we look for relatively long intervals of time between the start of consecutive notes (we don't care if there is a rest or there is a 
    ///   note playing all that time). We split the melody in these points including the long note in the first of the 2 groups
    /// 
    /// - we look for relatively long groups of notes equally spaced (for example 10 consecutive sixteenths). We split the group of notes at 
    ///   the beginning and end of this groups (split by rithm)
    ///   
    /// - we look for relatively long groups of notes that are monotonically going up or down. We split at the beginning and end of these
    ///   groups (split by pitches)
    ///   
    /// - we look for groups of notes that have small pitch intervals between consecutive pithces, we split when suddenly there is large interval
    /// 
    /// - we for patterns in rythm, for ex. if we have "sixteenth, sixteenth, eight" and then again "sixteenth, sixteenth, eigth", we
    ///   split at the beginning and end of the group of consecutive patterns
    ///   
    /// - we look for patterns in pitch, but not being strict. For ex we may find the pattern "short interval up, short interval up, 
    ///   long interval down". We split at the beginning and end of such groups
    /// 
    /// 
    /// 3. Extract skeleton long complex phrases
    /// After all the splitting done before we could end up with all short phrases, but we can as well have some long phrases. In these cases we 
    /// produce a simplified version  of the phrase by removing passing notes and embelishments. We keep the 2 versions of the phrase as 2 different
    /// phrases, but we add a link from the complex version to the simpler one
    /// 
    /// 
    /// Artifacts produced
    /// ******************
    /// As a result of the anlyasis we have the following artifacts
    /// 
    /// - A table Phrases
    /// It includes phrases, segments and phrase skeletons. When a phrase has a skeleton, it has a foreign key to its skeleton
    /// 
    /// - A table PhrsesOccurrences
    /// There is 1 record for each instance of each phrase in a song.
    /// 
    /// 
    /// - A table PhraseLinks
    /// It has links of consecutive phrases in one voice and a reference to the song and the place in the song. There may be multiple links between 2 particular phrases
    /// It has also links of simultaneous or consecutive segments played in different voices.
    /// 
    /// 
    /// 
    /// 

    public class Main
    {
        private SysStoreClient _sysStoreClient;

        public Main(SysStoreClient sysStoreClient)
        {
            _sysStoreClient = sysStoreClient;
        }

        public async Task<List<ExtractedPhrase>> ProcessSong(long songId)
        {
            try
            {
                var song = await _sysStoreClient.GetSongByIdAsync(songId, 0);
                if (song.SongSimplifications.Count == 0 || song.Bars == null || song.Bars.Count == 0)
                    return null;
                var notes = SongPreprocess.ExtractMelodies(song);
                return PhraseAnalysis.FindAllPhrases(notes, song.Bars, songId);

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
