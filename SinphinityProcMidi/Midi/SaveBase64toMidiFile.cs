using Melanchall.DryWetMidi.Core;
using System;
using System.IO;

namespace SinphinityProcMidi.Midi
{

    public static partial class MidiUtilities
    {
        // Used to investigate issues. 
        public static void SaveBase64toMidiFile(string base64encodedMidi, string filePath)
        {
            var bytes = Convert.FromBase64String(base64encodedMidi);

            using (var binWriter = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                binWriter.Write(bytes);
            }
        }
        public static void SaveMidiFile(MidiFile file, string filePath)
        {
            var encoded = Base64EncodeMidiFile(file);
            SaveBase64toMidiFile(encoded, filePath);
        }


    }
}
