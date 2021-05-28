using Sinphinity.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace SinphinityProcMidi.Midi
{
    public static partial class MidiUtilities
    {
        // Used to investigate issues. It allows to save a group of notes as a midi file that can be investigated using a tool
        // like Musescore
        public static void SaveNotesToMidiFile(string filePath, List<Note> notes)
        {
            var base64encodedMidi = GetMidiBytesFromNotes(notes);
            var bytes = Convert.FromBase64String(base64encodedMidi);

            using (var binWriter = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                binWriter.Write(bytes);
            }
        }
    }
}
