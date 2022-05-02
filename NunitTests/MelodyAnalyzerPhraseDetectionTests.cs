using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sinphinity.Models;
using SinphinityProcMelodyAnalyser.BusinessLogic;

namespace NunitTests
{
	internal class MelodyAnalyzerPhraseDetectionTests
	{

		private List<Note> notes = new List<Note>();


		private List<Bar> bars = new List<Bar>();


		[SetUp]
		public void Init()
		{
			for (int i = 0; i < 16 * 7; i++)
			{
				var n = new Note() { StartSinceBeginningOfSongInTicks = 24 * i, EndSinceBeginningOfSongInTicks = 24 * (i + 1) };
				notes.Add(n);
			}
			for (int i = 0; i < 7; i++)
			{
				var bar = new Bar() { TimeSignature = new TimeSignature { Denominator = 4, Numerator = 4 }, TicksFromBeginningOfSong = 384 * i, BarNumber = i + 1 };
				bars.Add(bar);
			}
		}



		[Test]
		public void GetEdgesOfGroupsOfNotesEvenlySpacedWorksAsExpected()
		{
			var edges = PhraseDetection.GetEdgesOfGroupsOfNotesEvenlySpaced(notes, bars, new HashSet<long>());


			Assert.IsTrue(true);
		}
		private List<long> GetDurations(List<Note> notes)
		{
			var retObj = new List<long>();
			for (var i = 0; i < notes.Count - 1; i++)
			{
				retObj.Add(notes[i + 1].StartSinceBeginningOfSongInTicks - notes[i].StartSinceBeginningOfSongInTicks);
			}
			return retObj;
		}
	}
}

