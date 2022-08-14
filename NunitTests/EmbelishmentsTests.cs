//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Newtonsoft.Json;
//using Sinphinity.Models;
//using SinphinityProcMelodyAnalyser.BusinessLogic;

//namespace NunitTests
//{
//    internal class EmbelishmentsTests
//    {
//        private List<Note> notes;
//        private List<Bar> bars;
//        [SetUp]
//        public void Init()
//        {
//            notes = new List<Note>(new Note[]{
//                                         new Note { StartSinceBeginningOfSongInTicks=0,EndSinceBeginningOfSongInTicks=96, Pitch=64, Voice=1},
//                                         new Note { StartSinceBeginningOfSongInTicks=96,EndSinceBeginningOfSongInTicks=102, Pitch=54, Voice=1},
//                                         new Note { StartSinceBeginningOfSongInTicks=102,EndSinceBeginningOfSongInTicks=144, Pitch=63, Voice=1},
//                                         new Note { StartSinceBeginningOfSongInTicks=144,EndSinceBeginningOfSongInTicks=196, Pitch=63, Voice=1},
//                                         new Note { StartSinceBeginningOfSongInTicks=196,EndSinceBeginningOfSongInTicks=240, Pitch=61, Voice=1},
//                                         new Note { StartSinceBeginningOfSongInTicks=240,EndSinceBeginningOfSongInTicks=288, Pitch=61, Voice=1},
//                                         new Note { StartSinceBeginningOfSongInTicks=288,EndSinceBeginningOfSongInTicks=291, Pitch=59, Voice=1},
//                                         new Note { StartSinceBeginningOfSongInTicks=291,EndSinceBeginningOfSongInTicks=336, Pitch=54, Voice=1},
//                                         new Note { StartSinceBeginningOfSongInTicks=336,EndSinceBeginningOfSongInTicks=384, Pitch=59, Voice=1}});
//            bars = new List<Bar>(new Bar[] { new Bar { BarNumber = 1, TicksFromBeginningOfSong=0, TimeSignature = new TimeSignature { Denominator = 4, Numerator = 4 } } });
//        }

//        [Test]
//        public void RemoveEmbellishmentsWorks()
//        {
//            (var hasEmbellishments, var phraseWithoutEmbellishmentNotes) = EmbelishmentsDetection.GetPhraseWithoutEmbellishments(notes);

//            Assert.IsTrue(hasEmbellishments);
//            Assert.AreEqual(7, phraseWithoutEmbellishmentNotes.Count);
//            Assert.IsFalse(phraseWithoutEmbellishmentNotes.Any(x => x.DurationInTicks <= 6));
//        }

//        [Test]
//        public void GetPhraseBetweenEdgesWorksWithEmbellishedPhrases()
//        {
//           var phraseInfo= PhraseDetection.GetPhraseBetweenEdges(notes, 0, 384, 1, 1, bars);
//            Assert.AreEqual("0,102,42,52,44,51,45,48", phraseInfo?.MetricsAsString);
//            Assert.AreEqual("0,-1,0,-2,0,-7,5", phraseInfo?.PitchesAsString);
//            Assert.AreEqual("0,96,6,42,52,44,48,3,45,48", phraseInfo?.EmbellishedMetricsAsString);
//            Assert.AreEqual("0,-10,9,0,-2,0,-2,-5,5", phraseInfo?.EmbellishedPitchesAsString);

//        }


//    }
//}
