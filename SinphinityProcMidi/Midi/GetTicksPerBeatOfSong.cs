﻿using Melanchall.DryWetMidi.Core;
using System;

namespace SinphinityProcMidi.Midi
{
    public static partial class MidiUtilities
    {
        public static int GetTicksPerQuarterNote(string base64encodedMidiFile)
        {
            var midiFile = MidiFile.Read(base64encodedMidiFile);
            var ticksPerQuarter = midiFile.TimeDivision as TicksPerQuarterNoteTimeDivision;
            if (ticksPerQuarter != null) return ticksPerQuarter.TicksPerQuarterNote;
            else throw new Exception("The midi file doesn't have a value for TicksPerBeat");
        }
    }
}
