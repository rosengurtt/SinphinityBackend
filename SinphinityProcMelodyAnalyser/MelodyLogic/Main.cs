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
    /// - remove the tracks that play chords
    /// - discretize the starts of the notes
    /// - find which of the remaining tracks have multiple voices (for ex. a piano track where the left hand and the right hand play independent melodies)
    /// - split the ployphoinic tracks split it in 2 voices (a lower one and a higher one) using the SubVoice field of the Note objects
    /// 
    /// 
    /// 2. Phrases detection
    /// Melodies tipically have phrases. When a track corresponds to a human singing a melody, there are usually intervals for the singer to breath and there is a clear
    /// separation of phrases. Even when the track is played by an instrument, tipically there are places where there is a large silence (or rest) or a note plays for a longer time
    /// than any of the neighboor notes.
    /// We split a melody in phrases with the following steps:
    /// 
    /// - we look for relatively long intervals of time between the start of consecutive notes (we don't care if there is a rest or there is a note playing all that time). We split the
    /// melody in these points
    /// 
    /// - we then look inside of the phrases obtained in the previous step (some can be pretty long) and we look for places where the rythm or the pitches change. 
    /// For example the phrase can start with a scale going up followed by an arpeggio going down a particular chord. We divide that phrase in 2. Each subphrase has a consistency
    /// that keeps it similar to itself. When there is a change in some of the properties listed below(like going down instead of up and jumping in 3rds and 4ths instead of 2nds like in the previous example) we consider it a phrase boundary and we split 
    /// the phrase.
    /// 
    /// Properties used to split phrases
    /// PITCHES
    /// - ZigzagIndex (a double)
    /// The number of changes in pitch direction, divided by the number of pitches minus 2
    /// - IntervalSpectrum (12 doubles separated by commas, where the first double corresponds to unisons, the second to 2nds, etc. The 12th double is for any intervals longer than an 11th)
    /// The number of seconds, thirds, fourths, etc  divided by number of notes
    /// - TotalNumberOfIntervals (integer)
    /// This is the total number of non zero values in the IntervalSpectrum
    /// - GeneralDirection
    /// The sign of the difference in picth of the last note minus the first note
    /// - TotalStep (integer)
    /// The absolute difference in pitch between the first note and the last
    /// - AverageInterval (double)
    /// This is the sum of the absolute value of all the relative notes, divided by the number of notes.
    /// 
    /// METRICS
    /// - RythmChangeIndex (double)
    /// The number of note duration changes divided the total number of notes
    /// - OrnamentIndex (double)
    /// The number of very short notes (shorter than 6 ticks) that are played before or after long notes divided by the total number of notes
    /// - RythmSpectrum (
    /// The number of quarters, eights, etc divided by total number of notes
    /// - TotalNumberOfDurations
    /// 
    /// 
    /// 3. Extract skeleton long complex phrases
    /// After all the splitting done before we could end up with all short phrases, but we can as well have some long phrases. In these cases we produce a simplified version
    /// of the phrase by removing passing notes and embelishments. We keep the 2 versions of the phrase as 2 different phrases, but we add a linkg from the complex version to the
    /// simpler one
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

        public async Task ProcessSong(long songId)
        {
            var song = await _sysStoreClient.GetSongByIdAsync(songId, 0);
            var notes = SongPreprocess.ExtractMelodies(song);

        }
    }
}
