namespace Sinphinity.Models
{
    public class MidiStats
    {
        public int TotalTracks { get; set; }
        public int TotalChannels { get; set; }
        public int TotalEvents { get; set; }
        public int TotalNoteEvents { get; set; }
        public int TotalTempoChanges { get; set; }
        public int TotalPitchBendEvents { get; set; }
        public int TotalProgramChangeEvents { get; set; }
        public int TotalControlChangeEvents { get; set; }
        public int TotalSustainPedalEvents { get; set; }
        public int TotalChannelIndependentEvents { get; set; }
        public bool HasMoreThanOneInstrumentPerTrack { get; set; }
        public bool HasMoreThanOneChannelPerTrack { get; set; }
        public int TotalChordTracks { get; set; }
        public int TotalMelodicTracks { get; set; }
        public int TotalBassTracks { get; set; }
        public int TotalPercussionTracks { get; set; }
        public int TotalTracksWithoutNotes { get; set; }
        public int TotalInstruments { get; set; }
        public int TotalPercussionInstruments { get; set; }
    }
}
