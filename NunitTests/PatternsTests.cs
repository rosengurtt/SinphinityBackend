/*
using NUnit.Framework;
using Sinphinity.Models;
using System.Collections.Generic;
using System.Linq;

namespace NunitTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void IsMonotoneIsCalculatedCorrectly()
        {
            var asString = "(144,0)(192,0)(96,-4)(96,1)(240,1)(432,-2)(96,0)(144,1)(96,3)(144,0)(192,0)(96,-4)(192,1)(192,1)";
            var pat = new Pattern(asString);
            Assert.IsFalse(pat.IsMonotone);
        }

        [Test]
        public void BasicPatternIsExtractedCorrectlyFromPattern()
        {
            var asString = "(144,0)(192,0)(96,-4)(96,1)(240,1)(432,-2)(96,0)(144,1)(96,3)(144,0)(192,0)(96,-4)(192,1)(192,1)";
            var pat = new Pattern(asString);
            var basPat = new BasicPattern(pat);
            Assert.AreEqual("(3,0)(4,0)(2,-4)(2,1)(5,1)(9,-2)(2,0)(3,1)(2,3)(3,0)(4,0)(2,-4)(4,1)(4,1)", basPat.AsString);
        }

        [Test]
        public void RangeOfPatternIsCalculatedCorrectly()
        {
            var asString = "(144,-1)(192,-1)";
            var pat = new Pattern(asString);
            var basPat = new BasicPattern(pat);
            Assert.AreEqual(2, basPat.Range);
        }

        [Test]
        public void ExtractionWorksAsExpected()
        {
            var notes = new List<Note>() { new Note { StartSinceBeginningOfSongInTicks=0, EndSinceBeginningOfSongInTicks=24, Pitch=60},
                new Note { StartSinceBeginningOfSongInTicks=24, EndSinceBeginningOfSongInTicks=96, Pitch=62, Voice=0},
                new Note { StartSinceBeginningOfSongInTicks=96, EndSinceBeginningOfSongInTicks=192, Pitch=65, Voice=0},
                new Note { StartSinceBeginningOfSongInTicks=192, EndSinceBeginningOfSongInTicks=240, Pitch=60, Voice=0},
                new Note { StartSinceBeginningOfSongInTicks=240, EndSinceBeginningOfSongInTicks=288, Pitch=62, Voice=0},
                new Note { StartSinceBeginningOfSongInTicks=288, EndSinceBeginningOfSongInTicks=384, Pitch=69, Voice=0},
                new Note { StartSinceBeginningOfSongInTicks=384, EndSinceBeginningOfSongInTicks=432, Pitch=71, Voice=0},
                new Note { StartSinceBeginningOfSongInTicks=432, EndSinceBeginningOfSongInTicks=480, Pitch=72, Voice=0},
                new Note { StartSinceBeginningOfSongInTicks=480, EndSinceBeginningOfSongInTicks=576, Pitch=74, Voice=0}
            };
            var bars = new List<Bar>() { new Bar { BarNumber=1, TicksFromBeginningOfSong=0,
                    TimeSignature=new TimeSignature{ Denominator=4, Numerator=4 },
                    KeySignature=new KeySignature { key=0, scale= Sinphinity.Models.Enums.ScaleType.major} },
                new Bar { BarNumber=2, TicksFromBeginningOfSong=384,
                    TimeSignature=new TimeSignature{ Denominator=4, Numerator=4 },
                    KeySignature=new KeySignature { key=0, scale= Sinphinity.Models.Enums.ScaleType.major} },
                new Bar { BarNumber=3, TicksFromBeginningOfSong=768,
                    TimeSignature=new TimeSignature{ Denominator=4, Numerator=4 },
                    KeySignature=new KeySignature { key=0, scale= Sinphinity.Models.Enums.ScaleType.major} }
            };

            var extraction = PatternsExtraction.BuildSetOfPatterns(notes, bars, new HashSet<string>());

            Assert.Contains("(24,1)(72,0)", extraction.ToList());
            Assert.Contains("(24,1)(72,2)(96,0)", extraction.ToList());
            Assert.Contains("(72,2)(96,0)", extraction.ToList());
            Assert.Contains("(24,1)(72,2)(96,-3)(48,0)", extraction.ToList());
            Assert.Contains("(72,2)(96,-3)(48,0)", extraction.ToList());
            Assert.Contains("(24,1)(72,2)(96,-3)(48,1)(48,0)", extraction.ToList());
        }

        [Test]
        public void RemovalOfPatternsThatAreRepetitionsOfOtherPatternsWorksCorrectly()
        {
            var patToRemove1 = "(24,1)(24,-1)(24,1)(24,-1)";
            var patToRemove2 = "(12,2)(24,-3)(36,1)(12,2)(24,-3)(36,1)(12,2)(24,-3)(36,1)";
            var patToKeep1 = "(6,1)(12,2)(6,1)(12,2)(6,1)(12,2)";
            var patToKeep2 = "(6,1)(6,1)(6,1)(6,1)(6,1)(6,1)(6,1)";
            var patToKeep3 = "(12,0)(12,0)";

            var tree = new HashSet<string>();
            tree.Add(patToRemove1);
            tree.Add(patToRemove2);
            tree.Add(patToKeep1);
            tree.Add(patToKeep2);
            tree.Add(patToKeep3);

            var result = PatternsExtraction.RemovePatternsTharAreArepetitionOfAnotherPattern(tree);
            Assert.IsFalse(result.ToList().Contains(patToRemove1));
            Assert.IsFalse(result.ToList().Contains(patToRemove2));
            Assert.IsTrue(result.ToList().Contains(patToKeep1));
            Assert.IsTrue(result.ToList().Contains(patToKeep2));
            Assert.IsTrue(result.ToList().Contains(patToKeep3));
        }
        [Test]
        public void RemovalOfPatternsWithlongNotesWorksCorrectly()
        {
            var patToRemove1 = "(2400,1)(24,-1)(24,1)(24,-1)";
            var patToRemove2 = "(10,2)(24,-3)(36,1)(1000,2)(24,-3)(36,1)(12,2)(24,-3)(36,1)";
            var patToKeep1 = "(6,1)(12,2)(6,1)(12,2)(6,1)(12,2)";
            var patToKeep2 = "(12,0)(12,0)";

            var tree = new HashSet<string>();
            tree.Add(patToRemove1);
            tree.Add(patToRemove2);
            tree.Add(patToKeep1);
            tree.Add(patToKeep2);


            var result = PatternsExtraction.RemovePatternsThatHaveVeryLongNotes(tree);
            Assert.IsFalse(result.ToList().Contains(patToRemove1));
            Assert.IsFalse(result.ToList().Contains(patToRemove2));
            Assert.IsTrue(result.ToList().Contains(patToKeep1));
            Assert.IsTrue(result.ToList().Contains(patToKeep2));
        }
    }
}
*/