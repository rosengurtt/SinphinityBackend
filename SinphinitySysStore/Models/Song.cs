using System.Collections.Generic;

namespace SinphinitySysStore.Models
{
    public class Song 
    {
        public Song() { }
        public Song(SongInfo sInfo, SongData sData, SongMidi sMidi)
        {
            Id = sInfo.Id;
            SongInfoId = sInfo.Id;
            SongDataId = sData?.Id;
            SongMidiId = sMidi?.Id;
            Name = sInfo.Name;
            IsSongProcessed = sInfo.IsSongProcessed;
            IsMidiCorrect = sInfo.IsMidiCorrect;
            CantBeProcessed = sInfo.CantBeProcessed;
            Band = sInfo.Band;
            Style = sInfo.Style;
            MidiStats = sInfo.MidiStats;
            DurationInSeconds = sInfo.DurationInSeconds;
            DurationInTicks = sInfo.DurationInTicks;
            AverageTempoInBeatsPerMinute = sInfo.AverageTempoInBeatsPerMinute;
            SongSimplifications = sData?.SongSimplifications;
            Bars = sData?.Bars;
            TempoChanges = sData?.TempoChanges;
            MidiBase64Encoded = sMidi?.MidiBase64Encoded;
        }
        public string Id { get; set; }
        public string SongInfoId { get; set; }
        public string SongDataId { get; set; }
        public string SongMidiId { get; set; }
        public string Name { get; set; }
        public bool IsSongProcessed { get; set; }
        public bool IsMidiCorrect { get; set; }
        public bool CantBeProcessed { get; set; }
        public Band Band { get; set; }
        public Style Style { get; set; }
        public MidiStats MidiStats { get; set; }
        public long DurationInSeconds { get; set; }
        public long DurationInTicks { get; set; }
        public long AverageTempoInBeatsPerMinute { get; set; }
        public List<SongSimplification> SongSimplifications { get; set; }
        public List<Bar> Bars { get; set; }
        public List<TempoChange> TempoChanges { get; set; }
        public string MidiBase64Encoded { get; set; }
    }
}
