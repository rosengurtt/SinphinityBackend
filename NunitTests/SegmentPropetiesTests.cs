
using NUnit.Framework;
using Sinphinity.Models;
using SinphinityProcMelodyAnalyser.MelodyLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NunitTests
{
	public class SegmentPropetiesTests
	{
		private List<Note> notes = new List<Note>();



		[SetUp]
		public void Init()
		{
		}

		[Test]
		public void SegmentPropertiesAreCalculatedOK()
        {
			var p = new Phrase("48,48,96,48,48,96,48,48,96,96", "0,12,-14,0,12,-12,0,12,-14");

			var seg = new Segment(p);

			Assert.AreEqual("0,+,-,0,+,-,0,+,-", seg.PitchDirections);
			Assert.AreEqual(10, seg.TotalNotes);
			Assert.AreEqual(672, seg.DurationInTicks);
			Assert.AreEqual(6, seg.NoteDensity);
			Assert.AreEqual(10, seg.MaxDurationVariation);
			Assert.AreEqual(-4, seg.PitchStep);
			Assert.AreEqual(16, seg.PitchRange);
			Assert.AreEqual(3, seg.AbsPitchVariation);
			Assert.AreEqual(6, seg.RelPitchVariation);
			Assert.AreEqual(84, seg.AverageInterval);
			Assert.AreEqual(3, seg.Monotony);
		}


		[Test]
		public void MonotoneIndexIsCalculatedCorrectly()
		{
			notes.Clear();
			notes.Add(new Note { Pitch = 45, StartSinceBeginningOfSongInTicks = 0 });
			notes.Add(new Note { Pitch = 47, StartSinceBeginningOfSongInTicks = 10 });
			notes.Add(new Note { Pitch = 48, StartSinceBeginningOfSongInTicks = 20 });
			notes.Add(new Note { Pitch = 51, StartSinceBeginningOfSongInTicks = 30 });
			notes.Add(new Note { Pitch = 51, StartSinceBeginningOfSongInTicks = 40 });
			notes.Add(new Note { Pitch = 60, StartSinceBeginningOfSongInTicks = 50 });

			var sp = new SegmentProperties(notes);
			Assert.AreEqual(0, sp.ZigzagIndex);

			notes.Add(new Note { Pitch = 60, StartSinceBeginningOfSongInTicks = 5 });
			sp = new SegmentProperties(notes);
			Assert.AreEqual(0.4, sp.ZigzagIndex);

		}

	
	}

}

