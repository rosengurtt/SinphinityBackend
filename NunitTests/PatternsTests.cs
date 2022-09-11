
using NUnit.Framework;
using Sinphinity.Models;
using SinphinityProcMelodyAnalyser.MelodyLogic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NunitTests
{
	public class Tests
	{
		private List<Note> notes = new List<Note>();



		[SetUp]
		public void Init()
		{
		}



		[Test]
		public void PatternsAreAsExpected()
		{
			notes.Clear();
			var pitchPattern1 = new List<int> { 3, -1, 4, -2, -2, 1, -3, 7, -1, -5, 3 };
			var pitchPattern2 = new List<int> { 3, -1, 4, -1, -2, 1, -4, 7, -1, -5, 3 };

			notes = AddRandomNotes(notes, 25);
			notes = AddPattern(notes, pitchPattern1);
			notes = AddRandomNotes(notes, 33);
			notes = AddPattern(notes, pitchPattern2);
			notes = AddRandomNotes(notes, 15);

			var edges = new HashSet<long> { 150, 300, 600, 880 };

			PatternDetection.GetNonContiguousPatterns(notes, PhraseTypeEnum.PitchDirectionAndMetrics, 10);

		}
		private List<Note> AddRandomNotes(List<Note> notes, int quant)
		{
			var random = new Random();
			var startTime = notes.Count > 0 ? notes[notes.Count - 1].EndSinceBeginningOfSongInTicks : 0;
			for (var i = 0; i < quant; i++)
			{
				notes.Add(new Note
				{
					StartSinceBeginningOfSongInTicks = startTime + i * 10,
					EndSinceBeginningOfSongInTicks = startTime + (i + 1) * 10,
					Pitch = (byte)random.Next(30, 70)
				});
			}
			return notes;
		}

		private List<Note> AddPattern(List<Note> notes, List<int> pattern)
		{
			var startTime = notes.Count > 0 ? notes[notes.Count - 1].EndSinceBeginningOfSongInTicks : 0;
			for (var i = 0; i <= pattern.Count; i++)
			{
				notes.Add(new Note
				{
					StartSinceBeginningOfSongInTicks = startTime + i * 10,
					EndSinceBeginningOfSongInTicks = startTime + (i + 1) * 10,
					Pitch = i == 0 ? (byte)50 : (byte)(50 + pattern[i - 1])
				});
			}
			return notes;
		}



		[Test]
		public void GetNotesAsStringWorksAsExpected()
		{
			notes.Clear();
			for (var i = 0; i < 8; i++)
			{
				notes.Add(new Note { StartSinceBeginningOfSongInTicks = i, EndSinceBeginningOfSongInTicks = i + 1, Pitch = (byte)(20 + i*3 ) });
			}
			var lolo = PatternDetection.GetNotesAsString(notes, PhraseTypeEnum.Pitches);
			var a = 9;
		}


	}
}
