
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

		//[Test]
		//public void GetGroupsOfZeroZigZagIndexWorksAsExpected()
		//{
		//	notes.Clear();
		//	notes.Add(new Note { Pitch = 45, StartSinceBeginningOfSongInTicks = 0 });
		//	notes.Add(new Note { Pitch = 47, StartSinceBeginningOfSongInTicks = 10 });
		//	notes.Add(new Note { Pitch = 48, StartSinceBeginningOfSongInTicks = 20 });
		//	notes.Add(new Note { Pitch = 51, StartSinceBeginningOfSongInTicks = 30 });
		//	notes.Add(new Note { Pitch = 50, StartSinceBeginningOfSongInTicks = 40 });
		//	notes.Add(new Note { Pitch = 60, StartSinceBeginningOfSongInTicks = 50 });
		//	notes.Add(new Note { Pitch = 70, StartSinceBeginningOfSongInTicks = 60 });
		//	notes.Add(new Note { Pitch = 74, StartSinceBeginningOfSongInTicks = 70 });
		//	notes.Add(new Note { Pitch = 54, StartSinceBeginningOfSongInTicks = 80 });
		//	notes.Add(new Note { Pitch = 53, StartSinceBeginningOfSongInTicks = 90 });
		//	notes.Add(new Note { Pitch = 52, StartSinceBeginningOfSongInTicks = 100 });
		//	notes.Add(new Note { Pitch = 51, StartSinceBeginningOfSongInTicks = 110 });

		//	var groups = SegmentsDetection.GetGroupsOfZeroZigZagIndex(notes);
	}

}

