namespace Sinphinity.Models
{
    public enum PhraseTypeEnum
    {
        Metrics = 0,
        Pitches = 1,
        Both = 2,
        EmbelishedMetrics = 3,
        EmbelishedPitches = 4,
        EmbellishedBoth = 5,
        // Possible values are +, - and 0, indicating going up, down or staying. So if we had this relatives pitches 3,-4,2,5,0,0,-2 it would be +,-,+,+,0,0,- when considered as of PitchDirection type
        PitchDirection = 7
    }
}
