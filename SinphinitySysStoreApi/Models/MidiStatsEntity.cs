using Sinphinity.Models;

namespace SinphinitySysStore.Models
{
    public class MidiStatsEntity
    {
        public MidiStatsEntity() { }
        public MidiStatsEntity(MidiStats ms, Song song)
        {
            if (song.MidiStats == null)
                return;
            TotalTracks = ms.TotalTracks;
            TotalChannels = ms.TotalChannels;
            TotalEvents = ms.TotalEvents;
            TotalNoteEvents = ms.TotalNoteEvents;
            TotalTempoChanges = ms.TotalTempoChanges;
            TotalPitchBendEvents = ms.TotalPitchBendEvents;
            TotalProgramChangeEvents = ms.TotalProgramChangeEvents;
            TotalControlChangeEvents = ms.TotalControlChangeEvents;
            TotalSustainPedalEvents = ms.TotalSustainPedalEvents;
            TotalChannelIndependentEvents = ms.TotalChannelIndependentEvents;
            HasMoreThanOneInstrumentPerTrack = ms.HasMoreThanOneInstrumentPerTrack;
            HasMoreThanOneChannelPerTrack = ms.HasMoreThanOneChannelPerTrack;
            TotalChordTracks = ms.TotalChordTracks;
            TotalMelodicTracks = ms.TotalMelodicTracks;
            TotalBassTracks = ms.TotalBassTracks;
            TotalPercussionTracks = ms.TotalPercussionTracks;
            TotalTracksWithoutNotes = ms.TotalTracksWithoutNotes;
            TotalInstruments = ms.TotalInstruments;
            TotalPercussionInstruments = ms.TotalPercussionInstruments;
            DurationInTicks = ms.DurationInTicks;
            DurationInSeconds = ms.DurationInSeconds;
        }

        public long Id { get; set; }

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
        public long DurationInTicks { get; set; }
        public int DurationInSeconds { get; set; }

        public MidiStats AsMidiStats()
        {
            return new MidiStats
            {
                TotalTracks = this.TotalTracks,
                TotalChannels = this.TotalChannels,
                TotalEvents = this.TotalEvents,
                TotalNoteEvents = this.TotalNoteEvents,
                TotalTempoChanges = this.TotalTempoChanges,
                TotalPitchBendEvents = this.TotalPitchBendEvents,
                TotalProgramChangeEvents = this.TotalProgramChangeEvents,
                TotalControlChangeEvents = this.TotalControlChangeEvents,
                TotalSustainPedalEvents = this.TotalSustainPedalEvents,
                TotalChannelIndependentEvents = this.TotalChannelIndependentEvents,
                HasMoreThanOneInstrumentPerTrack = this.HasMoreThanOneInstrumentPerTrack,
                HasMoreThanOneChannelPerTrack = this.HasMoreThanOneChannelPerTrack,
                TotalChordTracks = this.TotalChordTracks,
                TotalMelodicTracks = this.TotalMelodicTracks,
                TotalBassTracks = this.TotalBassTracks,
                TotalPercussionTracks = this.TotalPercussionTracks,
                TotalTracksWithoutNotes = this.TotalTracksWithoutNotes,
                TotalInstruments = this.TotalInstruments,
                TotalPercussionInstruments = this.TotalPercussionInstruments,
                DurationInTicks = this.DurationInTicks,
                DurationInSeconds = this.DurationInSeconds
            };
        }
    }
}

